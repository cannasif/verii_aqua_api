using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.FishGrowths.Application.Dtos;
using aqua_api.Modules.FishGrowths.Application.Services;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class OpeningImportShipmentLedgerIntegrationTests
    : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public OpeningImportShipmentLedgerIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Commit_AppliesOpeningShipmentToLedger_AndGrowthUsesRemainingFish()
    {
        const string projectCode = "PRJ-OPEN-SHIP-LEDGER";
        const string cageCode = "CAGE-OPEN-SHIP-LEDGER";
        const string batchCode = "BATCH-OPEN-SHIP-LEDGER";

        var request = new OpeningImportPreviewRequestDto
        {
            FileName = "opening-shipment-ledger.xlsx",
            SourceSystem = "integration-test",
            Sheets =
            [
                Sheet("Projects", new()
                {
                    ["projectCode"] = projectCode,
                    ["projectName"] = "Opening Shipment Ledger Project",
                    ["startDate"] = "2026-01-01",
                }),
                Sheet("Cages", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["cageName"] = "Opening Shipment Ledger Cage",
                    ["assignedDate"] = "2026-01-01",
                }),
                Sheet("OpeningStock", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["fishCount"] = "1000",
                    ["averageGram"] = "100",
                    ["asOfDate"] = "2026-01-01",
                }),
                Sheet("OpeningMortality", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["deadCount"] = "100",
                    ["mortalityBiomassKg"] = "10",
                    ["mortalityDate"] = "2026-01-20",
                }),
                Sheet("OpeningShipments", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["shipmentDate"] = "2026-01-10",
                    ["fishCount"] = "300",
                    ["averageGram"] = "120",
                    ["currencyCode"] = "TRY",
                    ["exchangeRate"] = "1",
                    ["unitPrice"] = "0",
                }),
            ]
        };

        long jobId;
        using (var previewScope = _factory.Services.CreateScope())
        {
            var service = previewScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var preview = await service.PreviewAsync(request);
            Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
            Assert.Equal(0, preview.Data!.Summary.ErrorRows);
            jobId = preview.Data.JobId;
        }

        using (var commitScope = _factory.Services.CreateScope())
        {
            var service = commitScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var commit = await service.CommitAsync(jobId);
            Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
            Assert.Equal(1, commit.Data!.CreatedShipmentLines);
        }

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var project = await db.Projects.SingleAsync(x => x.ProjectCode == projectCode);
        var projectCage = await db.ProjectCages
            .Include(x => x.Cage)
            .SingleAsync(x => x.ProjectId == project.Id && x.Cage!.CageCode == cageCode);
        var batch = await db.FishBatches
            .SingleAsync(x => x.ProjectId == project.Id && x.BatchCode == batchCode);
        var balance = await db.BatchCageBalances
            .SingleAsync(x => x.ProjectCageId == projectCage.Id && x.FishBatchId == batch.Id);

        Assert.Equal(600, balance.LiveCount);
        Assert.Equal(60_000m, balance.BiomassGram);
        Assert.Equal(100m, balance.AverageGram);
        Assert.Equal(100m, batch.CurrentAverageGram);
        Assert.Equal(new DateTime(2026, 1, 20), balance.AsOfDate.Date);

        var shipmentLine = await db.ShipmentLines
            .Include(x => x.Shipment)
            .SingleAsync(x => x.FishBatchId == batch.Id);
        Assert.StartsWith("OPENING_IMPORT:", shipmentLine.ErpSourceMovementKey);

        var shipmentMovement = await db.BatchMovements.SingleAsync(x =>
            x.MovementType == BatchMovementType.Shipment &&
            x.ReferenceTable == "RII_SHIPMENT_LINE" &&
            x.ReferenceId == shipmentLine.Id);
        Assert.Equal(-300, shipmentMovement.SignedCount);
        Assert.Equal(-30_000m, shipmentMovement.SignedBiomassGram);
        Assert.Equal(-36_000m, shipmentMovement.ReportedBiomassGram);
        Assert.Equal(100m, shipmentMovement.FromAverageGram);
        Assert.Equal(projectCage.Id, shipmentMovement.FromProjectCageId);

        var mortalityMovement = await db.BatchMovements.SingleAsync(x =>
            x.MovementType == BatchMovementType.Mortality &&
            x.FishBatchId == batch.Id);
        Assert.Equal(-100, mortalityMovement.SignedCount);
        Assert.Equal(-10_000m, mortalityMovement.SignedBiomassGram);
        Assert.Equal(-10_000m, mortalityMovement.ReportedBiomassGram);
        Assert.Equal(100m, mortalityMovement.FromAverageGram);

        var dashboardService = verifyScope.ServiceProvider.GetRequiredService<IDashboardProjectReportService>();
        var dashboard = await dashboardService.GetProjectDetailAsync(project.Id);
        Assert.True(dashboard.Success, $"{dashboard.Message} | {dashboard.ExceptionMessage}");
        var dashboardCage = Assert.Single(dashboard.Data!.Cages);
        Assert.Equal(600, dashboardCage.CurrentFishCount);
        Assert.Equal(100m, dashboardCage.CurrentAverageGram);
        Assert.Equal(60_000m, dashboardCage.CurrentBiomassGram);

        var summaries = await dashboardService.GetProjectSummariesAsync([project.Id]);
        Assert.True(summaries.Success, $"{summaries.Message} | {summaries.ExceptionMessage}");
        var summaryCage = Assert.Single(Assert.Single(summaries.Data!.Projects).Cages);
        Assert.Equal(100m, summaryCage.CurrentAverageGram);

        var devirService = verifyScope.ServiceProvider.GetRequiredService<IDevirFcrReportService>();
        var devir = await devirService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = [project.Id],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 31),
        });
        Assert.True(devir.Success, $"{devir.Message} | {devir.ExceptionMessage}");
        var devirRow = Assert.Single(devir.Data!.Rows);
        Assert.Equal(1_000, devirRow.OpeningFishCount);
        Assert.Equal(300, devirRow.ShipmentFishCount);
        Assert.Equal(100, devirRow.MortalityFishCount);
        Assert.Equal(600, devirRow.EndingFishCount);
        Assert.Equal(100m, devirRow.EndingAverageGram);
        Assert.Equal(60m, devirRow.EndingBiomassKg);
        Assert.Equal(36m, devirRow.ShippedBiomassKg);
        Assert.Equal(5m, devirRow.MortalityBiomassKg);

        var growthService = verifyScope.ServiceProvider.GetRequiredService<IFishGrowthService>();
        shipmentMovement.IsDeleted = true;
        shipmentMovement.DeletedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var reconciledGrowth = await growthService.CreateAsync(new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCage.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 2, 15),
            NewAverageGram = 110m,
        }, 1);
        Assert.True(
            reconciledGrowth.Success,
            $"{reconciledGrowth.Message} | {reconciledGrowth.ExceptionMessage}");
        Assert.Equal(600, reconciledGrowth.Data!.FishCount);
        Assert.Equal(100m, reconciledGrowth.Data.PreviousAverageGram);
        Assert.Equal(66_000m, reconciledGrowth.Data.NewBiomassGram);

        var reconstructedMovement = await db.BatchMovements.SingleAsync(x =>
            x.MovementType == BatchMovementType.Shipment
            && x.ReferenceTable == "RII_SHIPMENT_LINE"
            && x.ReferenceId == shipmentLine.Id);
        Assert.Equal(-300, reconstructedMovement.SignedCount);
        Assert.Equal(-30_000m, reconstructedMovement.SignedBiomassGram);
        Assert.Equal(-36_000m, reconstructedMovement.ReportedBiomassGram);
        var reconciledBalance = await db.BatchCageBalances.SingleAsync(x =>
            x.ProjectCageId == projectCage.Id && x.FishBatchId == batch.Id);
        Assert.Equal(600, reconciledBalance.LiveCount);
        Assert.Equal(110m, reconciledBalance.AverageGram);
        Assert.Equal(66_000m, reconciledBalance.BiomassGram);

        // Legacy data can contain case-variant references and zero-count biomass
        // correction rows. Reconciliation must repair the linked aggregate instead
        // of creating a second shipment exit.
        db.ChangeTracker.Clear();
        reconstructedMovement = await db.BatchMovements.SingleAsync(x =>
            x.MovementType == BatchMovementType.Shipment
            && x.ReferenceTable == "RII_SHIPMENT_LINE"
            && x.ReferenceId == shipmentLine.Id);
        reconstructedMovement.ReferenceTable = "RII_ShipmentLine";
        reconstructedMovement.SignedCount = -250;
        reconstructedMovement.SignedBiomassGram = -30_000m;
        db.BatchMovements.Add(new BatchMovement
        {
            FishBatchId = batch.Id,
            ProjectCageId = projectCage.Id,
            FromProjectCageId = projectCage.Id,
            FromStockId = batch.FishStockId,
            ToStockId = batch.FishStockId,
            FromAverageGram = 120m,
            MovementDate = shipmentLine.Shipment!.ShipmentDate,
            MovementType = BatchMovementType.Shipment,
            SignedCount = 0,
            SignedBiomassGram = -5_000m,
            ActorUserId = 1,
            ReferenceTable = "rii_shipment_line",
            ReferenceId = shipmentLine.Id,
            Note = "Legacy shipment biomass correction",
            CreatedBy = 1,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        var legacyLedger = await db.BatchMovements
            .Where(x => x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id)
            .ToListAsync();
        Assert.Equal(650, legacyLedger.Sum(x => x.SignedCount));
        Assert.Equal(61_000m, legacyLedger.Sum(x => x.SignedBiomassGram));

        var updatedGrowth = await growthService.UpdateAsync(
            reconciledGrowth.Data.Id,
            new UpdateFishGrowthDto { NewAverageGram = 120m },
            1);
        Assert.True(
            updatedGrowth.Success,
            $"{updatedGrowth.Message} | {updatedGrowth.ExceptionMessage}");

        var shipmentMovements = (await db.BatchMovements
                .Where(x =>
                    x.MovementType == BatchMovementType.Shipment
                    && x.ReferenceId == shipmentLine.Id)
                .ToListAsync())
            .Where(x => string.Equals(
                x.ReferenceTable,
                "RII_SHIPMENT_LINE",
                StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.Equal(2, shipmentMovements.Count);
        Assert.Equal(-300, shipmentMovements.Sum(x => x.SignedCount));
        Assert.Equal(-30_000m, shipmentMovements.Sum(x => x.SignedBiomassGram));
        Assert.Equal(-36_000m, shipmentMovements.Sum(x => x.ReportedBiomassGram ?? 0m));
        Assert.All(shipmentMovements, x => Assert.Equal("RII_SHIPMENT_LINE", x.ReferenceTable));

        var updatedBalance = await db.BatchCageBalances.SingleAsync(x =>
            x.ProjectCageId == projectCage.Id && x.FishBatchId == batch.Id);
        Assert.Equal(600, updatedBalance.LiveCount);
        Assert.Equal(120m, updatedBalance.AverageGram);
        Assert.Equal(72_000m, updatedBalance.BiomassGram);
    }

    [Fact]
    public async Task Commit_WithoutOpeningStock_DerivesGrossReceiptThenAppliesHistoricalExitsOnce()
    {
        const string projectCode = "PRJ-DERIVED-OPENING-GROSS";
        const string cageCode = "CAGE-DERIVED-OPENING-GROSS";
        const string batchCode = "BATCH-DERIVED-OPENING-GROSS";
        var request = new OpeningImportPreviewRequestDto
        {
            FileName = "derived-opening-gross.xlsx",
            SourceSystem = "integration-test",
            Sheets =
            [
                Sheet("Projects", new()
                {
                    ["projectCode"] = projectCode,
                    ["projectName"] = "Derived Opening Gross Project",
                    ["startDate"] = "2026-01-01",
                }),
                Sheet("Cages", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["cageName"] = "Derived Opening Gross Cage",
                    ["assignedDate"] = "2026-01-01",
                }),
                Sheet("OpeningGoodsReceipts", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["receiptNo"] = "DERIVED-OPENING-REC-001",
                    ["receiptDate"] = "2026-01-01",
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["fishCount"] = "1000",
                    ["averageGram"] = "800",
                }),
                Sheet("OpeningMortality", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["deadCount"] = "100",
                    ["mortalityBiomassKg"] = "80",
                    ["mortalityDate"] = "2026-02-01",
                }),
                Sheet("OpeningShipments", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["shipmentDate"] = "2026-03-01",
                    ["fishCount"] = "200",
                    ["averageGram"] = "800",
                    ["currencyCode"] = "TRY",
                    ["exchangeRate"] = "1",
                    ["unitPrice"] = "50",
                }),
            ]
        };

        long jobId;
        using (var previewScope = _factory.Services.CreateScope())
        {
            var service = previewScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var preview = await service.PreviewAsync(request);
            Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
            Assert.Equal(0, preview.Data!.Summary.ErrorRows);
            jobId = preview.Data.JobId;
        }

        using (var commitScope = _factory.Services.CreateScope())
        {
            var service = commitScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var commit = await service.CommitAsync(jobId);
            Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        }

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var project = await db.Projects.SingleAsync(x => x.ProjectCode == projectCode);
        var projectCage = await db.ProjectCages
            .Include(x => x.Cage)
            .SingleAsync(x => x.ProjectId == project.Id && x.Cage!.CageCode == cageCode);
        var batch = await db.FishBatches
            .SingleAsync(x => x.ProjectId == project.Id && x.BatchCode == batchCode);
        var balance = await db.BatchCageBalances
            .SingleAsync(x => x.ProjectCageId == projectCage.Id && x.FishBatchId == batch.Id);

        Assert.Equal(700, balance.LiveCount);
        Assert.Equal(800m, balance.AverageGram);
        Assert.Equal(560_000m, balance.BiomassGram);

        var devirService = verifyScope.ServiceProvider.GetRequiredService<IDevirFcrReportService>();
        var devir = await devirService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = [project.Id],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 3, 31),
        });
        Assert.True(devir.Success, $"{devir.Message} | {devir.ExceptionMessage}");
        var row = Assert.Single(devir.Data!.Rows);
        Assert.Equal(1_000, row.OpeningFishCount);
        Assert.Equal(200, row.ShipmentFishCount);
        Assert.Equal(100, row.MortalityFishCount);
        Assert.Equal(700, row.EndingFishCount);
        Assert.Equal(800m, row.EndingAverageGram);
        Assert.Equal(560m, row.EndingBiomassKg);
        Assert.Equal(160m, row.ShippedBiomassKg);
        Assert.Equal(40m, row.MortalityBiomassKg);
        Assert.Equal(760m, row.ProducedBiomassKg);
    }

    [Fact]
    public async Task Commit_ProductionScaleOpeningHistory_PreservesOpeningGramAndRemainingStockMass()
    {
        const string projectCode = "PRJ-OPEN-HISTORY-MASS";
        const string cageCode = "CAGE-OPEN-HISTORY-MASS";
        const string batchCode = "BATCH-OPEN-HISTORY-MASS";
        var request = new OpeningImportPreviewRequestDto
        {
            FileName = "opening-history-mass.xlsx",
            SourceSystem = "integration-test",
            Sheets =
            [
                Sheet("Projects", new()
                {
                    ["projectCode"] = projectCode,
                    ["projectName"] = "Opening History Mass Project",
                    ["startDate"] = "2026-03-01",
                }),
                Sheet("Cages", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["cageName"] = "Opening History Mass Cage",
                    ["assignedDate"] = "2026-03-01",
                }),
                Sheet("OpeningStock", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["fishCount"] = "1064000",
                    ["averageGram"] = "830",
                    ["asOfDate"] = "2026-03-01",
                }),
                Sheet("OpeningMortality", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["deadCount"] = "181042",
                    ["mortalityBiomassKg"] = "75132.44",
                    ["mortalityDate"] = "2026-03-20",
                }),
                Sheet("OpeningShipments", new()
                {
                    ["projectCode"] = projectCode,
                    ["cageCode"] = cageCode,
                    ["batchCode"] = batchCode,
                    ["fishStockCode"] = "PLAMUT-5G",
                    ["shipmentDate"] = "2026-03-10",
                    ["fishCount"] = "864655",
                    ["averageGram"] = "638.241842",
                    ["currencyCode"] = "TRY",
                    ["exchangeRate"] = "1",
                    ["unitPrice"] = "0",
                }),
            ]
        };

        long jobId;
        using (var previewScope = _factory.Services.CreateScope())
        {
            var service = previewScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var preview = await service.PreviewAsync(request);
            Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
            Assert.Equal(0, preview.Data!.Summary.ErrorRows);
            jobId = preview.Data.JobId;
        }

        using (var commitScope = _factory.Services.CreateScope())
        {
            var service = commitScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var commit = await service.CommitAsync(jobId);
            Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        }

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var project = await db.Projects.SingleAsync(x => x.ProjectCode == projectCode);
        var projectCage = await db.ProjectCages
            .Include(x => x.Cage)
            .SingleAsync(x => x.ProjectId == project.Id && x.Cage!.CageCode == cageCode);
        var batch = await db.FishBatches
            .SingleAsync(x => x.ProjectId == project.Id && x.BatchCode == batchCode);
        var balance = await db.BatchCageBalances
            .SingleAsync(x => x.ProjectCageId == projectCage.Id && x.FishBatchId == batch.Id);

        Assert.Equal(18_303, balance.LiveCount);
        Assert.Equal(830m, balance.AverageGram);
        Assert.Equal(15_191_490m, balance.BiomassGram);
        Assert.Equal(830m, batch.CurrentAverageGram);

        var dashboardService = verifyScope.ServiceProvider.GetRequiredService<IDashboardProjectReportService>();
        var dashboard = await dashboardService.GetProjectDetailAsync(project.Id);
        Assert.True(dashboard.Success, $"{dashboard.Message} | {dashboard.ExceptionMessage}");
        var dashboardCage = Assert.Single(dashboard.Data!.Cages);
        Assert.Equal(18_303, dashboardCage.CurrentFishCount);
        Assert.Equal(830m, dashboardCage.CurrentAverageGram);
        Assert.Equal(15_191_490m, dashboardCage.CurrentBiomassGram);

        var growthService = verifyScope.ServiceProvider.GetRequiredService<IFishGrowthService>();
        var openingTimeline = await growthService.GetTimelineAsync(
            projectCage.Id,
            batch.Id,
            2026,
            3);
        Assert.True(openingTimeline.Success, $"{openingTimeline.Message} | {openingTimeline.ExceptionMessage}");
        Assert.Equal(830m, openingTimeline.Data!.LatestAverageGram);
        var openingMonth = Assert.Single(openingTimeline.Data.Months);
        Assert.Equal(18_303, openingMonth.FishCount);
        Assert.Equal(830m, openingMonth.EndAverageGram);

        var growth = await growthService.CreateAsync(new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCage.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 4, 15),
            NewAverageGram = 850m,
        }, 1);
        Assert.True(growth.Success, $"{growth.Message} | {growth.ExceptionMessage}");
        Assert.Equal(18_303, growth.Data!.FishCount);
        Assert.Equal(830m, growth.Data.PreviousAverageGram);
        Assert.Equal(15_557_550m, growth.Data.NewBiomassGram);

        var grownTimeline = await growthService.GetTimelineAsync(
            projectCage.Id,
            batch.Id,
            2026,
            4);
        Assert.True(grownTimeline.Success, $"{grownTimeline.Message} | {grownTimeline.ExceptionMessage}");
        Assert.Equal(850m, grownTimeline.Data!.LatestAverageGram);
        Assert.Equal(850m, grownTimeline.Data.Months.Single(x => x.Month == 4).EndAverageGram);
    }

    [Fact]
    public async Task Commit_SynchronizesBatchAverageAcrossMultipleOpeningCages()
    {
        const string projectCode = "PRJ-OPEN-WEIGHTED-GRAM";
        const string batchCode = "BATCH-OPEN-WEIGHTED-GRAM";
        var request = new OpeningImportPreviewRequestDto
        {
            FileName = "opening-weighted-gram.xlsx",
            SourceSystem = "integration-test",
            Sheets =
            [
                Sheet("Projects", new()
                {
                    ["projectCode"] = projectCode,
                    ["projectName"] = "Opening Weighted Gram Project",
                    ["startDate"] = "2026-01-01",
                }),
                SheetRows("Cages",
                    new()
                    {
                        ["projectCode"] = projectCode,
                        ["cageCode"] = "CAGE-OPEN-WEIGHTED-1",
                        ["cageName"] = "Opening Weighted Cage 1",
                        ["assignedDate"] = "2026-01-01",
                    },
                    new()
                    {
                        ["projectCode"] = projectCode,
                        ["cageCode"] = "CAGE-OPEN-WEIGHTED-2",
                        ["cageName"] = "Opening Weighted Cage 2",
                        ["assignedDate"] = "2026-01-01",
                    }),
                SheetRows("OpeningStock",
                    new()
                    {
                        ["projectCode"] = projectCode,
                        ["cageCode"] = "CAGE-OPEN-WEIGHTED-1",
                        ["batchCode"] = batchCode,
                        ["fishStockCode"] = "PLAMUT-5G",
                        ["fishCount"] = "100",
                        ["averageGram"] = "100",
                        ["asOfDate"] = "2026-01-01",
                    },
                    new()
                    {
                        ["projectCode"] = projectCode,
                        ["cageCode"] = "CAGE-OPEN-WEIGHTED-2",
                        ["batchCode"] = batchCode,
                        ["fishStockCode"] = "PLAMUT-5G",
                        ["fishCount"] = "300",
                        ["averageGram"] = "200",
                        ["asOfDate"] = "2026-01-01",
                    }),
            ]
        };

        long jobId;
        using (var previewScope = _factory.Services.CreateScope())
        {
            var service = previewScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var preview = await service.PreviewAsync(request);
            Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
            Assert.Equal(0, preview.Data!.Summary.ErrorRows);
            jobId = preview.Data.JobId;
        }

        using (var commitScope = _factory.Services.CreateScope())
        {
            var service = commitScope.ServiceProvider.GetRequiredService<IOpeningImportService>();
            var commit = await service.CommitAsync(jobId);
            Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        }

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var project = await db.Projects.SingleAsync(x => x.ProjectCode == projectCode);
        var batch = await db.FishBatches.SingleAsync(x => x.ProjectId == project.Id && x.BatchCode == batchCode);
        var balances = await db.BatchCageBalances
            .Where(x => x.FishBatchId == batch.Id && !x.IsDeleted)
            .ToListAsync();

        Assert.Equal(2, balances.Count);
        Assert.Equal(400, balances.Sum(x => x.LiveCount));
        Assert.Equal(70_000m, balances.Sum(x => x.BiomassGram));
        Assert.Equal(175m, batch.CurrentAverageGram);
    }

    private static OpeningImportSheetPayloadDto Sheet(
        string sheetName,
        Dictionary<string, string?> row)
        => SheetRows(sheetName, row);

    private static OpeningImportSheetPayloadDto SheetRows(
        string sheetName,
        params Dictionary<string, string?>[] rows)
    {
        return new OpeningImportSheetPayloadDto
        {
            SheetName = sheetName,
            Rows = rows.ToList(),
            Mappings = rows[0].Keys
                .Select(key => new OpeningImportFieldMappingDto
                {
                    SourceColumn = key,
                    TargetField = key,
                })
                .ToList(),
        };
    }
}
