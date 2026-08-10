using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaSettings.Application.Dtos;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.Cages.Application.Dtos;
using aqua_api.Modules.FishGrowths.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.Projects.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaTwoCageLifecycleHttpIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaHttpTestWebApplicationFactory _factory;

    public AquaTwoCageLifecycleHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TwoCageLifecycle_AllHttpOperationsAndReportsRemainMathematicallyAligned()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var project = await PostOk<ProjectDto>(client, "/api/aqua/Project", new CreateProjectDto
        {
            ProjectCode = "E2E-TWO-CAGE-001",
            ProjectName = "E2E Two Cage Lifecycle",
            StartDate = new DateTime(2026, 1, 1),
            Status = DocumentStatus.Posted,
        });
        var cageA = await PostOk<CageDto>(client, "/api/aqua/Cage", new CreateCageDto
        {
            CageCode = "E2E-CAGE-A",
            CageName = "E2E Cage A",
            CapacityCount = 10_000,
            CapacityGram = 5_000_000m,
        });
        var cageB = await PostOk<CageDto>(client, "/api/aqua/Cage", new CreateCageDto
        {
            CageCode = "E2E-CAGE-B",
            CageName = "E2E Cage B",
            CapacityCount = 10_000,
            CapacityGram = 5_000_000m,
        });
        var projectCageA = await PostOk<ProjectCageDto>(client, "/api/aqua/ProjectCage", new CreateProjectCageDto
        {
            ProjectId = project.Id,
            CageId = cageA.Id,
            AssignedDate = new DateTime(2026, 1, 1),
        });
        var projectCageB = await PostOk<ProjectCageDto>(client, "/api/aqua/ProjectCage", new CreateProjectCageDto
        {
            ProjectId = project.Id,
            CageId = cageB.Id,
            AssignedDate = new DateTime(2026, 1, 1),
        });

        long fishStockId;
        long feedStockId;
        long warehouseId;
        long secondWarehouseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            fishStockId = await db.Stocks.Where(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            feedStockId = await db.Stocks.Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
            warehouseId = await db.Warehouses.Where(x => !x.IsDeleted && x.ErpWarehouseCode == 10).Select(x => x.Id).SingleAsync();
            var secondWarehouse = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
            {
                ErpWarehouseCode = 20,
                WarehouseName = "E2E Secondary Warehouse",
                BranchCode = 1,
                AllowNegativeBalance = false,
                IsLocked = false,
            };
            db.Warehouses.Add(secondWarehouse);
            await db.SaveChangesAsync();
            secondWarehouseId = secondWarehouse.Id;
        }

        var batch = await PostOk<FishBatchDto>(client, "/api/aqua/FishBatch", new CreateFishBatchDto
        {
            ProjectId = project.Id,
            BatchCode = "E2E-BATCH-001",
            FishStockId = fishStockId,
            CurrentAverageGram = 100m,
            StartDate = new DateTime(2026, 1, 1),
            TargetHarvestAverageGram = 250m,
        });
        var receipt = await PostOk<GoodsReceiptDto>(client, "/api/aqua/GoodsReceipt", new CreateGoodsReceiptDto
        {
            ProjectId = project.Id,
            ReceiptNo = "E2E-GR-001",
            ReceiptDate = new DateTime(2026, 1, 1),
            Status = DocumentStatus.Draft,
        });
        var receiptLine = await PostOk<GoodsReceiptLineDto>(client, "/api/aqua/GoodsReceiptLine", new CreateGoodsReceiptLineDto
        {
            GoodsReceiptId = receipt.Id,
            ItemType = GoodsReceiptItemType.Fish,
            StockId = fishStockId,
            FishBatchId = batch.Id,
            FishCount = 3_000,
            FishAverageGram = 100m,
            FishTotalGram = 300_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 4m,
        });
        await PostOk<GoodsReceiptFishDistributionDto>(client, "/api/aqua/GoodsReceiptFishDistribution", new CreateGoodsReceiptFishDistributionDto
        {
            GoodsReceiptLineId = receiptLine.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            FishCount = 1_800,
        });
        await PostOk<GoodsReceiptFishDistributionDto>(client, "/api/aqua/GoodsReceiptFishDistribution", new CreateGoodsReceiptFishDistributionDto
        {
            GoodsReceiptLineId = receiptLine.Id,
            ProjectCageId = projectCageB.Id,
            FishBatchId = batch.Id,
            FishCount = 1_200,
        });
        await PostOk<bool>(client, $"/api/aqua/posting/goods-receipt/{receipt.Id}", new { });

        await AssertCageBalance(batch.Id, projectCageA.Id, 1_800, 100m, 180_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 100m, 120_000m);
        await AssertProjectReports(client, project.Id, expectedCageFish: 3_000, expectedWarehouseFish: 0, expectedTotalBiomassKg: 300m, expectedCageCount: 2);

        await Feed(client, project.Id, projectCageA.Id, batch.Id, feedStockId, new DateTime(2026, 1, 5), FeedingSlot.Morning, 12m);
        await Feed(client, project.Id, projectCageB.Id, batch.Id, feedStockId, new DateTime(2026, 1, 5), FeedingSlot.Morning, 8m);
        await Feed(client, project.Id, projectCageA.Id, batch.Id, feedStockId, new DateTime(2026, 1, 5), FeedingSlot.Evening, 5m);
        await Mortality(client, project.Id, projectCageA.Id, batch.Id, new DateTime(2026, 1, 10), 20);
        await Mortality(client, project.Id, projectCageB.Id, batch.Id, new DateTime(2026, 1, 10), 10);

        await AssertCageBalance(batch.Id, projectCageA.Id, 1_780, 100m, 178_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_190, 100m, 119_000m);

        var januaryFeed = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-feedings", Range(project.Id, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)));
        Assert.Equal(25m, januaryFeed.TotalKg);
        Assert.Equal(3, januaryFeed.TotalLineCount);
        Assert.Equal(2, januaryFeed.TotalCageCount);

        var januaryMortality = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(project.Id, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)));
        Assert.Equal(30, januaryMortality.TotalCount);
        Assert.Equal(1.5m, januaryMortality.TotalKg);
        Assert.Equal(2, januaryMortality.TotalLineCount);
        Assert.Equal(2, januaryMortality.TotalCageCount);

        var mortalityTracking = await PostOk<MortalityTrackingReportDto>(client, "/api/kpi-report/mortality-tracking", Range(project.Id, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)));
        Assert.Equal(30, mortalityTracking.TotalCount);
        Assert.Equal(1.5m, mortalityTracking.TotalKg);
        Assert.Equal(2, mortalityTracking.TotalCageCount);
        await AssertProjectReports(client, project.Id, expectedCageFish: 2_970, expectedWarehouseFish: 0, expectedTotalBiomassKg: 297m, expectedCageCount: 2);

        var growthA = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 2, 1),
            NewAverageGram = 150m,
        });
        var growthB = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageB.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 2, 1),
            NewAverageGram = 140m,
        });
        Assert.Equal(50m, growthA.GrowthGram);
        Assert.Equal(40m, growthB.GrowthGram);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_780, 150m, 267_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_190, 140m, 166_600m);

        using (var duplicateGrowthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 2, 20),
            NewAverageGram = 155m,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, duplicateGrowthResponse.StatusCode);
            var body = await duplicateGrowthResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>(JsonOptions);
            Assert.False(body?.Success);
        }

        var timelineA = await GetOk<FishGrowthTimelineDto>(client,
            $"/api/aqua/FishGrowth/timeline?projectCageId={projectCageA.Id}&fishBatchId={batch.Id}&throughYear=2026&throughMonth=2");
        Assert.False(timelineA.HasContinuityIssue);
        Assert.Equal(150m, timelineA.LatestAverageGram);

        await Feed(client, project.Id, projectCageA.Id, batch.Id, feedStockId, new DateTime(2026, 2, 3), FeedingSlot.Morning, 10m);
        await Feed(client, project.Id, projectCageB.Id, batch.Id, feedStockId, new DateTime(2026, 2, 3), FeedingSlot.Morning, 7m);

        var partialTransferRequest = new CreateTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TargetProjectId = project.Id,
            TransferDate = new DateTime(2026, 2, 5),
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            ToProjectCageId = projectCageB.Id,
            FishCount = 200,
            AverageGram = 1m,
            BiomassGram = 200m,
        };

        using (var rejectedTransferResponse = await client.PostAsJsonAsync("/api/aqua/TransferLine/auto-header", partialTransferRequest))
        {
            Assert.Equal(HttpStatusCode.BadRequest, rejectedTransferResponse.StatusCode);
            var body = await rejectedTransferResponse.Content.ReadFromJsonAsync<ApiResponse<TransferLineDto>>(JsonOptions);
            Assert.False(body?.Success);
            Assert.Contains("Tüm balıklar", body?.ExceptionMessage);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.False(await db.Transfers.AnyAsync(x =>
                !x.IsDeleted &&
                x.ProjectId == project.Id &&
                x.TransferDate.Date == new DateTime(2026, 2, 5)));
            Assert.False(await db.TransferLines.AnyAsync(x =>
                !x.IsDeleted &&
                x.FishBatchId == batch.Id &&
                x.FromProjectCageId == projectCageA.Id &&
                x.ToProjectCageId == projectCageB.Id));
        }

        await PostOk<AquaSettingsDto>(client, "/api/aqua/AquaSettings/update", new UpdateAquaSettingsDto
        {
            RequireFullTransfer = false,
            AllowProjectMerge = false,
            PartialTransferOccupiedCageMode = 2,
            FeedCostFallbackStrategy = 0,
        });

        var transfer = await PostOk<TransferLineDto>(client, "/api/aqua/TransferLine/auto-header", partialTransferRequest);
        Assert.Equal(150m, transfer.AverageGram);
        Assert.Equal(30_000m, transfer.BiomassGram);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_580, 150m, 237_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_390, 141.439m, 196_600m);

        await Mortality(client, project.Id, projectCageB.Id, batch.Id, new DateTime(2026, 2, 8), 10);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_380, 141.439m, 195_185.610m);

        var shipmentA = await PostOk<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 2, 10),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            FishCount = 300,
            AverageGram = 1m,
            BiomassGram = 300m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 50m,
        });
        var shipmentB = await PostOk<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 2, 11),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageB.Id,
            FishCount = 100,
            AverageGram = 1m,
            BiomassGram = 100m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 60m,
        });
        Assert.Equal(150m, shipmentA.AverageGram);
        Assert.Equal(45_000m, shipmentA.BiomassGram);
        Assert.Equal(2_250m, shipmentA.LineAmount);
        Assert.Equal(141.439m, shipmentB.AverageGram);
        Assert.Equal(14_143.900m, shipmentB.BiomassGram);
        Assert.Equal(848.634m, shipmentB.LineAmount);

        await AssertCageBalance(batch.Id, projectCageA.Id, 1_280, 150m, 192_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_280, 141.439m, 181_041.710m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 400, 147.860m, 59_143.900m);

        var februaryShipments = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-shipments", Range(project.Id, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28)));
        Assert.Equal(400, februaryShipments.TotalCount);
        Assert.Equal(59.144m, februaryShipments.TotalKg);
        Assert.Equal(3_098.634m, februaryShipments.TotalAmount);
        Assert.Equal(2, februaryShipments.TotalLineCount);
        Assert.Equal(2, februaryShipments.TotalCageCount);

        var allMortality = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(project.Id, new DateTime(2026, 1, 1), new DateTime(2026, 2, 28)));
        Assert.Equal(40, allMortality.TotalCount);
        Assert.Equal(2.207m, allMortality.TotalKg);

        await AssertProjectReports(client, project.Id, expectedCageFish: 2_560, expectedWarehouseFish: 400, expectedTotalBiomassKg: 432.186m, expectedCageCount: 2);

        var devir = await PostOk<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [project.Id],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 2, 28),
        });
        var devirRow = Assert.Single(devir.Rows);
        Assert.Equal(3_000, devirRow.OpeningFishCount);
        Assert.Equal(400, devirRow.ShipmentFishCount);
        Assert.Equal(40, devirRow.MortalityFishCount);
        Assert.Equal(2.207m, devirRow.MortalityBiomassKg);
        Assert.Equal(42m, devirRow.TotalFeedKg);
        Assert.Equal(2_560, devirRow.EndingFishCount);
        Assert.Equal(373.042m, devirRow.EndingBiomassKg);
        Assert.Equal(0, devirRow.OpeningFishCount - devirRow.ShipmentFishCount - devirRow.MortalityFishCount - devirRow.EndingFishCount);

        var projectDetail = await GetOk<ProjectDetailReportDto>(client, $"/api/kpi-report/project-detail/{project.Id}");
        Assert.Equal(2, projectDetail.Cages.Count);
        Assert.Equal(400, projectDetail.WarehouseSummary.WarehouseFishCount);
        Assert.Equal(2_960, projectDetail.WarehouseSummary.TotalSystemFishCount);
        Assert.Equal(432_185.610m, projectDetail.WarehouseSummary.TotalSystemBiomassGram);

        var marchGrowthA = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 3, 1),
            NewAverageGram = 200m,
        });
        var marchGrowthB = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageB.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 3, 1),
            NewAverageGram = 190m,
        });
        Assert.Equal(50m, marchGrowthA.GrowthGram);
        Assert.Equal(48.561m, marchGrowthB.GrowthGram);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_280, 200m, 256_000m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_280, 190m, 243_200m);

        await Feed(client, project.Id, projectCageA.Id, batch.Id, feedStockId, new DateTime(2026, 3, 3), FeedingSlot.Morning, 6m);
        await Feed(client, project.Id, projectCageB.Id, batch.Id, feedStockId, new DateTime(2026, 3, 3), FeedingSlot.Morning, 4m);
        using (var duplicateFeedResponse = await client.PostAsJsonAsync("/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            FeedingDate = new DateTime(2026, 3, 3),
            FeedingSlot = FeedingSlot.Morning,
            SourceType = FeedingSourceType.Manual,
            StockId = feedStockId,
            QtyUnit = 6m,
            GramPerUnit = 1000m,
            TotalGram = 6_000m,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, duplicateFeedResponse.StatusCode);
        }

        await Mortality(client, project.Id, projectCageA.Id, batch.Id, new DateTime(2026, 3, 5), 8);
        await Mortality(client, project.Id, projectCageB.Id, batch.Id, new DateTime(2026, 3, 5), 5);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_272, 200m, 254_400m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_275, 190m, 242_250m);

        var marchShipment = await PostOk<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 3, 10),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageB.Id,
            FishCount = 75,
            AverageGram = 1m,
            BiomassGram = 75m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 70m,
        });
        Assert.Equal(190m, marchShipment.AverageGram);
        Assert.Equal(14_250m, marchShipment.BiomassGram);
        Assert.Equal(997.5m, marchShipment.LineAmount);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 190m, 228_000m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 154.513m, 73_393.900m);

        var updatedMarchGrowthB = await PutOk<FishGrowthDto>(client, $"/api/aqua/FishGrowth/{marchGrowthB.Id}", new UpdateFishGrowthDto
        {
            NewAverageGram = 195m,
            Description = "Replay downstream mortality and shipment",
        });
        Assert.Equal(195m, updatedMarchGrowthB.NewAverageGram);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 195m, 234_000m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 155.303m, 73_768.900m);

        marchShipment = await GetOk<ShipmentLineDto>(client, $"/api/aqua/ShipmentLine/{marchShipment.Id}");
        Assert.Equal(195m, marchShipment.AverageGram);
        Assert.Equal(14_625m, marchShipment.BiomassGram);
        Assert.Equal(1_023.75m, marchShipment.LineAmount);

        var marchFeed = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-feedings", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(10m, marchFeed.TotalKg);
        Assert.Equal(2, marchFeed.TotalLineCount);
        Assert.Equal(2, marchFeed.TotalCageCount);

        var marchMortality = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(13, marchMortality.TotalCount);
        Assert.Equal(1.288m, marchMortality.TotalKg);

        var marchShipments = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-shipments", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(75, marchShipments.TotalCount);
        Assert.Equal(14.625m, marchShipments.TotalKg);
        Assert.Equal(1_023.75m, marchShipments.TotalAmount);

        await AssertProjectReports(client, project.Id, expectedCageFish: 2_472, expectedWarehouseFish: 475, expectedTotalBiomassKg: 562.169m, expectedCageCount: 2);

        var firstQuarterDevir = await PostOk<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [project.Id],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 3, 31),
        });
        var firstQuarterRow = Assert.Single(firstQuarterDevir.Rows);
        Assert.Equal(3_000, firstQuarterRow.OpeningFishCount);
        Assert.Equal(475, firstQuarterRow.ShipmentFishCount);
        Assert.Equal(53, firstQuarterRow.MortalityFishCount);
        Assert.Equal(3.495m, firstQuarterRow.MortalityBiomassKg);
        Assert.Equal(52m, firstQuarterRow.TotalFeedKg);
        Assert.Equal(2_472, firstQuarterRow.EndingFishCount);
        Assert.Equal(488.400m, firstQuarterRow.EndingBiomassKg);
        Assert.Equal(0, firstQuarterRow.OpeningFishCount - firstQuarterRow.ShipmentFishCount - firstQuarterRow.MortalityFishCount - firstQuarterRow.EndingFishCount);

        await DeleteOk(client, $"/api/aqua/ShipmentLine/{marchShipment.Id}");
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_275, 195m, 248_625m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 400, 147.860m, 59_143.900m);
        var marchAfterShipmentDelete = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-shipments", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(0, marchAfterShipmentDelete.TotalCount);
        Assert.Equal(0m, marchAfterShipmentDelete.TotalKg);

        marchShipment = await PostOk<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 3, 10),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageB.Id,
            FishCount = 75,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 70m,
        });
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 195m, 234_000m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 155.303m, 73_768.900m);

        await DeleteOk(client, $"/api/aqua/FishGrowth/{marchGrowthB.Id}");
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 141.439m, 169_726.590m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 146.846m, 69_751.825m);
        var replayedShipmentWithoutMarchGrowth = await GetOk<ShipmentLineDto>(client, $"/api/aqua/ShipmentLine/{marchShipment.Id}");
        Assert.Equal(141.439m, replayedShipmentWithoutMarchGrowth.AverageGram);
        Assert.Equal(10_607.925m, replayedShipmentWithoutMarchGrowth.BiomassGram);
        Assert.Equal(742.55475m, replayedShipmentWithoutMarchGrowth.LineAmount);
        var marchWithoutGrowthMortality = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(1.154m, marchWithoutGrowthMortality.TotalKg);
        var marchWithoutGrowthShipment = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-shipments", Range(project.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)));
        Assert.Equal(10.608m, marchWithoutGrowthShipment.TotalKg);
        await AssertProjectReports(client, project.Id, expectedCageFish: 2_472, expectedWarehouseFish: 475, expectedTotalBiomassKg: 493.878m, expectedCageCount: 2);

        marchGrowthB = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageB.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 3, 1),
            NewAverageGram = 195m,
        });
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 195m, 234_000m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 155.303m, 73_768.900m);

        using (var invalidGrowthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 4, 1),
            NewAverageGram = 190m,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, invalidGrowthResponse.StatusCode);
        }

        using (var invalidMortalityResponse = await client.PostAsJsonAsync("/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageA.Id,
            FishBatchId = batch.Id,
            MortalityDate = new DateTime(2026, 4, 2),
            DeadCount = 5_000,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, invalidMortalityResponse.StatusCode);
        }

        using (var invalidShipmentResponse = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 4, 3),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            FishCount = 5_000,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 80m,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, invalidShipmentResponse.StatusCode);
        }

        using (var invalidSameCageTransferResponse = await client.PostAsJsonAsync("/api/aqua/TransferLine/auto-header", new CreateTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TargetProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            ToProjectCageId = projectCageA.Id,
            FishCount = 10,
        }))
        {
            Assert.Equal(HttpStatusCode.Conflict, invalidSameCageTransferResponse.StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.False(await db.FishGrowths.AnyAsync(x => !x.IsDeleted && x.ProjectCageId == projectCageA.Id && x.GrowthYear == 2026 && x.GrowthMonth == 4));
            Assert.False(await db.Mortalities.AnyAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.MortalityDate.Date == new DateTime(2026, 4, 2)));
            Assert.False(await db.Shipments.AnyAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.ShipmentDate.Date == new DateTime(2026, 4, 3)));
            Assert.False(await db.Transfers.AnyAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 4, 4)));
        }

        await AssertCageBalance(batch.Id, projectCageA.Id, 1_272, 200m, 254_400m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 195m, 234_000m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 475, 155.303m, 73_768.900m);
        await AssertProjectReports(client, project.Id, expectedCageFish: 2_472, expectedWarehouseFish: 475, expectedTotalBiomassKg: 562.169m, expectedCageCount: 2);

        var aprilGrowthB = await PostOk<FishGrowthDto>(client, "/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = project.Id,
            ProjectCageId = projectCageB.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 4, 1),
            NewAverageGram = 210m,
        });
        Assert.Equal(15m, aprilGrowthB.GrowthGram);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_200, 210m, 252_000m);

        var cageWarehouseTransfer = await PostOk<CageWarehouseTransferLineDto>(client, "/api/aqua/CageWarehouseTransferLine/auto-header-and-post", new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 5),
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            ToWarehouseId = warehouseId,
            FishCount = 100,
            AverageGram = 1m,
            BiomassGram = 100m,
        });
        Assert.Equal(200m, cageWarehouseTransfer.AverageGram);
        Assert.Equal(20_000m, cageWarehouseTransfer.BiomassGram);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_172, 200m, 234_400m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 575, 163.076m, 93_768.900m);

        var warehouseTransfer = await PostOk<WarehouseTransferLineDto>(client, "/api/aqua/WarehouseTransferLine/auto-header-and-post", new CreateWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 6),
            FishBatchId = batch.Id,
            FromWarehouseId = warehouseId,
            ToWarehouseId = secondWarehouseId,
            FishCount = 50,
            AverageGram = 1m,
            BiomassGram = 50m,
        });
        Assert.Equal(163.076m, warehouseTransfer.AverageGram);
        Assert.Equal(8_153.800m, warehouseTransfer.BiomassGram);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 525, 163.076m, 85_615.100m);
        await AssertWarehouseBalance(project.Id, batch.Id, secondWarehouseId, 50, 163.076m, 8_153.800m);

        var warehouseCageTransfer = await PostOk<WarehouseCageTransferLineDto>(client, "/api/aqua/WarehouseCageTransferLine/auto-header-and-post", new CreateWarehouseCageTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 4, 7),
            FishBatchId = batch.Id,
            FromWarehouseId = secondWarehouseId,
            ToProjectCageId = projectCageB.Id,
            FishCount = 20,
            AverageGram = 1m,
            BiomassGram = 20m,
        });
        Assert.Equal(163.076m, warehouseCageTransfer.AverageGram);
        Assert.Equal(3_261.520m, warehouseCageTransfer.BiomassGram);
        await AssertWarehouseBalance(project.Id, batch.Id, secondWarehouseId, 30, 163.076m, 4_892.280m);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_220, 209.231m, 255_261.520m);
        await AssertProjectReports(client, project.Id, expectedCageFish: 2_392, expectedWarehouseFish: 555, expectedTotalBiomassKg: 580.169m, expectedCageCount: 2);

        using (var growthAfterTransferResponse = await client.PutAsJsonAsync($"/api/aqua/FishGrowth/{aprilGrowthB.Id}", new UpdateFishGrowthDto
        {
            NewAverageGram = 215m,
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, growthAfterTransferResponse.StatusCode);
        }
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_220, 209.231m, 255_261.520m);

        await Feed(client, project.Id, projectCageA.Id, batch.Id, feedStockId, new DateTime(2026, 4, 9), FeedingSlot.Morning, 3m);
        await Feed(client, project.Id, projectCageB.Id, batch.Id, feedStockId, new DateTime(2026, 4, 9), FeedingSlot.Morning, 3m);
        await Mortality(client, project.Id, projectCageB.Id, batch.Id, new DateTime(2026, 4, 10), 10);
        await AssertCageBalance(batch.Id, projectCageB.Id, 1_210, 209.231m, 253_169.210m);

        var aprilShipment = await PostOk<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header-and-post", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 4, 11),
            TargetWarehouseId = warehouseId,
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            FishCount = 50,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 80m,
        });
        Assert.Equal(200m, aprilShipment.AverageGram);
        Assert.Equal(10_000m, aprilShipment.BiomassGram);
        Assert.Equal(800m, aprilShipment.LineAmount);
        await AssertCageBalance(batch.Id, projectCageA.Id, 1_122, 200m, 224_400m);
        await AssertWarehouseBalance(project.Id, batch.Id, warehouseId, 575, 166.287m, 95_615.100m);

        var aprilFeed = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-feedings", Range(project.Id, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30)));
        Assert.Equal(6m, aprilFeed.TotalKg);
        var aprilMortality = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(project.Id, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30)));
        Assert.Equal(10, aprilMortality.TotalCount);
        Assert.Equal(1.046m, aprilMortality.TotalKg);
        var aprilShipments = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-shipments", Range(project.Id, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30)));
        Assert.Equal(50, aprilShipments.TotalCount);
        Assert.Equal(10m, aprilShipments.TotalKg);
        Assert.Equal(800m, aprilShipments.TotalAmount);
        await AssertProjectReports(client, project.Id, expectedCageFish: 2_332, expectedWarehouseFish: 605, expectedTotalBiomassKg: 578.077m, expectedCageCount: 2);

        var throughAprilDevir = await PostOk<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [project.Id],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 4, 30),
        });
        var throughAprilRow = Assert.Single(throughAprilDevir.Rows);
        Assert.Equal(525, throughAprilRow.ShipmentFishCount);
        Assert.Equal(63, throughAprilRow.MortalityFishCount);
        Assert.Equal(4.541m, throughAprilRow.MortalityBiomassKg);
        Assert.Equal(58m, throughAprilRow.TotalFeedKg);
        Assert.Equal(2_412, throughAprilRow.EndingFishCount);
        Assert.Equal(494.308m, throughAprilRow.EndingBiomassKg);
        Assert.Equal(0, throughAprilRow.OpeningFishCount - throughAprilRow.ShipmentFishCount - throughAprilRow.MortalityFishCount - throughAprilRow.EndingFishCount);

        using (var invalidCageWarehouseResponse = await client.PostAsJsonAsync("/api/aqua/CageWarehouseTransferLine/auto-header-and-post", new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 5, 1),
            FishBatchId = batch.Id,
            FromProjectCageId = projectCageA.Id,
            ToWarehouseId = warehouseId,
            FishCount = 5_000,
            AverageGram = 1m,
            BiomassGram = 5_000m,
        }))
        {
            var body = await invalidCageWarehouseResponse.Content.ReadFromJsonAsync<ApiResponse<CageWarehouseTransferLineDto>>(JsonOptions);
            Assert.True(
                invalidCageWarehouseResponse.StatusCode == HttpStatusCode.BadRequest,
                $"HTTP {(int)invalidCageWarehouseResponse.StatusCode} | {body?.Message} | {body?.ExceptionMessage}");
        }

        using (var invalidWarehouseTransferResponse = await client.PostAsJsonAsync("/api/aqua/WarehouseTransferLine/auto-header-and-post", new CreateWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            TransferDate = new DateTime(2026, 5, 2),
            FishBatchId = batch.Id,
            FromWarehouseId = secondWarehouseId,
            ToWarehouseId = warehouseId,
            FishCount = 5_000,
            AverageGram = 1m,
            BiomassGram = 5_000m,
        }))
        {
            var body = await invalidWarehouseTransferResponse.Content.ReadFromJsonAsync<ApiResponse<WarehouseTransferLineDto>>(JsonOptions);
            Assert.True(
                invalidWarehouseTransferResponse.StatusCode == HttpStatusCode.BadRequest,
                $"HTTP {(int)invalidWarehouseTransferResponse.StatusCode} | {body?.Message} | {body?.ExceptionMessage}");
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.False(await db.CageWarehouseTransfers.AnyAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 5, 1)));
            Assert.False(await db.WarehouseTransfers.AnyAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.TransferDate.Date == new DateTime(2026, 5, 2)));
        }

        var businessKpi = await GetOk<BusinessKpiReportDto>(client, $"/api/kpi-report/business-kpi/{project.Id}");
        Assert.Equal(2, businessKpi.Rows.Count);
        Assert.True(businessKpi.Assumptions.SalePricePerKg > 0m);
    }

    private async Task AssertProjectReports(
        HttpClient client,
        long projectId,
        int expectedCageFish,
        int expectedWarehouseFish,
        decimal expectedTotalBiomassKg,
        int expectedCageCount)
    {
        var dashboard = await PostOk<DashboardProjectsResponseDto>(client, "/api/aqua/dashboard-project/summary", new DashboardProjectsRequestDto
        {
            ProjectIds = [projectId]
        });
        var dashboardProject = Assert.Single(dashboard.Projects);
        Assert.Equal(expectedCageFish, dashboardProject.CageFish);
        Assert.Equal(expectedWarehouseFish, dashboardProject.WarehouseFish);
        Assert.Equal(expectedCageFish + expectedWarehouseFish, dashboardProject.TotalSystemFish);
        Assert.Equal(
            expectedTotalBiomassKg,
            decimal.Round(dashboardProject.TotalSystemBiomassGram / 1000m, 3, MidpointRounding.AwayFromZero));
        Assert.Equal(expectedCageCount, dashboardProject.Cages.Count);

        var summary = await PostOk<ProjectFeedFishSummaryReportDto>(client, "/api/kpi-report/project-feed-fish-summary", new ProjectFeedFishSummaryRequestDto
        {
            ProjectIds = [projectId]
        });
        var row = Assert.Single(summary.Rows);
        Assert.Equal(expectedCageFish, row.CageFish);
        Assert.Equal(expectedWarehouseFish, row.WarehouseFish);
        Assert.Equal(expectedCageFish + expectedWarehouseFish, row.TotalFish);
        Assert.Equal(expectedTotalBiomassKg, row.TotalBiomassKg);
        Assert.Equal(expectedCageCount, row.ActiveCageCount);

        var raw = await GetOk<RawKpiReportDto>(client, $"/api/kpi-report/raw-kpi/{projectId}");
        Assert.Equal(expectedCageFish, raw.LiveFish);
        Assert.Equal(expectedWarehouseFish, raw.WarehouseFish);
        Assert.Equal(expectedCageFish + expectedWarehouseFish, raw.TotalSystemFish);
        Assert.Equal(expectedTotalBiomassKg, raw.TotalSystemBiomassKg);
        Assert.Equal(expectedCageCount, raw.Rows.Count);
    }

    private async Task AssertCageBalance(long fishBatchId, long projectCageId, int count, decimal averageGram, decimal biomassGram)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var balance = await db.BatchCageBalances.SingleAsync(x =>
            !x.IsDeleted && x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
        Assert.Equal(count, balance.LiveCount);
        Assert.Equal(averageGram, balance.AverageGram);
        Assert.Equal(biomassGram, balance.BiomassGram);
    }

    private async Task AssertWarehouseBalance(long projectId, long fishBatchId, long warehouseId, int count, decimal averageGram, decimal biomassGram)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var balance = await db.BatchWarehouseBalances.SingleAsync(x =>
            !x.IsDeleted && x.ProjectId == projectId && x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId);
        Assert.Equal(count, balance.LiveCount);
        Assert.Equal(averageGram, balance.AverageGram);
        Assert.Equal(biomassGram, balance.BiomassGram);
    }

    private static async Task Feed(
        HttpClient client,
        long projectId,
        long projectCageId,
        long fishBatchId,
        long feedStockId,
        DateTime date,
        FeedingSlot slot,
        decimal feedKg)
    {
        await PostOk<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            FeedingDate = date,
            FeedingSlot = slot,
            SourceType = FeedingSourceType.Manual,
            StockId = feedStockId,
            QtyUnit = feedKg,
            GramPerUnit = 1000m,
            TotalGram = feedKg * 1000m,
        });
    }

    private static async Task Mortality(
        HttpClient client,
        long projectId,
        long projectCageId,
        long fishBatchId,
        DateTime date,
        int deadCount)
    {
        await PostOk<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            MortalityDate = date,
            DeadCount = deadCount,
        });
    }

    private static MonthlyOperationalReportRequestDto Range(long projectId, DateTime fromDate, DateTime toDate)
    {
        return new MonthlyOperationalReportRequestDto
        {
            ProjectIds = [projectId],
            FromDate = fromDate,
            ToDate = toDate,
        };
    }

    private static async Task<T> PostOk<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    private static async Task<T> GetOk<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    private static async Task<T> PutOk<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PutAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    private static async Task DeleteOk(HttpClient client, string url)
    {
        using var response = await client.DeleteAsync(url);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.True(body.Data);
    }
}
