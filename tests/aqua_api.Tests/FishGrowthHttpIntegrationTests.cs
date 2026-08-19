using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.FishGrowths.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.Mortalities.Application.Dtos;
using aqua_api.Modules.Shipments.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class FishGrowthHttpIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public FishGrowthHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Timeline_ShowsRecordedAndCarriedForwardMonths_FromCageEntry()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-TL-{suffix}",
                ProjectName = "Fish Growth Timeline Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"GTL-{suffix}", CageName = "Timeline Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"GTL-B-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                100_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Timeline opening",
                "TEST_TIMELINE_OPENING",
                1,
                null,
                projectCage.Id,
                stockId,
                stockId,
                100m,
                100m,
                1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        using var createResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 2, 18),
            NewAverageGram = 200m
        });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(created?.Success, created?.ExceptionMessage);

        using var timelineResponse = await client.GetAsync(
            $"/api/aqua/FishGrowth/timeline?projectCageId={projectCageId}&fishBatchId={fishBatchId}&throughYear=2026&throughMonth=3");
        Assert.Equal(HttpStatusCode.OK, timelineResponse.StatusCode);
        var timelineBody = await timelineResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthTimelineDto>>();
        Assert.True(timelineBody?.Success, timelineBody?.ExceptionMessage);
        var timeline = timelineBody!.Data!;

        Assert.Equal(new DateTime(2026, 1, 1), timeline.StartPeriod);
        Assert.Equal(new DateTime(2026, 3, 1), timeline.EndPeriod);
        Assert.Equal(100m, timeline.InitialAverageGram);
        Assert.Equal(200m, timeline.LatestAverageGram);
        Assert.Equal(1, timeline.RecordedMonthCount);
        Assert.Equal(1, timeline.CarriedForwardMonthCount);
        Assert.False(timeline.HasContinuityIssue);

        var january = Assert.Single(timeline.Months, x => x.Period == new DateTime(2026, 1, 1));
        Assert.Equal("Baseline", january.Status);
        Assert.Equal(100m, january.EndAverageGram);
        Assert.Equal(1_000, january.FishCount);

        var february = Assert.Single(timeline.Months, x => x.Period == new DateTime(2026, 2, 1));
        Assert.Equal("Recorded", february.Status);
        Assert.Equal(100m, february.PreviousAverageGram);
        Assert.Equal(100m, february.GrowthGram);
        Assert.Equal(200m, february.EndAverageGram);
        Assert.Equal(1_000, february.FishCount);

        var march = Assert.Single(timeline.Months, x => x.Period == new DateTime(2026, 3, 1));
        Assert.Equal("CarriedForward", march.Status);
        Assert.Equal(0m, march.GrowthGram);
        Assert.Equal(200m, march.EndAverageGram);
        Assert.Equal(1_000, march.FishCount);
        Assert.Equal(new DateTime(2026, 2, 1), march.CarriedFromPeriod);

        using var deleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{created!.Data!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        using var timelineAfterDeleteResponse = await client.GetAsync(
            $"/api/aqua/FishGrowth/timeline?projectCageId={projectCageId}&fishBatchId={fishBatchId}&throughYear=2026&throughMonth=3");
        var timelineAfterDelete = (await timelineAfterDeleteResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthTimelineDto>>())!.Data!;
        Assert.Equal(0, timelineAfterDelete.RecordedMonthCount);
        Assert.Equal(100m, timelineAfterDelete.LatestAverageGram);

        using var monthlyAfterDeleteResponse = await client.GetAsync(
            $"/api/aqua/FishGrowth/monthly?projectCageId={projectCageId}&fishBatchId={fishBatchId}&year=2026&month=2");
        var monthlyAfterDelete = await monthlyAfterDeleteResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthDto?>>();
        Assert.True(monthlyAfterDelete?.Success, monthlyAfterDelete?.ExceptionMessage);
        Assert.Null(monthlyAfterDelete!.Data);
    }

    [Fact]
    public async Task PostedShipmentLine_Delete_ReplaysLaterOperationsAndKeepsErpIntegratedLineLocked()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;
        long warehouseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            warehouseId = await db.Warehouses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"SHIP-DEL-{suffix}",
                ProjectName = "Posted Shipment Delete Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"SD-{suffix}", CageName = "Shipment Delete Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"SD-B-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                100_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Shipment delete opening",
                "TEST_SHIPMENT_DELETE_OPENING",
                1,
                null,
                projectCage.Id,
                stockId,
                stockId,
                100m,
                100m,
                1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        await PostGrowth(new DateTime(2026, 2, 1), 200m);

        ShipmentLineDto deletedShipmentLine;
        using (var shipmentResponse = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 2, 15),
            FishBatchId = fishBatchId,
            FromProjectCageId = projectCageId,
            TargetWarehouseId = warehouseId,
            FishCount = 100,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 10m
        }))
        {
            Assert.Equal(HttpStatusCode.OK, shipmentResponse.StatusCode);
            var shipmentBody = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(shipmentBody?.Success, shipmentBody?.ExceptionMessage);
            deletedShipmentLine = shipmentBody!.Data!;
        }
        Assert.Equal(200m, deletedShipmentLine.AverageGram);
        Assert.Equal(20_000m, deletedShipmentLine.BiomassGram);
        Assert.Equal(10m, deletedShipmentLine.UnitPrice);
        Assert.Equal(200m, deletedShipmentLine.LineAmount);

        using (var postedScope = _factory.Services.CreateScope())
        {
            var db = postedScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var shipment = await db.Shipments.SingleAsync(x => x.Id == deletedShipmentLine.ShipmentId);
            var cageBalance = await db.BatchCageBalances.SingleAsync(x =>
                !x.IsDeleted && x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
            var warehouseBalance = await db.BatchWarehouseBalances.SingleAsync(x =>
                !x.IsDeleted && x.ProjectId == projectId && x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId);

            Assert.Equal(DocumentStatus.Posted, shipment.Status);
            Assert.Equal(900, cageBalance.LiveCount);
            Assert.Equal(200m, cageBalance.AverageGram);
            Assert.Equal(180_000m, cageBalance.BiomassGram);
            Assert.Equal(100, warehouseBalance.LiveCount);
            Assert.Equal(200m, warehouseBalance.AverageGram);
            Assert.Equal(20_000m, warehouseBalance.BiomassGram);
        }

        using (var mortalityResponse = await client.PostAsJsonAsync("/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 2, 20),
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 10
        }))
        {
            Assert.Equal(HttpStatusCode.OK, mortalityResponse.StatusCode);
        }

        await PostGrowth(new DateTime(2026, 3, 1), 300m);

        using (var deleteResponse = await client.DeleteAsync($"/api/aqua/ShipmentLine/{deletedShipmentLine.Id}"))
        {
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.True(deleteBody?.Success, deleteBody?.ExceptionMessage);
        }

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var cageBalance = await db.BatchCageBalances.SingleAsync(x =>
                !x.IsDeleted && x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
            var warehouseBalance = await db.BatchWarehouseBalances.SingleAsync(x =>
                !x.IsDeleted && x.ProjectId == projectId && x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId);
            var marchGrowth = await db.FishGrowths.SingleAsync(x =>
                !x.IsDeleted && x.FishBatchId == fishBatchId && x.GrowthYear == 2026 && x.GrowthMonth == 3);

            Assert.Equal(990, cageBalance.LiveCount);
            Assert.Equal(300m, cageBalance.AverageGram);
            Assert.Equal(297_000m, cageBalance.BiomassGram);
            Assert.Equal(0, warehouseBalance.LiveCount);
            Assert.Equal(0m, warehouseBalance.BiomassGram);
            Assert.Equal(200m, marchGrowth.PreviousAverageGram);
            Assert.Equal(300m, marchGrowth.NewAverageGram);
            Assert.True(await db.ShipmentLines.IgnoreQueryFilters().AnyAsync(x => x.Id == deletedShipmentLine.Id && x.IsDeleted));
            Assert.False(await db.BatchMovements.AnyAsync(x =>
                !x.IsDeleted
                && x.MovementType == BatchMovementType.Shipment
                && x.ReferenceTable == "RII_SHIPMENT_LINE"
                && x.ReferenceId == deletedShipmentLine.Id));
        }

        using (var timelineResponse = await client.GetAsync(
                   $"/api/aqua/FishGrowth/timeline?projectCageId={projectCageId}&fishBatchId={fishBatchId}&throughYear=2026&throughMonth=3"))
        {
            Assert.Equal(HttpStatusCode.OK, timelineResponse.StatusCode);
            var timeline = (await timelineResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthTimelineDto>>())!.Data!;
            Assert.False(timeline.HasContinuityIssue);
            Assert.Equal(300m, timeline.LatestAverageGram);
        }

        using (var reportResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-shipments", new MonthlyOperationalReportRequestDto
        {
            FromDate = new DateTime(2026, 2, 1),
            ToDate = new DateTime(2026, 3, 31),
            ProjectIds = [projectId],
            ProjectCageIds = [projectCageId]
        }))
        {
            Assert.Equal(HttpStatusCode.OK, reportResponse.StatusCode);
            var report = (await reportResponse.Content.ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
            Assert.Equal(0, report.TotalCount);
            Assert.Equal(0m, report.TotalKg);
        }

        using (var dashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}"))
        {
            Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
            var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
            var dashboardCage = Assert.Single(dashboard.Cages);
            Assert.Equal(990, dashboardCage.CurrentFishCount);
            Assert.Equal(300m, dashboardCage.CurrentAverageGram);
            Assert.Equal(297_000m, dashboardCage.CurrentBiomassGram);
        }

        ShipmentLineDto erpLockedLine;
        using (var shipmentResponse = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 3, 10),
            FishBatchId = fishBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 10
        }))
        {
            var shipmentBody = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(shipmentBody?.Success, shipmentBody?.ExceptionMessage);
            erpLockedLine = shipmentBody!.Data!;
        }
        using (var postResponse = await client.PostAsync($"/api/aqua/posting/shipment/{erpLockedLine.ShipmentId}", null))
        {
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        }
        using (var erpScope = _factory.Services.CreateScope())
        {
            var db = erpScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var shipment = await db.Shipments.SingleAsync(x => x.Id == erpLockedLine.ShipmentId);
            shipment.IsERPIntegrated = true;
            await db.SaveChangesAsync();
        }
        using (var blockedDeleteResponse = await client.DeleteAsync($"/api/aqua/ShipmentLine/{erpLockedLine.Id}"))
        {
            Assert.Equal(HttpStatusCode.BadRequest, blockedDeleteResponse.StatusCode);
            var blockedDelete = await blockedDeleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.False(blockedDelete?.Success);
            Assert.Contains("ERP", blockedDelete!.ExceptionMessage, StringComparison.OrdinalIgnoreCase);
        }

        using var finalScope = _factory.Services.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.True(await finalDb.ShipmentLines.AnyAsync(x => x.Id == erpLockedLine.Id && !x.IsDeleted));

        async Task<FishGrowthDto> PostGrowth(DateTime growthDate, decimal targetGram)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
            {
                ProjectId = projectId,
                ProjectCageId = projectCageId,
                FishBatchId = fishBatchId,
                GrowthDate = growthDate,
                NewAverageGram = targetGram
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return body!.Data!;
        }
    }

    [Fact]
    public async Task Timeline_UsesCountAndBiomassLedger_ForOperationalAverageChanges()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-LEDGER-{suffix}",
                ProjectName = "Fish Growth Ledger Timeline Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"GLT-{suffix}", CageName = "Ledger Timeline Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"GLT-B-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                100_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Timeline opening",
                "TEST_TIMELINE_LEDGER_OPENING",
                1,
                null,
                projectCage.Id,
                stockId,
                stockId,
                100m,
                100m,
                1);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                -100,
                -5_000m,
                BatchMovementType.Mortality,
                new DateTime(2026, 2, 15),
                "Low-weight mortality",
                "TEST_TIMELINE_LEDGER_MORTALITY",
                2,
                projectCage.Id,
                null,
                stockId,
                stockId,
                50m,
                50m,
                1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        using var createResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 3, 20),
            NewAverageGram = 200m
        });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        using var timelineResponse = await client.GetAsync(
            $"/api/aqua/FishGrowth/timeline?projectCageId={projectCageId}&fishBatchId={fishBatchId}&throughYear=2026&throughMonth=3");
        Assert.Equal(HttpStatusCode.OK, timelineResponse.StatusCode);
        var timelineBody = await timelineResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthTimelineDto>>();
        Assert.True(timelineBody?.Success, timelineBody?.ExceptionMessage);
        var timeline = timelineBody!.Data!;

        var february = Assert.Single(timeline.Months, x => x.Period == new DateTime(2026, 2, 1));
        Assert.Equal("OperationallyAdjusted", february.Status);
        Assert.Equal(100m, february.PreviousAverageGram);
        Assert.Equal(105.556m, february.EndAverageGram);
        Assert.Equal(5.556m, february.OperationalAverageChangeGram);
        Assert.Equal(900, february.FishCount);

        var march = Assert.Single(timeline.Months, x => x.Period == new DateTime(2026, 3, 1));
        Assert.Equal("Recorded", march.Status);
        Assert.Equal(105.556m, march.PreviousAverageGram);
        Assert.Equal(105.556m, march.ExpectedPreviousAverageGram);
        Assert.Equal(94.444m, march.GrowthGram);
        Assert.Equal(200m, march.EndAverageGram);
        Assert.False(march.HasContinuityIssue);
        Assert.False(timeline.HasContinuityIssue);
        Assert.Equal(1, timeline.CarriedForwardMonthCount);
    }

    [Fact]
    public async Task Create_GrowsCurrentCageBatch_AndRejectsSecondGrowthInSameMonth()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-{suffix}",
                ProjectName = "Fish Growth Integration Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"GC-{suffix}", CageName = "Growth Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"GB-{suffix}",
                CurrentAverageGram = 720m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                720_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Opening stocking",
                "TEST_OPENING",
                1,
                null,
                projectCage.Id,
                stockId,
                stockId,
                720m,
                720m,
                1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        var request = new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 7, 15),
            NewAverageGram = 840m
        };

        using var firstResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(firstBody?.Success, firstBody?.ExceptionMessage);
        Assert.Equal(720m, firstBody!.Data!.PreviousAverageGram);
        Assert.Equal(120m, firstBody.Data.GrowthGram);
        Assert.Equal(16.6667m, firstBody.Data.GrowthRatePercent);
        Assert.Equal(840m, firstBody.Data.NewAverageGram);
        Assert.Equal(1_000, firstBody.Data.FishCount);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x => x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(1_000, balance.LiveCount);
            Assert.Equal(840m, balance.AverageGram);
            Assert.Equal(840_000m, balance.BiomassGram);

            var movement = await db.BatchMovements.SingleAsync(x => x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id);
            Assert.Equal(BatchMovementType.FishGrowth, movement.MovementType);
            Assert.Equal(0, movement.SignedCount);
            Assert.Equal(120_000m, movement.SignedBiomassGram);
        }

        using var dashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        var dashboardBody = await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>();
        var dashboardGrowthDay = dashboardBody!.Data!.Cages.Single(x => x.ProjectCageId == projectCageId)
            .DailyRows.Single(x => x.Date == "2026-07-01");
        Assert.Equal(1, dashboardGrowthDay.FishGrowthCount);
        Assert.NotEmpty(dashboardGrowthDay.FishGrowthDetails);
        Assert.Equal(0, dashboardGrowthDay.StockConvertCount);

        using var projectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectDetailResponse.StatusCode);
        var projectDetailBody = await projectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>();
        var projectGrowthDay = projectDetailBody!.Data!.Cages.Single(x => x.ProjectCageId == projectCageId)
            .DailyRows.Single(x => x.Date == "2026-07-01");
        Assert.Equal(1, projectGrowthDay.FishGrowthCount);
        Assert.NotEmpty(projectGrowthDay.FishGrowthDetails);
        Assert.Equal(0, projectGrowthDay.StockConvertCount);

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/aqua/FishGrowth/{firstBody.Data.Id}",
            new UpdateFishGrowthDto
            {
                NewAverageGram = 800m,
                Description = "Corrected monthly target"
            });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updateBody = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(updateBody?.Success, updateBody?.ExceptionMessage);
        Assert.Equal(firstBody.Data.Id, updateBody!.Data!.Id);
        Assert.Equal(720m, updateBody.Data.PreviousAverageGram);
        Assert.Equal(80m, updateBody.Data.GrowthGram);
        Assert.Equal(800m, updateBody.Data.NewAverageGram);
        Assert.Equal(800_000m, updateBody.Data.NewBiomassGram);
        Assert.Equal("Corrected monthly target", updateBody.Data.Description);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x =>
                x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(1_000, balance.LiveCount);
            Assert.Equal(800m, balance.AverageGram);
            Assert.Equal(800_000m, balance.BiomassGram);

            var movement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id);
            Assert.Equal(new DateTime(2026, 7, 1), movement.MovementDate);
            Assert.Equal(720m, movement.FromAverageGram);
            Assert.Equal(800m, movement.ToAverageGram);
            Assert.Equal(80_000m, movement.SignedBiomassGram);
            Assert.Contains("toAvg=800", movement.Note);
        }

        using var updatedDashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, updatedDashboardResponse.StatusCode);
        var updatedDashboard = (await updatedDashboardResponse.Content
            .ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var updatedDashboardCage = Assert.Single(updatedDashboard.Cages);
        Assert.Equal(800m, updatedDashboardCage.CurrentAverageGram);
        Assert.Equal(800_000m, updatedDashboardCage.CurrentBiomassGram);

        using var updatedProjectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, updatedProjectDetailResponse.StatusCode);
        var updatedProjectDetail = (await updatedProjectDetailResponse.Content
            .ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>())!.Data!;
        var updatedProjectDetailCage = Assert.Single(updatedProjectDetail.Cages);
        Assert.Equal(800m, updatedProjectDetailCage.CurrentAverageGram);
        Assert.Equal(800_000m, updatedProjectDetailCage.CurrentBiomassGram);

        request.GrowthDate = new DateTime(2026, 7, 28);
        request.NewAverageGram = 850m;
        using var duplicateResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        var duplicateBody = await duplicateResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.False(duplicateBody?.Success);
        Assert.Equal(
            "Bu kafes ve balık partisi için seçilen ayda büyütme kaydı zaten bulunmaktadır.",
            duplicateBody!.Message);
        Assert.Equal(duplicateBody.Message, duplicateBody.ExceptionMessage);

        using var deleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{firstBody.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.True(deleteBody?.Success);
        Assert.True(deleteBody!.Data);

        using var monthlyAfterDeleteResponse = await client.GetAsync(
            $"/api/aqua/FishGrowth/monthly?projectCageId={projectCageId}&fishBatchId={fishBatchId}&year=2026&month=7");
        Assert.Equal(HttpStatusCode.OK, monthlyAfterDeleteResponse.StatusCode);
        var monthlyAfterDelete = await monthlyAfterDeleteResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthDto?>>();
        Assert.Null(monthlyAfterDelete!.Data);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var revertedBalance = await db.BatchCageBalances.SingleAsync(x =>
                x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(720m, revertedBalance.AverageGram);
            Assert.Equal(720_000m, revertedBalance.BiomassGram);
            Assert.Empty(await db.FishGrowths.Where(x => x.Id == firstBody.Data.Id).ToListAsync());
            Assert.Empty(await db.BatchMovements.Where(x =>
                x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id).ToListAsync());
            Assert.True(await db.FishGrowths.IgnoreQueryFilters().AnyAsync(x =>
                x.Id == firstBody.Data.Id && x.IsDeleted));
            Assert.True(await db.BatchMovements.IgnoreQueryFilters().AnyAsync(x =>
                x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id && x.IsDeleted));
        }

        request.NewAverageGram = 730m;
        using var recreateResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.OK, recreateResponse.StatusCode);
        var recreateBody = await recreateResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(recreateBody?.Success, recreateBody?.ExceptionMessage);
        Assert.Equal(730m, recreateBody!.Data!.NewAverageGram);

        using var invalidUpdateResponse = await client.PutAsJsonAsync(
            $"/api/aqua/FishGrowth/{recreateBody.Data.Id}",
            new UpdateFishGrowthDto { NewAverageGram = 720m });
        var invalidUpdateBody = await invalidUpdateResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(
            invalidUpdateResponse.StatusCode == HttpStatusCode.BadRequest,
            $"{invalidUpdateResponse.StatusCode} | {invalidUpdateBody?.Message} | {invalidUpdateBody?.ExceptionMessage}");
        Assert.Contains("büyütme öncesi gramajdan büyük", invalidUpdateBody!.Message);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                projectId,
                fishBatchId,
                projectCageId,
                -100,
                -73_000m,
                BatchMovementType.Mortality,
                new DateTime(2026, 7, 20),
                "Post-growth mortality",
                "TEST_LATER_MORTALITY",
                1,
                projectCageId,
                null,
                null,
                null,
                730m,
                730m,
                1);
            await db.SaveChangesAsync();
        }

        using var driftedBalanceUpdateResponse = await client.PutAsJsonAsync(
            $"/api/aqua/FishGrowth/{recreateBody.Data.Id}",
            new UpdateFishGrowthDto { NewAverageGram = 740m });
        Assert.Equal(HttpStatusCode.OK, driftedBalanceUpdateResponse.StatusCode);
        var driftedBalanceUpdateBody = await driftedBalanceUpdateResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(driftedBalanceUpdateBody?.Success, driftedBalanceUpdateBody?.ExceptionMessage);
        Assert.Equal(740m, driftedBalanceUpdateBody!.Data!.NewAverageGram);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x =>
                x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(900, balance.LiveCount);
            Assert.Equal(740m, balance.AverageGram);
            Assert.Equal(BatchMath.CalculateBiomassGram(900, 740m), balance.BiomassGram);
            var mortalityMovement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "TEST_LATER_MORTALITY");
            Assert.Equal(-74_000m, mortalityMovement.SignedBiomassGram);
        }

        using var allowedUpdateResponse = await client.PutAsJsonAsync(
            $"/api/aqua/FishGrowth/{recreateBody.Data.Id}",
            new UpdateFishGrowthDto { NewAverageGram = 750m });
        Assert.Equal(HttpStatusCode.OK, allowedUpdateResponse.StatusCode);
        var allowedUpdateBody = await allowedUpdateResponse.Content
            .ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(allowedUpdateBody?.Success, allowedUpdateBody?.ExceptionMessage);
        Assert.Equal(750m, allowedUpdateBody!.Data!.NewAverageGram);

        using var allowedDeleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{recreateBody.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, allowedDeleteResponse.StatusCode);
        var allowedDeleteBody = await allowedDeleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.True(allowedDeleteBody?.Success, allowedDeleteBody?.ExceptionMessage);
        Assert.True(allowedDeleteBody!.Data);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.Equal(0, await verifyDb.FishGrowths.CountAsync(x => x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId));
        var finalBalance = await verifyDb.BatchCageBalances.SingleAsync(x =>
            x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
        Assert.Equal(900, finalBalance.LiveCount);
        Assert.Equal(720m, finalBalance.AverageGram);
        Assert.Equal(648_000m, finalBalance.BiomassGram);
    }

    [Fact]
    public async Task UpdateAndDelete_ReplayMortalityShipmentAndFeeding_InOneTransaction()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;
        long growthId;
        long shipmentLineId;
        long mortalityId;
        long feedingDistributionId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var fishStockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var feedStockId = await db.Stocks
                .Where(x => x.ErpStockCode == "YEM-STD")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-REPLAY-{suffix}",
                ProjectName = "Growth Replay Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"GR-{suffix}", CageName = "Growth Replay Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = fishStockId,
                BatchCode = $"GR-B-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                100_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Opening stocking",
                "TEST_REPLAY_OPENING",
                1,
                null,
                projectCage.Id,
                fishStockId,
                fishStockId,
                100m,
                100m,
                1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        using (var createResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 2, 15),
            NewAverageGram = 200m
        }))
        {
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
            Assert.True(createBody?.Success, createBody?.ExceptionMessage);
            growthId = createBody!.Data!.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var feedStockId = await db.Stocks
                .Where(x => x.ErpStockCode == "YEM-STD")
                .Select(x => x.Id)
                .SingleAsync();

            var mortality = new Mortality
            {
                ProjectId = projectId,
                MortalityNo = $"MORT-REPLAY-{growthId}",
                MortalityDate = new DateTime(2026, 2, 10),
                Status = DocumentStatus.Posted
            };
            var feeding = new Feeding
            {
                ProjectId = projectId,
                FeedingNo = $"FEED-REPLAY-{growthId}",
                FeedingDate = new DateTime(2026, 2, 15),
                FeedingSlot = FeedingSlot.Morning,
                SourceType = FeedingSourceType.Manual,
                Status = DocumentStatus.Posted
            };
            var shipment = new Shipment
            {
                ProjectId = projectId,
                ShipmentNo = $"SHIP-REPLAY-{growthId}",
                ShipmentDate = new DateTime(2026, 2, 20),
                Status = DocumentStatus.Posted
            };
            db.Mortalities.Add(mortality);
            db.Feedings.Add(feeding);
            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();

            var mortalityLine = new MortalityLine
            {
                MortalityId = mortality.Id,
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                DeadCount = 100
            };
            var feedingLine = new FeedingLine
            {
                FeedingId = feeding.Id,
                StockId = feedStockId,
                QtyUnit = 5m,
                GramPerUnit = 1_000m,
                TotalGram = 5_000m
            };
            var shipmentLine = new ShipmentLine
            {
                ShipmentId = shipment.Id,
                FishBatchId = fishBatchId,
                FromProjectCageId = projectCageId,
                FishCount = 100,
                AverageGram = 200m,
                BiomassGram = 20_000m,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                UnitPrice = 10m,
                LocalUnitPrice = 10m,
                LineAmount = 200m,
                LocalLineAmount = 200m
            };
            db.MortalityLines.Add(mortalityLine);
            db.FeedingLines.Add(feedingLine);
            db.ShipmentLines.Add(shipmentLine);
            await db.SaveChangesAsync();

            var distribution = new FeedingDistribution
            {
                FeedingLineId = feedingLine.Id,
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                FeedGram = 5_000m
            };
            db.FeedingDistributions.Add(distribution);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                projectId,
                fishBatchId,
                projectCageId,
                -100,
                -20_000m,
                BatchMovementType.Mortality,
                mortality.MortalityDate,
                "Mortality",
                "RII_MORTALITY",
                mortality.Id,
                projectCageId,
                null,
                null,
                null,
                200m,
                200m,
                1);
            await ledger.ApplyDelta(
                projectId,
                fishBatchId,
                projectCageId,
                -100,
                -20_000m,
                BatchMovementType.Shipment,
                shipment.ShipmentDate,
                "Shipment",
                "RII_SHIPMENT",
                shipment.Id,
                projectCageId,
                null,
                null,
                null,
                200m,
                null,
                1);
            db.BatchMovements.Add(new BatchMovement
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                MovementDate = feeding.FeedingDate,
                MovementType = BatchMovementType.Feeding,
                FeedGram = distribution.FeedGram,
                ReferenceTable = "RII_FEEDING_DISTRIBUTION",
                ReferenceId = distribution.Id,
                CreatedBy = 1
            });
            await db.SaveChangesAsync();

            shipmentLineId = shipmentLine.Id;
            mortalityId = mortality.Id;
            feedingDistributionId = distribution.Id;
        }

        using (var updateResponse = await client.PutAsJsonAsync(
                   $"/api/aqua/FishGrowth/{growthId}",
                   new UpdateFishGrowthDto { NewAverageGram = 250m }))
        {
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
            Assert.Equal(800, balance.LiveCount);
            Assert.Equal(250m, balance.AverageGram);
            Assert.Equal(200_000m, balance.BiomassGram);

            var mortalityMovement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "RII_MORTALITY" && x.ReferenceId == mortalityId);
            Assert.Equal(-25_000m, mortalityMovement.SignedBiomassGram);
            var shipmentLine = await db.ShipmentLines.SingleAsync(x => x.Id == shipmentLineId);
            Assert.Equal(250m, shipmentLine.AverageGram);
            Assert.Equal(25_000m, shipmentLine.BiomassGram);
            Assert.Equal(250m, shipmentLine.LineAmount);
            var feedingMovement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "RII_FEEDING_DISTRIBUTION"
                && x.ReferenceId == feedingDistributionId);
            Assert.Equal(5_000m, feedingMovement.FeedGram);
        }

        using (var deleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{growthId}"))
        {
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
            Assert.Equal(800, balance.LiveCount);
            Assert.Equal(100m, balance.AverageGram);
            Assert.Equal(80_000m, balance.BiomassGram);

            var mortalityMovement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "RII_MORTALITY" && x.ReferenceId == mortalityId);
            Assert.Equal(-10_000m, mortalityMovement.SignedBiomassGram);
            var shipmentLine = await db.ShipmentLines.SingleAsync(x => x.Id == shipmentLineId);
            Assert.Equal(100m, shipmentLine.AverageGram);
            Assert.Equal(10_000m, shipmentLine.BiomassGram);
            Assert.Equal(100m, shipmentLine.LineAmount);
            var feedingMovement = await db.BatchMovements.SingleAsync(x =>
                x.ReferenceTable == "RII_FEEDING_DISTRIBUTION"
                && x.ReferenceId == feedingDistributionId);
            Assert.Equal(5_000m, feedingMovement.FeedGram);
        }
    }

    [Fact]
    public async Task Update_RollsBackEntireReplay_WhenLaterGrowthBecomesInvalid()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var stockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-ROLLBACK-{suffix}",
                ProjectName = "Growth Rollback Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"RB-{suffix}", CageName = "Rollback Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();
            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"RB-B-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();
            await ledger.ApplyDelta(
                project.Id,
                batch.Id,
                projectCage.Id,
                1_000,
                100_000m,
                BatchMovementType.Stocking,
                project.StartDate,
                "Opening stocking",
                "TEST_ROLLBACK_OPENING",
                1,
                null,
                projectCage.Id,
                stockId,
                stockId,
                100m,
                100m,
                1);
            await db.SaveChangesAsync();
            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        async Task<FishGrowthDto> CreateGrowth(DateTime date, decimal gram)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
            {
                ProjectId = projectId,
                ProjectCageId = projectCageId,
                FishBatchId = fishBatchId,
                GrowthDate = date,
                NewAverageGram = gram
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>())!.Data!;
        }

        var january = await CreateGrowth(new DateTime(2026, 1, 20), 200m);
        var february = await CreateGrowth(new DateTime(2026, 2, 20), 210m);

        long shipmentLineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var shipment = new Shipment
            {
                ProjectId = projectId,
                ShipmentNo = $"ROLLBACK-SHIP-{suffix}",
                ShipmentDate = new DateTime(2026, 1, 15),
                Status = DocumentStatus.Posted,
                CreatedBy = 1,
                IsDeleted = false
            };
            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();
            var shipmentLine = new ShipmentLine
            {
                ShipmentId = shipment.Id,
                FishBatchId = fishBatchId,
                FromProjectCageId = projectCageId,
                FishCount = 100,
                AverageGram = 200m,
                BiomassGram = 20_000m,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CreatedBy = 1,
                IsDeleted = false
            };
            db.ShipmentLines.Add(shipmentLine);
            await db.SaveChangesAsync();
            shipmentLineId = shipmentLine.Id;
        }

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/aqua/FishGrowth/{january.Id}",
            new UpdateFishGrowthDto { NewAverageGram = 220m });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var januaryAfterRollback = await verifyDb.FishGrowths.SingleAsync(x => x.Id == january.Id);
        var februaryAfterRollback = await verifyDb.FishGrowths.SingleAsync(x => x.Id == february.Id);
        var balanceAfterRollback = await verifyDb.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
        Assert.Equal(200m, januaryAfterRollback.NewAverageGram);
        Assert.Equal(210m, februaryAfterRollback.NewAverageGram);
        Assert.Equal(210m, balanceAfterRollback.AverageGram);
        Assert.Equal(210_000m, balanceAfterRollback.BiomassGram);
        Assert.False(await verifyDb.BatchMovements.AnyAsync(x =>
            x.MovementType == BatchMovementType.Shipment
            && x.ReferenceTable == "RII_SHIPMENT_LINE"
            && x.ReferenceId == shipmentLineId));
    }

    [Fact]
    public async Task TargetGram_IsEffectiveForWholeMonth_AndAllReportsUseHistoricalWeight()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var fishStockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var feedStockId = await db.Stocks.Where(x => x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"MONTH-GROW-{suffix}",
                ProjectName = "Monthly Target Gram Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"MC-{suffix}", CageName = "Monthly Growth Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = fishStockId,
                BatchCode = $"MB-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, projectCage.Id, 1_000, 100_000m,
                BatchMovementType.Stocking, project.StartDate, "Opening stocking", "TEST_OPENING", 1,
                null, projectCage.Id, fishStockId, fishStockId, 100m, 100m, 1);

            foreach (var (date, feedGram) in new[]
                     {
                         (new DateTime(2026, 1, 15), 50_000m),
                         (new DateTime(2026, 2, 15), 60_000m),
                         (new DateTime(2026, 3, 15), 70_000m)
                     })
            {
                var feeding = new Feeding
                {
                    ProjectId = project.Id,
                    FeedingNo = $"MONTH-FEED-{date:yyyyMM}-{suffix}",
                    FeedingDate = date,
                    FeedingSlot = FeedingSlot.Morning,
                    SourceType = FeedingSourceType.Manual,
                    Status = DocumentStatus.Posted
                };
                db.Feedings.Add(feeding);
                await db.SaveChangesAsync();

                var line = new FeedingLine
                {
                    FeedingId = feeding.Id,
                    StockId = feedStockId,
                    QtyUnit = feedGram / 1_000m,
                    GramPerUnit = 1_000m,
                    TotalGram = feedGram
                };
                db.FeedingLines.Add(line);
                await db.SaveChangesAsync();
                db.FeedingDistributions.Add(new FeedingDistribution
                {
                    FeedingLineId = line.Id,
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    FeedGram = feedGram
                });
            }

            await db.SaveChangesAsync();
            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        var februaryGrowth = await PostGrowth(new DateTime(2026, 2, 26), 200m);
        Assert.Equal(100m, februaryGrowth.PreviousAverageGram);
        Assert.Equal(100m, februaryGrowth.GrowthGram);
        Assert.Equal(200m, februaryGrowth.NewAverageGram);

        var marchGrowth = await PostGrowth(new DateTime(2026, 3, 3), 300m);
        Assert.Equal(200m, marchGrowth.PreviousAverageGram);
        Assert.Equal(100m, marchGrowth.GrowthGram);
        Assert.Equal(300m, marchGrowth.NewAverageGram);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var growthMovements = await db.BatchMovements
                .Where(x => x.FishBatchId == fishBatchId && x.MovementType == BatchMovementType.FishGrowth)
                .OrderBy(x => x.MovementDate)
                .ToListAsync();
            Assert.Collection(
                growthMovements,
                row =>
                {
                    Assert.Equal(new DateTime(2026, 2, 1), row.MovementDate);
                    Assert.Equal(100m, row.FromAverageGram);
                    Assert.Equal(200m, row.ToAverageGram);
                    Assert.Equal(100_000m, row.SignedBiomassGram);
                },
                row =>
                {
                    Assert.Equal(new DateTime(2026, 3, 1), row.MovementDate);
                    Assert.Equal(200m, row.FromAverageGram);
                    Assert.Equal(300m, row.ToAverageGram);
                    Assert.Equal(100_000m, row.SignedBiomassGram);
                });
        }

        var january = await GetDevirFcr(new DateTime(2026, 1, 31));
        Assert.Equal(1_000, january.EndingFishCount);
        Assert.Equal(100m, january.EndingAverageGram);
        Assert.Equal(100m, january.EndingBiomassKg);
        Assert.Equal(50m, january.TotalFeedKg);
        Assert.Equal(0.5m, january.Fcr);

        var february = await GetDevirFcr(new DateTime(2026, 2, 1));
        Assert.Equal(1_000, february.EndingFishCount);
        Assert.Equal(200m, february.EndingAverageGram);
        Assert.Equal(200m, february.EndingBiomassKg);

        var februaryEnd = await GetDevirFcr(new DateTime(2026, 2, 28));
        Assert.Equal(110m, februaryEnd.TotalFeedKg);
        Assert.Equal(0.55m, februaryEnd.Fcr);

        var march = await GetDevirFcr(new DateTime(2026, 3, 31));
        Assert.Equal(1_000, march.EndingFishCount);
        Assert.Equal(300m, march.EndingAverageGram);
        Assert.Equal(300m, march.EndingBiomassKg);
        Assert.Equal(180m, march.TotalFeedKg);
        Assert.Equal(0.6m, march.Fcr);

        using var dashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var dashboardCage = Assert.Single(dashboard.Cages);
        Assert.Equal(1_000, dashboardCage.CurrentFishCount);
        Assert.Equal(300m, dashboardCage.CurrentAverageGram);
        Assert.Equal(300_000m, dashboardCage.CurrentBiomassGram);
        Assert.Equal(1, dashboardCage.DailyRows.Single(x => x.Date == "2026-02-01").FishGrowthCount);
        Assert.Equal(1, dashboardCage.DailyRows.Single(x => x.Date == "2026-03-01").FishGrowthCount);

        using var summaryResponse = await client.PostAsJsonAsync(
            "/api/aqua/dashboard-project/summary",
            new DashboardProjectsRequestDto { ProjectIds = [projectId] });
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = Assert.Single((await summaryResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectsResponseDto>>())!.Data!.Projects);
        Assert.Equal(1_000, summary.CageFish);
        Assert.Equal(300m, summary.MeasurementAverageGram);
        Assert.Equal(300_000m, summary.CageBiomassGram);
        Assert.Equal(0.6m, summary.Fcr);

        using var rawKpiResponse = await client.GetAsync($"/api/kpi-report/raw-kpi/{projectId}");
        Assert.Equal(HttpStatusCode.OK, rawKpiResponse.StatusCode);
        var rawKpi = (await rawKpiResponse.Content.ReadFromJsonAsync<ApiResponse<RawKpiReportDto>>())!.Data!;
        Assert.Equal(1_000, rawKpi.LiveFish);
        Assert.Equal(100m, rawKpi.InitialAverageGram);
        Assert.Equal(300m, rawKpi.CurrentAverageGram);
        Assert.Equal(300m, rawKpi.CurrentBiomassKg);
        Assert.Equal(0.9m, rawKpi.Fcr);

        using var projectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectDetailResponse.StatusCode);
        var projectDetail = (await projectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>())!.Data!;
        var projectDetailCage = Assert.Single(projectDetail.Cages);
        Assert.Equal(1_000, projectDetailCage.CurrentFishCount);
        Assert.Equal(300m, projectDetailCage.CurrentAverageGram);
        Assert.Equal(300_000m, projectDetailCage.CurrentBiomassGram);
        Assert.Equal(1, projectDetailCage.DailyRows.Single(x => x.Date == "2026-02-01").FishGrowthCount);
        Assert.Equal(1, projectDetailCage.DailyRows.Single(x => x.Date == "2026-03-01").FishGrowthCount);

        using (var shipmentResponse = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 3, 20),
            FishBatchId = fishBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 100,
            AverageGram = 1m,
            BiomassGram = 1m
        }))
        {
            Assert.Equal(HttpStatusCode.OK, shipmentResponse.StatusCode);
            var shipmentBody = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(shipmentBody?.Success, shipmentBody?.ExceptionMessage);
            Assert.Equal(300m, shipmentBody!.Data!.AverageGram);
            Assert.Equal(30_000m, shipmentBody.Data.BiomassGram);

            using var postResponse = await client.PostAsync($"/api/aqua/posting/shipment/{shipmentBody.Data.ShipmentId}", null);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        }

        using (var mortalityResponse = await client.PostAsJsonAsync("/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 3, 21),
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 10
        }))
        {
            Assert.Equal(HttpStatusCode.OK, mortalityResponse.StatusCode);
            var mortalityBody = await mortalityResponse.Content.ReadFromJsonAsync<ApiResponse<MortalityLineDto>>();
            Assert.True(mortalityBody?.Success, mortalityBody?.ExceptionMessage);
        }

        var marchAfterOutputs = await GetDevirFcr(new DateTime(2026, 3, 31));
        Assert.Equal(890, marchAfterOutputs.EndingFishCount);
        Assert.Equal(300m, marchAfterOutputs.EndingAverageGram);
        Assert.Equal(267m, marchAfterOutputs.EndingBiomassKg);
        Assert.Equal(100, marchAfterOutputs.ShipmentFishCount);
        Assert.Equal(30m, marchAfterOutputs.ShippedBiomassKg);
        Assert.Equal(10, marchAfterOutputs.MortalityFishCount);
        Assert.Equal(1.5m, marchAfterOutputs.MortalityBiomassKg);
        Assert.Equal(0.603m, marchAfterOutputs.Fcr);

        var reportRequest = new MonthlyOperationalReportRequestDto
        {
            FromDate = new DateTime(2026, 3, 1),
            ToDate = new DateTime(2026, 3, 31),
            ProjectIds = [projectId],
            ProjectCageIds = [projectCageId]
        };
        using var shipmentReportResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-shipments", reportRequest);
        Assert.Equal(HttpStatusCode.OK, shipmentReportResponse.StatusCode);
        var shipmentReport = (await shipmentReportResponse.Content.ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(100, shipmentReport.TotalCount);
        Assert.Equal(30m, shipmentReport.TotalKg);

        using var mortalityReportResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-mortalities", reportRequest);
        Assert.Equal(HttpStatusCode.OK, mortalityReportResponse.StatusCode);
        var mortalityReport = (await mortalityReportResponse.Content.ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(10, mortalityReport.TotalCount);
        Assert.Equal(1.5m, mortalityReport.TotalKg);

        using var finalDashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        var finalDashboard = (await finalDashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var finalDashboardCage = Assert.Single(finalDashboard.Cages);
        Assert.Equal(890, finalDashboardCage.CurrentFishCount);
        Assert.Equal(300m, finalDashboardCage.CurrentAverageGram);
        Assert.Equal(267_000m, finalDashboardCage.CurrentBiomassGram);
        Assert.Equal(1_500m, finalDashboardCage.TotalDeadBiomassGram);
        Assert.Equal(1_500m, finalDashboardCage.DailyRows.Single(x => x.Date == "2026-03-21").DeadBiomassGram);
        Assert.Equal(0.603m, finalDashboardCage.Fcr);

        using var finalProjectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        var finalProjectDetail = (await finalProjectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>())!.Data!;
        var finalProjectDetailCage = Assert.Single(finalProjectDetail.Cages);
        Assert.Equal(10, finalProjectDetailCage.TotalDeadCount);
        Assert.Equal(1_500m, finalProjectDetailCage.DailyRows.Single(x => x.Date == "2026-03-21").DeadBiomassGram);

        using var finalRawKpiResponse = await client.GetAsync($"/api/kpi-report/raw-kpi/{projectId}");
        var finalRawKpi = (await finalRawKpiResponse.Content.ReadFromJsonAsync<ApiResponse<RawKpiReportDto>>())!.Data!;
        Assert.Equal(890, finalRawKpi.LiveFish);
        Assert.Equal(300m, finalRawKpi.CurrentAverageGram);
        Assert.Equal(267m, finalRawKpi.CurrentBiomassKg);
        Assert.Equal(10, finalRawKpi.DeadFish);

        using (var corruptScope = _factory.Services.CreateScope())
        {
            var db = corruptScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId && !x.IsDeleted);
            balance.BiomassGram = 1m;

            var batch = await db.FishBatches.SingleAsync(x => x.Id == fishBatchId);
            batch.CurrentAverageGram = 999m;

            var movements = await db.BatchMovements
                .Where(x => x.FishBatchId == fishBatchId && !x.IsDeleted)
                .ToListAsync();
            foreach (var movement in movements)
            {
                movement.SignedBiomassGram = movement.SignedCount < 0 ? -1m : 1m;
            }

            var shipmentLine = await db.ShipmentLines.SingleAsync(x =>
                x.FishBatchId == fishBatchId && !x.IsDeleted);
            shipmentLine.BiomassGram = 1m;
            await db.SaveChangesAsync();
        }

        var devirAfterStoredKgCorruption = await GetDevirFcr(new DateTime(2026, 3, 31));
        Assert.Equal(300m, devirAfterStoredKgCorruption.EndingAverageGram);
        Assert.Equal(267m, devirAfterStoredKgCorruption.EndingBiomassKg);
        Assert.Equal(30m, devirAfterStoredKgCorruption.ShippedBiomassKg);
        Assert.Equal(1.5m, devirAfterStoredKgCorruption.MortalityBiomassKg);

        using var dashboardAfterCorruptionResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        var dashboardAfterCorruption = (await dashboardAfterCorruptionResponse.Content
            .ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var dashboardCageAfterCorruption = Assert.Single(dashboardAfterCorruption.Cages);
        Assert.Equal(300m, dashboardCageAfterCorruption.CurrentAverageGram);
        Assert.Equal(267_000m, dashboardCageAfterCorruption.CurrentBiomassGram);
        Assert.Equal(1_500m, dashboardCageAfterCorruption.TotalDeadBiomassGram);

        using var shipmentAfterCorruptionResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-shipments", reportRequest);
        var shipmentAfterCorruption = (await shipmentAfterCorruptionResponse.Content
            .ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(30m, shipmentAfterCorruption.TotalKg);

        using var mortalityAfterCorruptionResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-mortalities", reportRequest);
        var mortalityAfterCorruption = (await mortalityAfterCorruptionResponse.Content
            .ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(1.5m, mortalityAfterCorruption.TotalKg);

        using var rawKpiAfterCorruptionResponse = await client.GetAsync($"/api/kpi-report/raw-kpi/{projectId}");
        var rawKpiAfterCorruption = (await rawKpiAfterCorruptionResponse.Content
            .ReadFromJsonAsync<ApiResponse<RawKpiReportDto>>())!.Data!;
        Assert.Equal(300m, rawKpiAfterCorruption.CurrentAverageGram);
        Assert.Equal(267m, rawKpiAfterCorruption.CurrentBiomassKg);

        async Task<FishGrowthDto> PostGrowth(DateTime growthDate, decimal targetGram)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
            {
                ProjectId = projectId,
                ProjectCageId = projectCageId,
                FishBatchId = fishBatchId,
                GrowthDate = growthDate,
                NewAverageGram = targetGram
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return body!.Data!;
        }

        async Task<DevirFcrReportRowDto> GetDevirFcr(DateTime toDate)
        {
            using var response = await client.PostAsJsonAsync("/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
            {
                ProjectIds = [projectId],
                FromDate = new DateTime(2026, 1, 1),
                ToDate = toDate
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<DevirFcrReportDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return Assert.Single(body!.Data!.Rows);
        }
    }

    [Fact]
    public async Task Shipment_UsesEnteredTotalKg_AndPreservesItDuringGrowthReplay()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var stockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"SHIP-KG-{suffix}",
                ProjectName = "Shipment Entered Total Kg Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"SK-{suffix}", CageName = "Shipment Total Kg Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage { ProjectId = project.Id, CageId = cage.Id, AssignedDate = project.StartDate };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"SKB-{suffix}",
                CurrentAverageGram = 720m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, projectCage.Id, 1_000, 720_000m,
                BatchMovementType.Stocking, project.StartDate, "Opening stocking", "TEST_OPENING", 1,
                null, projectCage.Id, stockId, stockId, 720m, 720m, 1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        const decimal enteredTotalKg = 75.12345678m;
        using var shipmentResponse = await client.PostAsJsonAsync(
            "/api/aqua/ShipmentLine/auto-header-and-post",
            new CreateShipmentLineWithAutoHeaderDto
            {
                ProjectId = projectId,
                ShipmentDate = new DateTime(2026, 1, 20),
                FishBatchId = fishBatchId,
                FromProjectCageId = projectCageId,
                FishCount = 100,
                AverageGram = 1m,
                BiomassGram = 1m,
                TotalKg = enteredTotalKg
            });
        Assert.Equal(HttpStatusCode.OK, shipmentResponse.StatusCode);
        var shipmentBody = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
        Assert.True(shipmentBody?.Success, shipmentBody?.ExceptionMessage);
        Assert.Equal(enteredTotalKg, shipmentBody!.Data!.TotalKg);
        Assert.Equal(75_123.45678m, shipmentBody.Data.BiomassGram);
        Assert.Equal(751.2345678m, shipmentBody.Data.AverageGram);

        using (var postedScope = _factory.Services.CreateScope())
        {
            var db = postedScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var movement = await db.BatchMovements.SingleAsync(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.MovementType == BatchMovementType.Shipment
                && x.ProjectCageId == projectCageId
                && x.SignedCount < 0);
            Assert.Equal(-75_123.45678m, movement.SignedBiomassGram);
            Assert.Equal(-75_123.45678m, movement.ReportedBiomassGram);
        }

        using var growthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 1, 25),
            NewAverageGram = 800m
        });
        Assert.Equal(HttpStatusCode.OK, growthResponse.StatusCode);

        using var replayScope = _factory.Services.CreateScope();
        var replayDb = replayScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var replayedLine = await replayDb.ShipmentLines.SingleAsync(x => x.Id == shipmentBody.Data.Id);
        Assert.Equal(enteredTotalKg, replayedLine.TotalKg);
        Assert.Equal(75_123.45678m, replayedLine.BiomassGram);
        Assert.Equal(751.2345678m, replayedLine.AverageGram);
        var replayedMovement = await replayDb.BatchMovements.SingleAsync(x =>
            !x.IsDeleted
            && x.FishBatchId == fishBatchId
            && x.MovementType == BatchMovementType.Shipment
            && x.ProjectCageId == projectCageId
            && x.SignedCount < 0);
        Assert.Equal(-75_123.45678m, replayedMovement.SignedBiomassGram);
        Assert.Equal(-75_123.45678m, replayedMovement.ReportedBiomassGram);
    }

    [Fact]
    public async Task Shipment_UsesExitWeightAtShipmentDate_AndIgnoresClientWeight()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var stockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"SHIP-GROW-{suffix}",
                ProjectName = "Shipment Weight Snapshot Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"SC-{suffix}", CageName = "Shipment Snapshot Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage { ProjectId = project.Id, CageId = cage.Id, AssignedDate = project.StartDate };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"SB-{suffix}",
                CurrentAverageGram = 720m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, projectCage.Id, 1_000, 720_000m,
                BatchMovementType.Stocking, project.StartDate, "Opening stocking", "TEST_OPENING", 1,
                null, projectCage.Id, stockId, stockId, 720m, 720m, 1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        var januaryShipment = await PostShipment(new DateTime(2026, 1, 20));
        Assert.Equal(720m, januaryShipment.AverageGram);
        Assert.Equal(72_000m, januaryShipment.BiomassGram);

        using var lateJanuaryGrowthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 1, 25),
            NewAverageGram = 800m
        });
        Assert.Equal(HttpStatusCode.OK, lateJanuaryGrowthResponse.StatusCode);
        var lateJanuaryGrowthBody = await lateJanuaryGrowthResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(lateJanuaryGrowthBody?.Success, lateJanuaryGrowthBody?.ExceptionMessage);
        Assert.Equal(800m, lateJanuaryGrowthBody!.Data!.NewAverageGram);
        Assert.Equal(1_000, lateJanuaryGrowthBody.Data.FishCount);

        using (var replayScope = _factory.Services.CreateScope())
        {
            var replayDb = replayScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var replayedJanuaryShipment = await replayDb.ShipmentLines
                .SingleAsync(x => x.Id == januaryShipment.Id);
            Assert.Equal(800m, replayedJanuaryShipment.AverageGram);
            Assert.Equal(80_000m, replayedJanuaryShipment.BiomassGram);
            var replayedBalance = await replayDb.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
            Assert.Equal(900, replayedBalance.LiveCount);
            Assert.Equal(800m, replayedBalance.AverageGram);
            Assert.Equal(720_000m, replayedBalance.BiomassGram);
        }

        // February growth is dated on the 1st, after the January shipment movement date,
        // so later February shipments snapshot this growth weight.
        using var februaryGrowthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 2, 20),
            NewAverageGram = 850m
        });
        Assert.Equal(HttpStatusCode.OK, februaryGrowthResponse.StatusCode);

        var februaryShipment = await PostShipment(new DateTime(2026, 2, 5));
        Assert.Equal(850m, februaryShipment.AverageGram);
        Assert.Equal(85_000m, februaryShipment.BiomassGram);

        async Task<ShipmentLineDto> PostShipment(DateTime shipmentDate)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
            {
                ProjectId = projectId,
                ShipmentDate = shipmentDate,
                FishBatchId = fishBatchId,
                FromProjectCageId = projectCageId,
                FishCount = 100,
                AverageGram = 1m,
                BiomassGram = 1m
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            using var postResponse = await client.PostAsync($"/api/aqua/posting/shipment/{body!.Data!.ShipmentId}", null);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            return body!.Data!;
        }
    }

    [Fact]
    public async Task Growth_ReconcilesOnlySelectedCage_AndDoesNotBlockOnAnotherCagesShipment()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long firstProjectCageId;
        long secondProjectCageId;
        long fishBatchId;
        long shipmentLineId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var stockId = await db.Stocks
                .Where(x => x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-CAGE-{suffix}",
                ProjectName = "Cage Scoped Growth Reconciliation",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var firstCage = new Cage { CageCode = $"GC-A-{suffix}", CageName = "Cage A" };
            var secondCage = new Cage { CageCode = $"GC-B-{suffix}", CageName = "Cage B" };
            db.Projects.Add(project);
            db.Cages.AddRange(firstCage, secondCage);
            await db.SaveChangesAsync();

            var firstProjectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = firstCage.Id,
                AssignedDate = project.StartDate
            };
            var secondProjectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = secondCage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"GC-BATCH-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.AddRange(firstProjectCage, secondProjectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, firstProjectCage.Id, 1_000, 100_000m,
                BatchMovementType.Stocking, project.StartDate, "Cage A opening",
                "TEST_CAGE_A_OPENING", 1, null, firstProjectCage.Id,
                stockId, stockId, 100m, 100m, 1);
            await ledger.ApplyDelta(
                project.Id, batch.Id, secondProjectCage.Id, 1_000, 100_000m,
                BatchMovementType.Stocking, project.StartDate, "Cage B opening",
                "TEST_CAGE_B_OPENING", 1, null, secondProjectCage.Id,
                stockId, stockId, 100m, 100m, 1);

            var shipment = new Shipment
            {
                ProjectId = project.Id,
                ShipmentNo = $"LEGACY-SHIP-{suffix}",
                ShipmentDate = new DateTime(2026, 1, 15),
                Status = DocumentStatus.Posted,
                CreatedBy = 1,
                IsDeleted = false
            };
            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();
            var shipmentLine = new ShipmentLine
            {
                ShipmentId = shipment.Id,
                FishBatchId = batch.Id,
                FromProjectCageId = firstProjectCage.Id,
                FishCount = 300,
                AverageGram = 100m,
                BiomassGram = 30_000m,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                CreatedBy = 1,
                IsDeleted = false
            };
            db.ShipmentLines.Add(shipmentLine);
            await db.SaveChangesAsync();

            projectId = project.Id;
            firstProjectCageId = firstProjectCage.Id;
            secondProjectCageId = secondProjectCage.Id;
            fishBatchId = batch.Id;
            shipmentLineId = shipmentLine.Id;
        }

        using (var secondCageGrowthResponse = await client.PostAsJsonAsync(
                   "/api/aqua/FishGrowth",
                   new CreateFishGrowthDto
                   {
                       ProjectId = projectId,
                       ProjectCageId = secondProjectCageId,
                       FishBatchId = fishBatchId,
                       GrowthDate = new DateTime(2026, 2, 10),
                       NewAverageGram = 200m
                   }))
        {
            Assert.Equal(HttpStatusCode.OK, secondCageGrowthResponse.StatusCode);
        }

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.False(await db.BatchMovements.AnyAsync(x =>
                x.MovementType == BatchMovementType.Shipment
                && x.ReferenceId == shipmentLineId));
            var firstBalance = await db.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == firstProjectCageId);
            Assert.Equal(1_000, firstBalance.LiveCount);
            Assert.Equal(100m, firstBalance.AverageGram);
        }

        using (var firstCageGrowthResponse = await client.PostAsJsonAsync(
                   "/api/aqua/FishGrowth",
                   new CreateFishGrowthDto
                   {
                       ProjectId = projectId,
                       ProjectCageId = firstProjectCageId,
                       FishBatchId = fishBatchId,
                       GrowthDate = new DateTime(2026, 2, 10),
                       NewAverageGram = 200m
                   }))
        {
            Assert.Equal(HttpStatusCode.OK, firstCageGrowthResponse.StatusCode);
        }

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var shipmentMovement = await db.BatchMovements.SingleAsync(x =>
                x.MovementType == BatchMovementType.Shipment
                && x.ReferenceTable == "RII_SHIPMENT_LINE"
                && x.ReferenceId == shipmentLineId);
            Assert.Equal(-300, shipmentMovement.SignedCount);
            Assert.Equal(-30_000m, shipmentMovement.SignedBiomassGram);

            var firstBalance = await db.BatchCageBalances.SingleAsync(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == firstProjectCageId);
            Assert.Equal(700, firstBalance.LiveCount);
            Assert.Equal(200m, firstBalance.AverageGram);
            Assert.Equal(140_000m, firstBalance.BiomassGram);
        }
    }
}
