using System.Globalization;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Dtos;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Entities;
using aqua_api.Modules.Aqua.Domain.Enums;
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
        services.AddStockModule();
        services.AddWarehouseModule();
        services.AddAquaModule();
        services.AddScoped<IErpService, FakeErpService>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureCreatedAsync();

        await SeedMasterDataAsync(db);

        var openingImportService = scope.ServiceProvider.GetRequiredService<IOpeningImportService>();
        var fishBatchService = scope.ServiceProvider.GetRequiredService<IFishBatchService>();
        var feedingLineService = scope.ServiceProvider.GetRequiredService<IFeedingLineService>();
        var feedingDistributionService = scope.ServiceProvider.GetRequiredService<IFeedingDistributionService>();
        var mortalityLineService = scope.ServiceProvider.GetRequiredService<IMortalityLineService>();
        var stockConvertLineService = scope.ServiceProvider.GetRequiredService<IStockConvertLineService>();
        var cageWarehouseTransferLineService = scope.ServiceProvider.GetRequiredService<ICageWarehouseTransferLineService>();
        var cageWarehouseTransferService = scope.ServiceProvider.GetRequiredService<ICageWarehouseTransferService>();
        var warehouseCageTransferLineService = scope.ServiceProvider.GetRequiredService<IWarehouseCageTransferLineService>();
        var warehouseCageTransferService = scope.ServiceProvider.GetRequiredService<IWarehouseCageTransferService>();
        var shipmentLineService = scope.ServiceProvider.GetRequiredService<IShipmentLineService>();
        var shipmentService = scope.ServiceProvider.GetRequiredService<IShipmentService>();
        var kpiService = scope.ServiceProvider.GetRequiredService<IProjectCageDailyKpiService>();

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
        var openingBatch = await db.FishBatches.SingleAsync(x => !x.IsDeleted && x.BatchCode == "BATCH-PLAMUT-5G");
        var fish5Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G");
        var fish10Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-10G");
        var feedStock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD");
        var warehouse = await db.Warehouses.SingleAsync(x => !x.IsDeleted && x.ErpWarehouseCode == 10);

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

        var movements = await db.BatchMovements
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var movementSummary = string.Join(", ", movements
            .GroupBy(x => x.MovementType)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Count()}"));
        Assert.True(movements.Count == 15, movementSummary);
        Assert.Equal(1, movements.Count(x => x.MovementType == BatchMovementType.OpeningImport));
        Assert.Equal(4, movements.Count(x => x.MovementType == BatchMovementType.Feeding));
        Assert.Equal(2, movements.Count(x => x.MovementType == BatchMovementType.Mortality));
        Assert.Equal(2, movements.Count(x => x.MovementType == BatchMovementType.StockConvert));
        Assert.Equal(4, movements.Count(x => x.MovementType == BatchMovementType.WarehouseTransfer));
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

        db.Warehouses.Add(new Warehouse
        {
            ErpWarehouseCode = 10,
            WarehouseName = "Ana Depo",
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
