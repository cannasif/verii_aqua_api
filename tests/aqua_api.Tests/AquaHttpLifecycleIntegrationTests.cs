using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Dtos;
using aqua_api.Modules.Aqua.Domain.Entities;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaHttpLifecycleIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaHttpTestWebApplicationFactory _factory;

    public AquaHttpLifecycleIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HttpLifecycle_OpeningToShipment_KeepsEndpointsAndReportsAligned()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "http-lifecycle.xlsx",
            SourceSystem = "http-integration-test",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["projectName"] = "HTTP Integration Farm",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["cageCode"] = "CAGE-HTTP-01",
                            ["cageName"] = "HTTP Main Cage",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["cageCode"] = "CAGE-HTTP-01",
                            ["batchCode"] = "BATCH-HTTP-PLAMUT-5G",
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "10000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long projectCageId;
        long openingBatchId;
        long fish10StockId;
        long feedStockId;
        long warehouseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var project = await db.Projects.SingleAsync(x => !x.IsDeleted && x.ProjectCode == "PRJ-HTTP-001");
            var projectCage = await db.ProjectCages.Include(x => x.Cage).SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id);
            var openingBatch = await db.FishBatches.SingleAsync(x => !x.IsDeleted && x.BatchCode == "BATCH-HTTP-PLAMUT-5G");
            var fish10Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-10G");
            var feedStock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD");
            var warehouse = await db.Warehouses.SingleAsync(x => !x.IsDeleted && x.ErpWarehouseCode == 10);

            projectId = project.Id;
            projectCageId = projectCage.Id;
            openingBatchId = openingBatch.Id;
            fish10StockId = fish10Stock.Id;
            feedStockId = feedStock.Id;
            warehouseId = warehouse.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            await AquaHttpTestWebApplicationFactory.SeedFeedPurchaseHistoryAsync(db, projectId, warehouseId, feedStockId);
        }

        var convertedBatch = await PostAsync<FishBatchDto>(client, "/api/aqua/FishBatch", new CreateFishBatchDto
        {
            ProjectId = projectId,
            BatchCode = "BATCH-HTTP-PLAMUT-10G",
            FishStockId = fish10StockId,
            CurrentAverageGram = 10m,
            StartDate = new DateTime(2026, 4, 3),
            TargetHarvestAverageGram = 20m,
        });
        Assert.True(convertedBatch.Success, $"{convertedBatch.Message} | {convertedBatch.ExceptionMessage}");
        var convertedBatchId = convertedBatch.Data!.Id;

        var feedingDay2 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 2),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 20m,
            GramPerUnit = 1000m,
            TotalGram = 20_000m,
        });
        Assert.True(feedingDay2.Success, $"{feedingDay2.Message} | {feedingDay2.ExceptionMessage}");
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay2.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 20_000m,
        })).Success);

        var mortalityDay2 = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 4, 2),
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 100,
        });
        Assert.True(mortalityDay2.Success, $"{mortalityDay2.Message} | {mortalityDay2.ExceptionMessage}");

        var feedingDay3 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 3),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 25m,
            GramPerUnit = 1000m,
            TotalGram = 25_000m,
        });
        Assert.True(feedingDay3.Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay3.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 25_000m,
        })).Success);

        var stockConvert = await PostAsync<StockConvertLineDto>(client, "/api/aqua/StockConvertLine/auto-header", new CreateStockConvertLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ConvertDate = new DateTime(2026, 4, 3),
            FromFishBatchId = openingBatchId,
            ToFishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            ToProjectCageId = projectCageId,
            FishCount = 4_000,
            AverageGram = 5m,
            NewAverageGram = 5m,
            BiomassGram = 20_000m,
        });
        Assert.True(stockConvert.Success, $"{stockConvert.Message} | {stockConvert.ExceptionMessage}");

        var feedingDay4 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 4),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 18m,
            GramPerUnit = 1000m,
            TotalGram = 18_000m,
        });
        Assert.True(feedingDay4.Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 10_000m,
        })).Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = convertedBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 8_000m,
        })).Success);

        var cageWarehouseLine = await PostAsync<CageWarehouseTransferLineDto>(client, "/api/aqua/CageWarehouseTransferLine/auto-header", new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            ToWarehouseId = warehouseId,
            FishCount = 1_500,
            AverageGram = 10m,
            BiomassGram = 15_000m,
        });
        Assert.True(cageWarehouseLine.Success);

        long cageWarehouseHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            cageWarehouseHeaderId = await db.CageWarehouseTransfers
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.TransferDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/cage-warehouse-transfer/{cageWarehouseHeaderId}", new { })).Success);

        var warehouseCageLine = await PostAsync<WarehouseCageTransferLineDto>(client, "/api/aqua/WarehouseCageTransferLine/auto-header", new CreateWarehouseCageTransferLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromWarehouseId = warehouseId,
            ToProjectCageId = projectCageId,
            FishCount = 500,
            AverageGram = 10m,
            BiomassGram = 5_000m,
        });
        Assert.True(warehouseCageLine.Success);

        long warehouseCageHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            warehouseCageHeaderId = await db.WarehouseCageTransfers
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.TransferDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/warehouse-cage-transfer/{warehouseCageHeaderId}", new { })).Success);

        Assert.True((await PostAsync<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 1_000,
            AverageGram = 10m,
            BiomassGram = 10_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 210m,
        })).Success);

        Assert.True((await PostAsync<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 200,
            AverageGram = 10m,
            BiomassGram = 2_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 230m,
        })).Success);

        long shipmentHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            shipmentHeaderId = await db.Shipments
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.ShipmentDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/shipment/{shipmentHeaderId}", new { })).Success);

        var mortalityDay4 = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 4, 4),
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 50,
        });
        Assert.True(mortalityDay4.Success);

        var snapshot = await PostAsync<List<ProjectCageDailyKpiSnapshotDto>>(client, "/api/aqua/ProjectCageDailyKpi/snapshot", new CreateProjectCageDailyKpiSnapshotRequest
        {
            ProjectId = projectId,
            SnapshotDate = new DateTime(2026, 4, 4),
        });
        Assert.True(snapshot.Success, $"{snapshot.Message} | {snapshot.ExceptionMessage}");
        Assert.Equal(2, snapshot.Data!.Count);

        var latestKpis = await GetAsync<List<ProjectCageDailyKpiSnapshotDto>>(client, $"/api/aqua/ProjectCageDailyKpi?projectId={projectId}&snapshotDate=2026-04-04");
        Assert.True(latestKpis.Success);
        Assert.Equal(2, latestKpis.Data!.Count);
        Assert.Equal(7_650, latestKpis.Data.Sum(x => x.LiveCount));
        Assert.Equal(47.25m, latestKpis.Data.Sum(x => x.BiomassKg));
        Assert.Equal(63m, latestKpis.Data.Sum(x => x.FeedKgPeriod));

        var cageBalances = await GetAsync<PagedResponse<BatchCageBalanceDto>>(client, "/api/aqua/BatchCageBalance");
        Assert.True(cageBalances.Success);
        Assert.True(cageBalances.Data!.Items.Count >= 2);

        var movements = await GetAsync<PagedResponse<BatchMovementDto>>(client, "/api/aqua/BatchMovement");
        Assert.True(movements.Success);
        Assert.True(movements.Data!.TotalCount >= 15);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var openingBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == openingBatchId && x.ProjectCageId == projectCageId);
            var convertedBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCageId);
            var warehouseBalance = await db.BatchWarehouseBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouseId);
            var shipmentLines = await db.ShipmentLines
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToListAsync();

            Assert.Equal(5_850, openingBalance.LiveCount);
            Assert.Equal(29_250m, openingBalance.BiomassGram);
            Assert.Equal(1_800, convertedBalance.LiveCount);
            Assert.Equal(18_000m, convertedBalance.BiomassGram);
            Assert.Equal(1_000, warehouseBalance.LiveCount);
            Assert.Equal(10_000m, warehouseBalance.BiomassGram);

            var weightedFeedCostPerKg = CalculateWeightedFeedCostPerKg(await db.GoodsReceiptLines.Where(x => !x.IsDeleted && x.StockId == feedStockId).ToListAsync());
            var weightedSalePricePerKg = CalculateWeightedSalePricePerKg(shipmentLines);
            Assert.Equal(61.079m, weightedFeedCostPerKg);
            Assert.Equal(213.333m, weightedSalePricePerKg);
        }
    }

    private static async Task<ApiResponse<T>> PostAsync<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }

    private static async Task<ApiResponse<T>> GetAsync<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }

    private static decimal CalculateWeightedFeedCostPerKg(List<GoodsReceiptLine> lines)
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

    private static decimal CalculateWeightedSalePricePerKg(List<ShipmentLine> lines)
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
}
