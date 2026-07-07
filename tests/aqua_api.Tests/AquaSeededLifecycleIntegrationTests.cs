using System.Globalization;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaSettings.Domain.Entities;
using aqua_api.Modules.Cages.Application.Dtos;
using aqua_api.Modules.Cages.Application.Services;
using aqua_api.Modules.Cages.DependencyInjection;
using aqua_api.Modules.Identity.Domain.Entities;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Modules.Stock.DependencyInjection;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.Warehouse.DependencyInjection;
using aqua_api.Modules.Warehouse.Domain.Entities;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Mappings;
using aqua_api.Shared.Infrastructure.DependencyInjection;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public class AquaSeededLifecycleIntegrationTests
{
    [Fact]
    public async Task SeededLifecycle_OpeningToWarehouseAndShipment_KeepsBackendStateConsistent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        var dbOptions = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlite(connection)
            .Options;
        services.AddScoped<SqliteTestAquaDbContext>(_ => new SqliteTestAquaDbContext(dbOptions));
        services.AddScoped<AquaDbContext>(sp => sp.GetRequiredService<SqliteTestAquaDbContext>());
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddAquaSharedInfrastructure();
        services.AddCageModule();
        services.AddStockModule();
        services.AddWarehouseModule();
        services.AddFishBatchModule();
        services.AddDailyWeathersModule();
        services.AddFeedingsModule();
        services.AddGoodsReceiptsModule();
        services.AddMortalitiesModule();
        services.AddNetOperationsModule();
        services.AddStockConvertsModule();
        services.AddTransfersModule();
        services.AddShipmentsModule();
        services.AddWeighingsModule();
        services.AddBatchBalancesModule();
        services.AddOpeningImportsModule();
        services.AddProjectMergesModule();
        services.AddProjectKpisModule();
        services.AddAquaReportsModule();
        services.AddAquaModule();
        services.AddScoped<IErpService, FakeErpService>();
        services.AddScoped<INetsisItemSlipService, FakeNetsisItemSlipService>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureCreatedAsync();

        await SeedMasterDataAsync(db);

        var cageService = scope.ServiceProvider.GetRequiredService<ICageService>();
        var openingImportService = scope.ServiceProvider.GetRequiredService<IOpeningImportService>();
        var goodsReceiptFishDistributionService = scope.ServiceProvider.GetRequiredService<IGoodsReceiptFishDistributionService>();
        var fishBatchService = scope.ServiceProvider.GetRequiredService<IFishBatchService>();
        var feedingLineService = scope.ServiceProvider.GetRequiredService<IFeedingLineService>();
        var feedingDistributionService = scope.ServiceProvider.GetRequiredService<IFeedingDistributionService>();
        var mortalityLineService = scope.ServiceProvider.GetRequiredService<IMortalityLineService>();
        var stockConvertLineService = scope.ServiceProvider.GetRequiredService<IStockConvertLineService>();
        var transferLineService = scope.ServiceProvider.GetRequiredService<ITransferLineService>();
        var transferService = scope.ServiceProvider.GetRequiredService<ITransferService>();
        var cageWarehouseTransferLineService = scope.ServiceProvider.GetRequiredService<ICageWarehouseTransferLineService>();
        var cageWarehouseTransferService = scope.ServiceProvider.GetRequiredService<ICageWarehouseTransferService>();
        var warehouseTransferLineService = scope.ServiceProvider.GetRequiredService<IWarehouseTransferLineService>();
        var warehouseTransferService = scope.ServiceProvider.GetRequiredService<IWarehouseTransferService>();
        var warehouseCageTransferLineService = scope.ServiceProvider.GetRequiredService<IWarehouseCageTransferLineService>();
        var warehouseCageTransferService = scope.ServiceProvider.GetRequiredService<IWarehouseCageTransferService>();
        var shipmentLineService = scope.ServiceProvider.GetRequiredService<IShipmentLineService>();
        var shipmentService = scope.ServiceProvider.GetRequiredService<IShipmentService>();
        var kpiService = scope.ServiceProvider.GetRequiredService<IProjectCageDailyKpiService>();

        var missingCageName = await cageService.CreateAsync(new CreateCageDto
        {
            CageCode = "CAGE-VALIDATION",
            CageName = string.Empty,
        });
        Assert.False(missingCageName.Success);
        Assert.Equal(400, missingCageName.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(missingCageName.Message));

        var firstCage = await cageService.CreateAsync(new CreateCageDto
        {
            CageCode = "CAGE-DUPLICATE",
            CageName = "Duplicate Guard Cage",
        });
        Assert.True(firstCage.Success, firstCage.Message);

        var duplicateCage = await cageService.CreateAsync(new CreateCageDto
        {
            CageCode = "CAGE-DUPLICATE",
            CageName = "Duplicate Guard Cage Again",
        });
        Assert.False(duplicateCage.Success);
        Assert.Equal(409, duplicateCage.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(duplicateCage.Message));

        var invalidFishDistribution = await goodsReceiptFishDistributionService.CreateAsync(new CreateGoodsReceiptFishDistributionDto());
        Assert.False(invalidFishDistribution.Success);
        Assert.Equal(400, invalidFishDistribution.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(invalidFishDistribution.Message));

        var preview = await openingImportService.PreviewAsync(new OpeningImportPreviewRequestDto
        {
            FileName = "seeded-lifecycle.xlsx",
            SourceSystem = "integration-test",
            Sheets =
            [
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Projects",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "projectName", TargetField = "projectName" },
                        new OpeningImportFieldMappingDto { SourceColumn = "startDate", TargetField = "startDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = "PRJ-001",
                            ["projectName"] = "Integration Farm",
                            ["startDate"] = "2026-04-01",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Cages",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageName", TargetField = "cageName" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = "PRJ-001",
                            ["cageCode"] = "CAGE-01",
                            ["cageName"] = "Main Cage",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "OpeningStock",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "batchCode", TargetField = "batchCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishStockCode", TargetField = "fishStockCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishCount", TargetField = "fishCount" },
                        new OpeningImportFieldMappingDto { SourceColumn = "averageGram", TargetField = "averageGram" },
                        new OpeningImportFieldMappingDto { SourceColumn = "asOfDate", TargetField = "asOfDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = "PRJ-001",
                            ["cageCode"] = "CAGE-01",
                            ["batchCode"] = "BATCH-PLAMUT-5G",
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "10000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success);
        var previewDump = string.Join(" || ", preview.Data!.Rows.Select(row =>
            $"{row.SheetName}:{row.RowNumber}:{row.Status}:{string.Join(" | ", row.Messages)}"));
        Assert.True(preview.Data.Summary.ErrorRows == 0, previewDump);
        Assert.All(preview.Data.Rows, row => Assert.DoesNotContain("Error", row.Status, StringComparison.OrdinalIgnoreCase));

        var commit = await openingImportService.CommitAsync(preview.Data!.JobId);
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        Assert.Equal(1, commit.Data!.CreatedProjects);
        Assert.Equal(1, commit.Data.CreatedCages);
        Assert.Equal(1, commit.Data.CreatedFishBatches);
        Assert.Equal(1, commit.Data.AppliedCageRows);

        var project = await db.Projects.SingleAsync(x => !x.IsDeleted && x.ProjectCode == "PRJ-001");
        var projectCage = await db.ProjectCages
            .Include(x => x.Cage)
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id);
        var targetCage = new Cage
        {
            CageCode = "CAGE-02",
            CageName = "Target Cage",
        };
        db.Cages.Add(targetCage);
        await db.SaveChangesAsync();

        var targetProjectCage = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = targetCage.Id,
            AssignedDate = new DateTime(2026, 4, 1),
        };
        db.ProjectCages.Add(targetProjectCage);
        db.AquaSettings.Add(new AquaSetting
        {
            RequireFullTransfer = false,
            PartialTransferOccupiedCageMode = 2,
        });
        await db.SaveChangesAsync();

        var openingBatch = await db.FishBatches.SingleAsync(x => !x.IsDeleted && x.BatchCode == "BATCH-PLAMUT-5G");
        var fish5Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G");
        var fish10Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-10G");
        var feedStock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD");
        var warehouse = await db.Warehouses.SingleAsync(x => !x.IsDeleted && x.ErpWarehouseCode == 10);
        var targetWarehouse = await db.Warehouses.SingleAsync(x => !x.IsDeleted && x.ErpWarehouseCode == 20);

        var convertedBatchCreate = await fishBatchService.CreateAsync(new CreateFishBatchDto
        {
            ProjectId = project.Id,
            BatchCode = "BATCH-PLAMUT-10G",
            FishStockId = fish10Stock.Id,
            CurrentAverageGram = 10m,
            StartDate = new DateTime(2026, 4, 3),
            TargetHarvestAverageGram = 20m,
        });

        Assert.True(convertedBatchCreate.Success);
        var convertedBatchId = convertedBatchCreate.Data!.Id;

        await SeedFeedPurchaseHistoryAsync(db, project.Id, warehouse.Id, feedStock.Id);

        var feedingDay2 = await feedingLineService.CreateWithAutoHeaderAsync(new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            FeedingDate = new DateTime(2026, 4, 2),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStock.Id,
            QtyUnit = 20m,
            GramPerUnit = 1000m,
            TotalGram = 20_000m,
        });
        Assert.True(feedingDay2.Success);
        var feedingDistributionDay2 = await feedingDistributionService.CreateAsync(new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay2.Data!.Id,
            FishBatchId = openingBatch.Id,
            ProjectCageId = projectCage.Id,
            FeedGram = 20_000m,
        });
        Assert.True(feedingDistributionDay2.Success, $"{feedingDistributionDay2.Message} | {feedingDistributionDay2.ExceptionMessage}");

        var mortalityDay2 = await mortalityLineService.CreateWithAutoHeaderAsync(new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            MortalityDate = new DateTime(2026, 4, 2),
            FishBatchId = openingBatch.Id,
            ProjectCageId = projectCage.Id,
            DeadCount = 100,
        });
        Assert.True(mortalityDay2.Success);

        var feedingDay3 = await feedingLineService.CreateWithAutoHeaderAsync(new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            FeedingDate = new DateTime(2026, 4, 3),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStock.Id,
            QtyUnit = 25m,
            GramPerUnit = 1000m,
            TotalGram = 25_000m,
        });
        Assert.True(feedingDay3.Success);
        Assert.True((await feedingDistributionService.CreateAsync(new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay3.Data!.Id,
            FishBatchId = openingBatch.Id,
            ProjectCageId = projectCage.Id,
            FeedGram = 25_000m,
        })).Success);

        var stockConvert = await stockConvertLineService.CreateWithAutoHeaderAsync(new CreateStockConvertLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ConvertDate = new DateTime(2026, 4, 3),
            FromFishBatchId = openingBatch.Id,
            ToFishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            ToProjectCageId = projectCage.Id,
            FishCount = 4_000,
            AverageGram = 5m,
            NewAverageGram = 5m,
            BiomassGram = BatchMath.CalculateBiomassGram(4_000, 5m),
        });
        Assert.True(stockConvert.Success);

        var feedingDay4 = await feedingLineService.CreateWithAutoHeaderAsync(new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            FeedingDate = new DateTime(2026, 4, 4),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStock.Id,
            QtyUnit = 18m,
            GramPerUnit = 1000m,
            TotalGram = 18_000m,
        });
        Assert.True(feedingDay4.Success);
        Assert.True((await feedingDistributionService.CreateAsync(new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = openingBatch.Id,
            ProjectCageId = projectCage.Id,
            FeedGram = 10_000m,
        })).Success);
        Assert.True((await feedingDistributionService.CreateAsync(new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = convertedBatchId,
            ProjectCageId = projectCage.Id,
            FeedGram = 8_000m,
        })).Success);

        var cageWarehouseLine = await cageWarehouseTransferLineService.CreateWithAutoHeaderAsync(new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            ToWarehouseId = warehouse.Id,
            FishCount = 1_500,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(1_500, 10m),
        });
        Assert.True(cageWarehouseLine.Success);

        var cageWarehouseHeader = await db.CageWarehouseTransfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 4));
        Assert.True((await cageWarehouseTransferService.Post(cageWarehouseHeader.Id, 1)).Success);

        var warehouseCageLine = await warehouseCageTransferLineService.CreateWithAutoHeaderAsync(new CreateWarehouseCageTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromWarehouseId = warehouse.Id,
            ToProjectCageId = projectCage.Id,
            FishCount = 500,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(500, 10m),
        });
        Assert.True(warehouseCageLine.Success);

        var warehouseCageHeader = await db.WarehouseCageTransfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 4));
        Assert.True((await warehouseCageTransferService.Post(warehouseCageHeader.Id, 1)).Success);

        var shipmentLineOne = await shipmentLineService.CreateWithAutoHeaderAsync(new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            FishCount = 1_000,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(1_000, 10m),
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 210m,
        });
        Assert.True(shipmentLineOne.Success);

        var shipmentLineTwo = await shipmentLineService.CreateWithAutoHeaderAsync(new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            FishCount = 200,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(200, 10m),
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 230m,
        });
        Assert.True(shipmentLineTwo.Success, $"{shipmentLineTwo.Message} | {shipmentLineTwo.ExceptionMessage}");

        var shipmentHeader = await db.Shipments
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.ShipmentDate.Date == new DateTime(2026, 4, 4));
        Assert.True((await shipmentService.Post(shipmentHeader.Id, 1)).Success);

        var mortalityDay4 = await mortalityLineService.CreateWithAutoHeaderAsync(new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            MortalityDate = new DateTime(2026, 4, 4),
            FishBatchId = openingBatch.Id,
            ProjectCageId = projectCage.Id,
            DeadCount = 50,
        });
        Assert.True(mortalityDay4.Success);

        var openingBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == openingBatch.Id && x.ProjectCageId == projectCage.Id);
        var convertedBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var warehouseBalance = await db.BatchWarehouseBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);

        Assert.Equal(5_850, openingBalance.LiveCount);
        Assert.Equal(29_250m, openingBalance.BiomassGram);
        Assert.Equal(5m, openingBalance.AverageGram);
        Assert.Equal(new DateTime(2026, 4, 4), openingBalance.AsOfDate.Date);

        Assert.Equal(1_800, convertedBalance.LiveCount);
        Assert.Equal(18_000m, convertedBalance.BiomassGram);
        Assert.Equal(10m, convertedBalance.AverageGram);
        Assert.Equal(new DateTime(2026, 4, 4), convertedBalance.AsOfDate.Date);

        Assert.Equal(1_000, warehouseBalance.LiveCount);
        Assert.Equal(10_000m, warehouseBalance.BiomassGram);
        Assert.Equal(10m, warehouseBalance.AverageGram);

        var kpiResult = await kpiService.CreateSnapshotAsync(new CreateProjectCageDailyKpiSnapshotRequest
        {
            ProjectId = project.Id,
            SnapshotDate = new DateTime(2026, 4, 4),
        }, 1);

        Assert.True(kpiResult.Success, $"{kpiResult.Message} | {kpiResult.ExceptionMessage}");
        Assert.Equal(2, kpiResult.Data!.Count);
        Assert.Equal(7_650, kpiResult.Data.Sum(x => x.LiveCount));
        Assert.Equal(47.25m, kpiResult.Data.Sum(x => x.BiomassKg));
        Assert.Equal(63m, kpiResult.Data.Sum(x => x.FeedKgPeriod));
        Assert.Contains(kpiResult.Data, x => x.FishBatchId == openingBatch.Id && x.DeadCountPeriod == 150);
        Assert.Contains(kpiResult.Data, x => x.FishBatchId == convertedBatchId && x.LiveCount == 1_800);

        var shipmentLines = await db.ShipmentLines
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync();
        Assert.Equal(2, shipmentLines.Count);
        Assert.Equal(2_100m, shipmentLines[0].LocalLineAmount);
        Assert.Equal(460m, shipmentLines[1].LocalLineAmount);

        var feedPurchaseLines = await db.GoodsReceiptLines
            .Where(x => !x.IsDeleted && x.ItemType == GoodsReceiptItemType.Feed)
            .OrderBy(x => x.Id)
            .ToListAsync();
        Assert.Equal(3, feedPurchaseLines.Count);

        var weightedFeedCostPerKg = CalculateWeightedFeedCostPerKg(feedPurchaseLines);
        var weightedSalePricePerKg = CalculateWeightedSalePricePerKg(shipmentLines);
        var cageBiomassKg = roundKg(openingBalance.BiomassGram + convertedBalance.BiomassGram);
        var estimatedFeedCost = Math.Round(63m * weightedFeedCostPerKg, 3, MidpointRounding.AwayFromZero);
        var projectedRevenue = Math.Round(cageBiomassKg * weightedSalePricePerKg, 3, MidpointRounding.AwayFromZero);
        var projectedGrossMargin = Math.Round(projectedRevenue - estimatedFeedCost, 3, MidpointRounding.AwayFromZero);

        Assert.Equal(61.079m, weightedFeedCostPerKg);
        Assert.Equal(213.333m, weightedSalePricePerKg);
        Assert.Equal(3_847.977m, estimatedFeedCost);
        Assert.Equal(10_079.984m, projectedRevenue);
        Assert.Equal(6_232.007m, projectedGrossMargin);

        var cageBalanceBeforeCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var warehouseBalanceBeforeCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);

        var reversibleTransferLine = await cageWarehouseTransferLineService.CreateWithAutoHeaderAsync(new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 8),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            ToWarehouseId = warehouse.Id,
            FishCount = 100,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(100, 10m),
        });
        Assert.True(reversibleTransferLine.Success);

        var reversibleTransferHeader = await db.CageWarehouseTransfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 8));
        Assert.True((await cageWarehouseTransferService.Post(reversibleTransferHeader.Id, 1)).Success);

        var cageBalanceAfterPost = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var warehouseBalanceAfterPost = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);

        Assert.Equal(cageBalanceBeforeCancel.LiveCount - 100, cageBalanceAfterPost.LiveCount);
        Assert.Equal(cageBalanceBeforeCancel.BiomassGram - 1_000m, cageBalanceAfterPost.BiomassGram);
        Assert.Equal(warehouseBalanceBeforeCancel.LiveCount + 100, warehouseBalanceAfterPost.LiveCount);
        Assert.Equal(warehouseBalanceBeforeCancel.BiomassGram + 1_000m, warehouseBalanceAfterPost.BiomassGram);

        Assert.True((await cageWarehouseTransferLineService.SoftDeleteAsync(reversibleTransferLine.Data!.Id, 1)).Success);

        var cancelledTransferHeader = await db.CageWarehouseTransfers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reversibleTransferHeader.Id);
        var cageBalanceAfterCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var warehouseBalanceAfterCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);

        Assert.True(cancelledTransferHeader.IsDeleted);
        Assert.Equal(DocumentStatus.Cancelled, cancelledTransferHeader.Status);
        Assert.Equal(cageBalanceBeforeCancel.LiveCount, cageBalanceAfterCancel.LiveCount);
        Assert.Equal(cageBalanceBeforeCancel.BiomassGram, cageBalanceAfterCancel.BiomassGram);
        Assert.Equal(warehouseBalanceBeforeCancel.LiveCount, warehouseBalanceAfterCancel.LiveCount);
        Assert.Equal(warehouseBalanceBeforeCancel.BiomassGram, warehouseBalanceAfterCancel.BiomassGram);

        var warehouseTransferSourceBeforeCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseTransferTargetBeforeCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == targetWarehouse.Id);
        Assert.Null(warehouseTransferTargetBeforeCancel);

        var reversibleWarehouseTransferLine = await warehouseTransferLineService.CreateWithAutoHeaderAsync(new CreateWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 8),
            FishBatchId = convertedBatchId,
            FromWarehouseId = warehouse.Id,
            ToWarehouseId = targetWarehouse.Id,
            FishCount = 100,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(100, 10m),
        });
        Assert.True(reversibleWarehouseTransferLine.Success, $"{reversibleWarehouseTransferLine.Message} | {reversibleWarehouseTransferLine.ExceptionMessage}");

        var reversibleWarehouseTransferHeader = await db.WarehouseTransfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 8));
        Assert.True((await warehouseTransferService.Post(reversibleWarehouseTransferHeader.Id, 1)).Success);

        var warehouseTransferSourceAfterPost = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseTransferTargetAfterPost = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == targetWarehouse.Id);

        Assert.Equal(warehouseTransferSourceBeforeCancel.LiveCount - 100, warehouseTransferSourceAfterPost.LiveCount);
        Assert.Equal(warehouseTransferSourceBeforeCancel.BiomassGram - 1_000m, warehouseTransferSourceAfterPost.BiomassGram);
        Assert.Equal(100, warehouseTransferTargetAfterPost.LiveCount);
        Assert.Equal(1_000m, warehouseTransferTargetAfterPost.BiomassGram);

        Assert.True((await warehouseTransferLineService.SoftDeleteAsync(reversibleWarehouseTransferLine.Data!.Id, 1)).Success);

        var cancelledWarehouseTransferHeader = await db.WarehouseTransfers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reversibleWarehouseTransferHeader.Id);
        var warehouseTransferSourceAfterCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseTransferTargetAfterCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == targetWarehouse.Id);

        Assert.True(cancelledWarehouseTransferHeader.IsDeleted);
        Assert.Equal(DocumentStatus.Cancelled, cancelledWarehouseTransferHeader.Status);
        Assert.Equal(warehouseTransferSourceBeforeCancel.LiveCount, warehouseTransferSourceAfterCancel.LiveCount);
        Assert.Equal(warehouseTransferSourceBeforeCancel.BiomassGram, warehouseTransferSourceAfterCancel.BiomassGram);
        Assert.Equal(0, warehouseTransferTargetAfterCancel.LiveCount);
        Assert.Equal(0m, warehouseTransferTargetAfterCancel.BiomassGram);

        var warehouseCageSourceBeforeLineCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseCageTargetBeforeLineCancel = await db.BatchCageBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);
        Assert.Null(warehouseCageTargetBeforeLineCancel);

        var reversibleWarehouseCageLine = await warehouseCageTransferLineService.CreateWithAutoHeaderAsync(new CreateWarehouseCageTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 8),
            FishBatchId = convertedBatchId,
            FromWarehouseId = warehouse.Id,
            ToProjectCageId = targetProjectCage.Id,
            FishCount = 100,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(100, 10m),
        });
        Assert.True(reversibleWarehouseCageLine.Success, $"{reversibleWarehouseCageLine.Message} | {reversibleWarehouseCageLine.ExceptionMessage}");

        var reversibleWarehouseCageHeader = await db.WarehouseCageTransfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 8));
        Assert.True((await warehouseCageTransferService.Post(reversibleWarehouseCageHeader.Id, 1)).Success);

        var warehouseCageSourceAfterPost = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseCageTargetAfterPost = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);

        Assert.Equal(warehouseCageSourceBeforeLineCancel.LiveCount - 100, warehouseCageSourceAfterPost.LiveCount);
        Assert.Equal(warehouseCageSourceBeforeLineCancel.BiomassGram - 1_000m, warehouseCageSourceAfterPost.BiomassGram);
        Assert.Equal(100, warehouseCageTargetAfterPost.LiveCount);
        Assert.Equal(1_000m, warehouseCageTargetAfterPost.BiomassGram);

        Assert.True((await warehouseCageTransferLineService.SoftDeleteAsync(reversibleWarehouseCageLine.Data!.Id, 1)).Success);

        var cancelledWarehouseCageHeader = await db.WarehouseCageTransfers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reversibleWarehouseCageHeader.Id);
        var warehouseCageSourceAfterCancel = await db.BatchWarehouseBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouse.Id);
        var warehouseCageTargetAfterCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);

        Assert.True(cancelledWarehouseCageHeader.IsDeleted);
        Assert.Equal(DocumentStatus.Cancelled, cancelledWarehouseCageHeader.Status);
        Assert.Equal(warehouseCageSourceBeforeLineCancel.LiveCount, warehouseCageSourceAfterCancel.LiveCount);
        Assert.Equal(warehouseCageSourceBeforeLineCancel.BiomassGram, warehouseCageSourceAfterCancel.BiomassGram);
        Assert.Equal(0, warehouseCageTargetAfterCancel.LiveCount);
        Assert.Equal(0m, warehouseCageTargetAfterCancel.BiomassGram);

        var sourceCageBeforeTransferCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var targetBalanceBeforeTransferCancel = await db.BatchCageBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);
        Assert.True(targetBalanceBeforeTransferCancel == null || targetBalanceBeforeTransferCancel.LiveCount == 0);
        Assert.True(targetBalanceBeforeTransferCancel == null || targetBalanceBeforeTransferCancel.BiomassGram == 0m);

        var reversibleCageTransferLine = await transferLineService.CreateWithAutoHeaderAsync(new CreateTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 9),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCage.Id,
            ToProjectCageId = targetProjectCage.Id,
            FishCount = 100,
            AverageGram = 10m,
            BiomassGram = BatchMath.CalculateBiomassGram(100, 10m),
        });
        Assert.True(reversibleCageTransferLine.Success, $"{reversibleCageTransferLine.Message} | {reversibleCageTransferLine.ExceptionMessage}");

        var reversibleCageTransferHeader = await db.Transfers
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 9));
        Assert.Equal(DocumentStatus.Posted, reversibleCageTransferHeader.Status);

        var sourceCageAfterTransferPost = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var targetCageAfterTransferPost = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);

        Assert.Equal(sourceCageBeforeTransferCancel.LiveCount - 100, sourceCageAfterTransferPost.LiveCount);
        Assert.Equal(sourceCageBeforeTransferCancel.BiomassGram - 1_000m, sourceCageAfterTransferPost.BiomassGram);
        Assert.Equal(100, targetCageAfterTransferPost.LiveCount);
        Assert.Equal(1_000m, targetCageAfterTransferPost.BiomassGram);

        Assert.True((await transferLineService.SoftDeleteAsync(reversibleCageTransferLine.Data!.Id, 1)).Success);

        var cancelledCageTransferHeader = await db.Transfers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reversibleCageTransferHeader.Id);
        var sourceCageAfterTransferCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCage.Id);
        var targetCageAfterTransferCancel = await db.BatchCageBalances
            .AsNoTracking()
            .SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == targetProjectCage.Id);

        Assert.True(cancelledCageTransferHeader.IsDeleted);
        Assert.Equal(DocumentStatus.Cancelled, cancelledCageTransferHeader.Status);
        Assert.Equal(sourceCageBeforeTransferCancel.LiveCount, sourceCageAfterTransferCancel.LiveCount);
        Assert.Equal(sourceCageBeforeTransferCancel.BiomassGram, sourceCageAfterTransferCancel.BiomassGram);
        Assert.Equal(0, targetCageAfterTransferCancel.LiveCount);
        Assert.Equal(0m, targetCageAfterTransferCancel.BiomassGram);

        var movements = await db.BatchMovements
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var movementSummary = string.Join(", ", movements
            .GroupBy(x => x.MovementType)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Count()}"));
        Assert.True(movements.Count == 31, movementSummary);
        Assert.Equal(1, movements.Count(x => x.MovementType == BatchMovementType.OpeningImport));
        Assert.Equal(4, movements.Count(x => x.MovementType == BatchMovementType.Feeding));
        Assert.Equal(2, movements.Count(x => x.MovementType == BatchMovementType.Mortality));
        Assert.Equal(2, movements.Count(x => x.MovementType == BatchMovementType.StockConvert));
        Assert.Equal(4, movements.Count(x => x.MovementType == BatchMovementType.Transfer));
        Assert.Equal(16, movements.Count(x => x.MovementType == BatchMovementType.WarehouseTransfer));
        Assert.Equal(2, movements.Count(x => x.MovementType == BatchMovementType.Shipment));
    }

    private static async Task SeedMasterDataAsync(AquaDbContext db)
    {
        db.Users.Add(new User
        {
            Id = 1,
            Username = "integration-user",
            Email = "integration-user@example.com",
            PasswordHash = "seeded-hash",
            FirstName = "Integration",
            LastName = "User",
            RoleId = 1,
            IsActive = true,
            IsEmailConfirmed = true,
        });

        db.Stocks.AddRange(
            new Stock
            {
                ErpStockCode = "PLAMUT-5G",
                StockName = "Plamut 5g",
                Unit = "ADET",
                BranchCode = 1,
            },
            new Stock
            {
                ErpStockCode = "PLAMUT-10G",
                StockName = "Plamut 10g",
                Unit = "ADET",
                BranchCode = 1,
            },
            new Stock
            {
                ErpStockCode = "YEM-STD",
                StockName = "Standart Yem",
                Unit = "KG",
                BranchCode = 1,
            });

        db.Warehouses.AddRange(
            new Warehouse
            {
                ErpWarehouseCode = 10,
                WarehouseName = "Ana Depo",
                BranchCode = 1,
                AllowNegativeBalance = false,
                IsLocked = false,
            },
            new Warehouse
            {
                ErpWarehouseCode = 20,
                WarehouseName = "Yedek Depo",
                BranchCode = 1,
                AllowNegativeBalance = false,
                IsLocked = false,
            });

        await db.SaveChangesAsync();
    }

    private static async Task SeedFeedPurchaseHistoryAsync(AquaDbContext db, long projectId, long warehouseId, long feedStockId)
    {
        var rows = new[]
        {
            new { ReceiptNo = "FEED-01", Date = new DateTime(2026, 4, 1), TotalGram = 20_000m, LocalAmount = 1_160m },
            new { ReceiptNo = "FEED-02", Date = new DateTime(2026, 4, 2), TotalGram = 25_000m, LocalAmount = 1_500m },
            new { ReceiptNo = "FEED-03", Date = new DateTime(2026, 4, 4), TotalGram = 18_000m, LocalAmount = 1_188m },
        };

        foreach (var row in rows)
        {
            var receipt = new GoodsReceipt
            {
                ProjectId = projectId,
                ReceiptNo = row.ReceiptNo,
                ReceiptDate = row.Date,
                WarehouseId = warehouseId,
                Status = DocumentStatus.Posted,
            };

            receipt.Lines.Add(new GoodsReceiptLine
            {
                ItemType = GoodsReceiptItemType.Feed,
                StockId = feedStockId,
                QtyUnit = row.TotalGram / 1000m,
                GramPerUnit = 1000m,
                TotalGram = row.TotalGram,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                UnitPrice = row.LocalAmount / (row.TotalGram / 1000m),
                LocalUnitPrice = row.LocalAmount / (row.TotalGram / 1000m),
                LineAmount = row.LocalAmount,
                LocalLineAmount = row.LocalAmount,
            });

            db.GoodsReceipts.Add(receipt);
        }

        await db.SaveChangesAsync();
    }

    private static decimal CalculateWeightedFeedCostPerKg(IEnumerable<GoodsReceiptLine> lines)
    {
        var totals = lines.Aggregate(
            new { Kg = 0m, Amount = 0m },
            (sum, line) => new
            {
                Kg = sum.Kg + ((line.TotalGram ?? 0m) / 1000m),
                Amount = sum.Amount + (line.LocalLineAmount ?? 0m),
            });

        return totals.Kg > 0
            ? Math.Round(totals.Amount / totals.Kg, 3, MidpointRounding.AwayFromZero)
            : 0m;
    }

    private static decimal CalculateWeightedSalePricePerKg(IEnumerable<ShipmentLine> lines)
    {
        var totals = lines.Aggregate(
            new { Kg = 0m, Amount = 0m },
            (sum, line) => new
            {
                Kg = sum.Kg + (line.BiomassGram / 1000m),
                Amount = sum.Amount + (line.LocalLineAmount ?? 0m),
            });

        return totals.Kg > 0
            ? Math.Round(totals.Amount / totals.Kg, 3, MidpointRounding.AwayFromZero)
            : 0m;
    }

    private static decimal roundKg(decimal biomassGram)
        => Math.Round(biomassGram / 1000m, 3, MidpointRounding.AwayFromZero);

    private sealed class FakeErpService : IErpService
    {
        public Task<ApiResponse<short>> GetBranchCodeFromContext()
            => Task.FromResult(ApiResponse<short>.SuccessResult(1, "ok"));

        public Task<ApiResponse<List<CariDto>>> GetCarisAsync(string? cariKodu)
            => Task.FromResult(ApiResponse<List<CariDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<CariDto>>> GetCarisByCodesAsync(IEnumerable<string> cariKodlari)
            => Task.FromResult(ApiResponse<List<CariDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<DepoDto>>> GetDeposAsync(short? depoKodu)
            => Task.FromResult(ApiResponse<List<DepoDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<StokFunctionDto>>> GetStoksAsync(string? stokKodu)
            => Task.FromResult(ApiResponse<List<StokFunctionDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null)
            => Task.FromResult(ApiResponse<List<BranchDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<KurDto>>> GetExchangeRateAsync(DateTime tarih, int fiyatTipi)
            => Task.FromResult(ApiResponse<List<KurDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<ErpShippingAddressDto>>> GetErpShippingAddressAsync(string customerCode)
            => Task.FromResult(ApiResponse<List<ErpShippingAddressDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<StokGroupDto>>> GetStokGroupAsync(string? grupKodu)
            => Task.FromResult(ApiResponse<List<StokGroupDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<List<ProjeDto>>> GetProjectCodesAsync()
            => Task.FromResult(ApiResponse<List<ProjeDto>>.SuccessResult([], "ok"));

        public Task<ApiResponse<object>> HealthCheckAsync()
            => Task.FromResult(ApiResponse<object>.SuccessResult(new { healthy = true }, "ok"));
    }

    private sealed class FakeNetsisItemSlipService : INetsisItemSlipService
    {
        public Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferOutAsync(NetsisItemSlipCreateDto request, CancellationToken cancellationToken = default)
            => CreateDocumentAsync(request, NetsisItemSlipDocumentType.WarehouseTransferOut, cancellationToken);

        public Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferInAsync(NetsisItemSlipCreateDto request, CancellationToken cancellationToken = default)
            => CreateDocumentAsync(request, NetsisItemSlipDocumentType.WarehouseTransferIn, cancellationToken);

        public Task<NetsisItemSlipCreateResponseDto> CreateDocumentAsync(
            NetsisItemSlipCreateDto request,
            NetsisItemSlipDocumentType documentType,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new NetsisItemSlipCreateResponseDto
            {
                IsSuccessful = true,
                IsSuccessStatusCode = true,
                Data = new NetsisItemSlipResponseDataDto
                {
                    FisNo = request.FatUst.FatirsNo ?? $"TEST-{documentType}-{Guid.NewGuid():N}"[..20]
                }
            });
        }
    }

    private sealed class SqliteTestAquaDbContext : AquaDbContext
    {
        public SqliteTestAquaDbContext(DbContextOptions<AquaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var columnType = property.GetColumnType();
                    if (!string.IsNullOrWhiteSpace(columnType) && columnType.Contains("max", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetColumnType("TEXT");
                    }
                }
            }

            var feedingEntity = modelBuilder.Model.FindEntityType(typeof(Feeding));
            var feedingDateOnly = feedingEntity?.FindProperty("FeedingDateOnly");
            feedingDateOnly?.SetAnnotation("Relational:ComputedColumnSql", "date(FeedingDate)");
            feedingDateOnly?.SetAnnotation("Relational:IsStored", true);
        }
    }
}
