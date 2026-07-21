using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Modules.Budget.Application.Dtos;
using aqua_api.Modules.Budget.Application.Services;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Application.Dtos;
using aqua_api.Modules.BudgetPlanning.Application.Services;
using aqua_api.Modules.BudgetKpi.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Common.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace aqua_api.Tests;

public class BudgetPlanningServiceIntegrationTests
{
    [Fact]
    public async Task AvailableFishBatches_SubtractsPostedShipmentsMissingFromLedger()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;

        var stock = new Stock { ErpStockCode = "L001", StockName = "Levrek", Unit = "AD" };
        var cage = new Cage { CageCode = "A1", CageName = "A1" };
        var project = new Project
        {
            ProjectCode = "20240331ILKNAK",
            ProjectName = "15. PROJE",
            StartDate = new DateTime(2026, 1, 1),
            Status = DocumentStatus.Posted
        };
        db.AddRange(stock, cage, project);
        await db.SaveChangesAsync();

        var projectCage = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = cage.Id,
            AssignedDate = project.StartDate
        };
        var fishBatch = new FishBatch
        {
            ProjectId = project.Id,
            FishStockId = stock.Id,
            BatchCode = "BATCH-004",
            CurrentAverageGram = 643.12m,
            StartDate = project.StartDate
        };
        db.AddRange(projectCage, fishBatch);
        await db.SaveChangesAsync();

        var balance = new BatchCageBalance
        {
            FishBatchId = fishBatch.Id,
            ProjectCageId = projectCage.Id,
            LiveCount = 728_489,
            AverageGram = 643.12m,
            BiomassGram = 468_505_730m,
            AsOfDate = new DateTime(2026, 3, 31)
        };
        db.BatchCageBalances.Add(balance);
        var shipment = new Shipment
        {
            ProjectId = project.Id,
            ShipmentNo = "LEGACY-SHIPMENT",
            ShipmentDate = new DateTime(2026, 7, 20),
            Status = DocumentStatus.Posted
        };
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        db.ShipmentLines.Add(new ShipmentLine
        {
            ShipmentId = shipment.Id,
            FishBatchId = fishBatch.Id,
            FromProjectCageId = projectCage.Id,
            FishCount = 301_022,
            AverageGram = 534.89m,
            BiomassGram = 161_012_000m
        });
        await db.SaveChangesAsync();

        var result = await fixture.Service.GetAvailableFishBatchesAsync();

        Assert.True(result.Success, result.Message);
        var row = Assert.Single(result.Data!);
        Assert.Equal(427_467, row.LiveCount);
        Assert.Equal(307_493.73m, row.BiomassKg);
        Assert.Equal(719.339m, row.AverageGram);
        Assert.Equal(new DateTime(2026, 7, 20), row.AsOfDate);

        balance.LiveCount = 427_467;
        balance.AverageGram = 719.339m;
        balance.BiomassGram = 307_493_730m;
        balance.AsOfDate = shipment.ShipmentDate;
        db.BatchMovements.Add(new BatchMovement
        {
            FishBatchId = fishBatch.Id,
            ProjectCageId = projectCage.Id,
            FromProjectCageId = projectCage.Id,
            MovementDate = shipment.ShipmentDate,
            MovementType = BatchMovementType.Shipment,
            SignedCount = -301_022,
            SignedBiomassGram = -161_012_000m,
            ReferenceTable = "RII_SHIPMENT",
            ReferenceId = shipment.Id
        });
        await db.SaveChangesAsync();

        var representedResult = await fixture.Service.GetAvailableFishBatchesAsync();

        var representedRow = Assert.Single(representedResult.Data!);
        Assert.Equal(427_467, representedRow.LiveCount);
        Assert.Equal(307_493.73m, representedRow.BiomassKg);
        Assert.Equal(719.339m, representedRow.AverageGram);

        var budgetPlan = new BudgetPlan
        {
            BudgetNo = "BUD-IMPORT-OVERRIDE",
            BudgetCode = "IMPORT-OVERRIDE",
            BudgetName = "Biological start override",
            StartYear = 2026,
            StartMonth = 3,
            EndYear = 2027,
            EndMonth = 3
        };
        db.BudgetPlans.Add(budgetPlan);
        await db.SaveChangesAsync();

        var imported = await fixture.Service.AddActualFishBatchesAsync(budgetPlan.Id, new AddActualFishBatchesToBudgetDto
        {
            FishBatchIds = new List<long> { fishBatch.Id },
            GrowthStartYear = 2024,
            GrowthStartMonth = 7
        });

        Assert.True(imported.Success, imported.Message);
        var importedBatch = Assert.Single(imported.Data!);
        Assert.Equal(720m, importedBatch.InitialAverageGram);
        Assert.Equal(307_776.24m, importedBatch.InitialBiomassKg);
        Assert.Equal(2024, importedBatch.GrowthStartYear);
        Assert.Equal(7, importedBatch.GrowthStartMonth);
    }

    [Fact]
    public async Task FishPrices_GenerateByEnteredMonthPeriod_AndKeepPriceDimensionsSeparate()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-PRICE", StockName = "Price Fish", Unit = "AD" };
        var calibration = new BudgetCalibrationDefinition { CalibrationCode = "K-PRICE", CalibrationInfo = "Price Calibration" };
        db.AddRange(fishStock, calibration);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        db.BudgetPlanExchangeRates.AddRange(
            new BudgetPlanExchangeRate
            {
                BudgetPlanId = plan.Id,
                Year = 2026,
                Month = 1,
                CurrencyCode = "EUR",
                RateType = "Budget",
                ExchangeRate = 40m,
                SourceType = "Manual",
                IsManualOverride = true
            },
            new BudgetPlanExchangeRate
            {
                BudgetPlanId = plan.Id,
                Year = 2026,
                Month = 4,
                CurrencyCode = "EUR",
                RateType = "Budget",
                ExchangeRate = 42m,
                SourceType = "Manual",
                IsManualOverride = true
            });
        await db.SaveChangesAsync();

        var salesResult = await service.GenerateFishPricesAsync(plan.Id, new GenerateBudgetPlanFishPricesDto
        {
            PriceType = BudgetFishPriceType.Sales,
            MarketType = BudgetMarketType.Domestic,
            CurrencyCode = "eur",
            DefaultUnitPrice = 100m,
            IncreaseRatePercent = 10m,
            IncreasePeriodMonths = 3,
            FishStockIds = [fishStock.Id],
            CalibrationDefinitionIds = [calibration.Id]
        });

        Assert.True(salesResult.Success, salesResult.Message);
        Assert.Equal(12, salesResult.Data!.Count);
        Assert.Equal(100m, salesResult.Data.Single(x => x.Month == 1).UnitPrice);
        Assert.Equal(100m, salesResult.Data.Single(x => x.Month == 3).UnitPrice);
        Assert.Equal(110m, salesResult.Data.Single(x => x.Month == 4).UnitPrice);
        Assert.Equal(110m, salesResult.Data.Single(x => x.Month == 6).UnitPrice);
        Assert.Equal(121m, salesResult.Data.Single(x => x.Month == 7).UnitPrice);
        Assert.Equal(4_000m, salesResult.Data.Single(x => x.Month == 1).UnitPriceTry);
        Assert.Equal(4_620m, salesResult.Data.Single(x => x.Month == 4).UnitPriceTry);
        Assert.Null(salesResult.Data.Single(x => x.Month == 2).UnitPriceTry);

        var purchaseResult = await service.GenerateFishPricesAsync(plan.Id, new GenerateBudgetPlanFishPricesDto
        {
            PriceType = BudgetFishPriceType.Purchase,
            MarketType = BudgetMarketType.Domestic,
            CurrencyCode = "TRY",
            DefaultUnitPrice = 25m,
            IncreasePeriodMonths = 1,
            FishStockIds = [fishStock.Id],
            CalibrationDefinitionIds = [calibration.Id]
        });

        Assert.True(purchaseResult.Success, purchaseResult.Message);
        Assert.Equal(24, purchaseResult.Data!.Count);
        var januaryRows = purchaseResult.Data.Where(x => x.Month == 1).ToList();
        Assert.Equal(2, januaryRows.Count);
        Assert.Contains(januaryRows, x => x.PriceType == BudgetFishPriceType.Sales && x.CurrencyCode == "EUR");
        Assert.Contains(januaryRows, x =>
            x.PriceType == BudgetFishPriceType.Purchase &&
            x.CurrencyCode == "TRY" &&
            x.UnitPriceTry == 25m);
    }

    [Fact]
    public async Task AdjustmentDefinitions_SupportCreateUpdateListAndSoftDelete()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.AdjustmentDefinitionService;

        var fishStock = new Stock { ErpStockCode = "FISH-DEF", StockName = "Definition Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-DEF", StockName = "Definition Feed", Unit = "KG", GrupKodu = "YEM" };
        var calibration = new BudgetCalibrationDefinition { CalibrationCode = "K-DEF", CalibrationInfo = "Definition" };
        var temperature = new BudgetWaterTemperature { Year = 2026, Month = 1, WaterTemperatureCelsius = 16m };
        db.AddRange(fishStock, feedStock, calibration, temperature);
        await db.SaveChangesAsync();

        var feedCreated = await service.CreateFeedMortalityRateAsync(new CreateBudgetFeedMortalityRateDto
        {
            WaterTemperatureId = temperature.Id,
            CalibrationDefinitionId = calibration.Id,
            FeedStockId = feedStock.Id,
            ReductionRatePercent = 25m
        });
        Assert.True(feedCreated.Success, feedCreated.Message);

        var feedUpdated = await service.UpdateFeedMortalityRateAsync(feedCreated.Data!.Id, new CreateBudgetFeedMortalityRateDto
        {
            WaterTemperatureId = temperature.Id,
            CalibrationDefinitionId = calibration.Id,
            FeedStockId = feedStock.Id,
            ReductionRatePercent = 30m
        });
        Assert.True(feedUpdated.Success, feedUpdated.Message);
        Assert.Equal(30m, feedUpdated.Data!.ReductionRatePercent);

        var growthCreated = await service.CreateFishGrowthQualityAsync(new CreateBudgetFishGrowthQualityDto
        {
            FishStockId = fishStock.Id,
            GrowthMonthNo = 1,
            QualityPercent = 85m
        });
        Assert.True(growthCreated.Success, growthCreated.Message);

        var feedList = await service.GetFeedMortalityRatesAsync(new PagedRequest());
        var growthList = await service.GetFishGrowthQualitiesAsync(new PagedRequest());
        Assert.Single(feedList.Data!.Items);
        Assert.Single(growthList.Data!.Items);

        Assert.True((await service.DeleteFeedMortalityRateAsync(feedCreated.Data.Id)).Success);
        Assert.True((await service.DeleteFishGrowthQualityAsync(growthCreated.Data!.Id)).Success);
        Assert.False((await service.GetFeedMortalityRateAsync(feedCreated.Data.Id)).Success);
        Assert.False((await service.GetFishGrowthQualityAsync(growthCreated.Data.Id)).Success);
    }

    [Fact]
    public async Task CalculateGrowth_BlocksWhenGrowthProfileIsMissing()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-001", StockName = "Budget Fish", Unit = "AD" };
        db.Stocks.Add(fishStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);

        var result = await service.CalculateGrowthAsync(plan.Id);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("buyume profili yok", result.Message);
        Assert.False(await db.BudgetPlanMonthlyProjections.AnyAsync(x => x.BudgetPlanId == plan.Id));
    }

    [Fact]
    public async Task CalculateGrowth_UsesGeneralSpeciesProfileForNewFishStock()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var generalSeaBassStock = new Stock { ErpStockCode = "L001", StockName = "Levrek", Unit = "AD" };
        var newSeaBassStock = new Stock { ErpStockCode = "FISH-LVRK-10-20", StockName = "Levrek 10-20g", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-SPECIES", StockName = "Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(generalSeaBassStock, newSeaBassStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, newSeaBassStock.Id, BudgetPlanStatus.LiveImported);
        await SeedCompleteBudgetDefinitionsAsync(db, generalSeaBassStock.Id, feedStock.Id);
        await db.SaveChangesAsync();

        var result = await service.CalculateGrowthAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(12, result.Data!.Count);
        Assert.Equal(110m, result.Data[0].ClosingAverageGram);
        Assert.Equal(220m, result.Data[^1].ClosingAverageGram);
    }

    [Fact]
    public async Task CalculateGrowth_VirtualBatchUsesSharedDefinitionsFromItsStartPeriod()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-VIRTUAL", StockName = "Virtual Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-VIRTUAL", StockName = "Virtual Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        var virtualBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        virtualBatch.GrowthStartMonth = 3;
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id, growthStartMonth: 3);
        await db.SaveChangesAsync();

        var result = await service.CalculateGrowthAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data!.Count);
        Assert.DoesNotContain(result.Data, row => row.Year == 2026 && row.Month < 3);
        Assert.Equal(3, result.Data[0].Month);
        Assert.Equal(1, result.Data[0].MonthIndex);
        Assert.Equal(110m, result.Data[0].ClosingAverageGram);
        Assert.Equal(200m, result.Data[^1].ClosingAverageGram);
    }

    [Fact]
    public async Task AddVirtualFishBatch_AllowsMultipleVirtualProjectsInSamePlan()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-MULTI-VIRTUAL", StockName = "Multi Virtual Fish", Unit = "AD" };
        db.Stocks.Add(fishStock);
        await db.SaveChangesAsync();
        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);

        foreach (var projectNo in new[] { 1, 2 })
        {
            var result = await service.AddVirtualFishBatchAsync(plan.Id, new AddVirtualFishBatchDto
            {
                ProjectCode = $"VIRTUAL-{projectNo}",
                ProjectName = $"Virtual Project {projectNo}",
                FishStockId = fishStock.Id,
                BatchCode = $"VIRTUAL-BATCH-{projectNo}",
                InitialLiveCount = 50_000 * projectNo,
                InitialAverageGram = 100m,
                GrowthStartYear = 2026,
                GrowthStartMonth = projectNo
            });

            Assert.True(result.Success, result.Message);
        }

        Assert.Equal(3, await db.BudgetPlanProjects.CountAsync(x => x.BudgetPlanId == plan.Id));
        Assert.Equal(3, await db.BudgetPlanFishBatches.CountAsync(x => x.BudgetPlanId == plan.Id));
        Assert.Contains(await db.BudgetPlanProjects.Where(x => x.BudgetPlanId == plan.Id).ToListAsync(), x => x.ProjectCode == "VIRTUAL-1");
        Assert.Contains(await db.BudgetPlanProjects.Where(x => x.BudgetPlanId == plan.Id).ToListAsync(), x => x.ProjectCode == "VIRTUAL-2");
    }

    [Fact]
    public async Task CalculateGrowth_AppliesNovemberIncreaseToVirtualBatchStartingInJune()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-SCHEDULED", StockName = "Scheduled Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-SCHEDULED", StockName = "Scheduled Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();
        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        plan.StartMonth = 6;
        plan.EndMonth = 12;
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        var batch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        Assert.Equal(BudgetPlanSourceType.Virtual, batch.SourceType);

        var adjustmentResult = await service.CreateFishBatchAdjustmentAsync(plan.Id, new CreateBudgetPlanFishBatchAdjustmentDto
        {
            BudgetPlanFishBatchId = batch.Id,
            AdjustmentType = BudgetPlanFishBatchAdjustmentType.Increase,
            EffectiveYear = 2026,
            EffectiveMonth = 11,
            LiveCount = 50_000,
            Description = "November planned intake"
        });

        Assert.True(adjustmentResult.Success, adjustmentResult.Message);
        var unchangedVirtualBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.Id == batch.Id);
        Assert.Equal(1000, unchangedVirtualBatch.InitialLiveCount);
        Assert.Equal(100m, unchangedVirtualBatch.InitialAverageGram);

        var result = await service.CalculateGrowthAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(7, result.Data!.Count);
        Assert.All(result.Data.Where(x => x.Month < 11), x => Assert.Equal(1000, x.OpeningLiveCount));
        Assert.Equal(51_000, result.Data.Single(x => x.Month == 11).OpeningLiveCount);
        Assert.Equal(51_000, result.Data.Single(x => x.Month == 12).OpeningLiveCount);
    }

    [Fact]
    public async Task CalculateGrowth_ActualBatchContinuesBiologicalProfileAndKpiUsesPreMovementStock()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;

        var fishStock = new Stock { ErpStockCode = "01", StockName = "Levrek", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-01", StockName = "Legacy Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var sourceProject = new Project
        {
            ProjectCode = "20240715001A",
            ProjectName = "Legacy project",
            StartDate = new DateTime(2024, 7, 1),
            Status = DocumentStatus.Posted
        };
        db.Projects.Add(sourceProject);
        await db.SaveChangesAsync();

        var sourceFishBatch = new FishBatch
        {
            ProjectId = sourceProject.Id,
            FishStockId = fishStock.Id,
            BatchCode = "LEGACY-BATCH",
            CurrentAverageGram = 719.339m,
            StartDate = new DateTime(2024, 7, 1)
        };
        db.FishBatches.Add(sourceFishBatch);
        await db.SaveChangesAsync();

        var plan = new BudgetPlan
        {
            BudgetNo = "BUD-LEGACY-137",
            BudgetCode = "LEGACY-137",
            BudgetName = "Legacy comparison",
            StartYear = 2026,
            StartMonth = 3,
            EndYear = 2027,
            EndMonth = 3,
            Status = BudgetPlanStatus.LiveImported
        };
        db.BudgetPlans.Add(plan);
        await db.SaveChangesAsync();

        var project = new BudgetPlanProject
        {
            BudgetPlanId = plan.Id,
            SourceType = BudgetPlanSourceType.Actual,
            ProjectCode = "20240715001A",
            ProjectName = "Legacy project"
        };
        db.BudgetPlanProjects.Add(project);
        await db.SaveChangesAsync();

        db.BudgetPlanFishBatches.Add(new BudgetPlanFishBatch
        {
            BudgetPlanId = plan.Id,
            BudgetPlanProjectId = project.Id,
            SourceType = BudgetPlanSourceType.Actual,
            SourceFishBatchId = sourceFishBatch.Id,
            FishStockId = fishStock.Id,
            BatchCode = "LEGACY-BATCH",
            InitialLiveCount = 427_467,
            InitialAverageGram = 720m,
            InitialBiomassKg = 307_776.24m,
            GrowthStartYear = 2024,
            GrowthStartMonth = 7
        });

        var calibration = new BudgetCalibrationDefinition
        {
            CalibrationCode = "K-ALL",
            CalibrationInfo = "0 - 9999 gr"
        };
        db.BudgetCalibrationDefinitions.Add(calibration);

        var profile = new BudgetFishGrowthProfile
        {
            StockId = fishStock.Id,
            StartMonth = 7,
            Name = "Legacy July profile"
        };
        var legacyGrowth = new Dictionary<int, decimal>
        {
            [21] = 1m,
            [22] = 1m,
            [23] = 26.71739848m,
            [24] = 37.03358581m,
            [25] = 52.49510788m,
            [26] = 40.32874169m,
            [27] = 49.20051241m,
            [28] = 46.86437724m,
            [29] = 41.33052886m,
            [30] = 35.77846115m,
            [31] = 1m,
            [32] = 1m,
            [33] = 1m
        };
        for (var month = 1; month <= 33; month++)
        {
            profile.Lines.Add(new BudgetFishGrowthProfileLine
            {
                GrowthMonthNo = month,
                CalendarMonth = ((6 + month - 1) % 12) + 1,
                MonthlyGrowthGram = legacyGrowth.GetValueOrDefault(month),
                TotalGram = 0m
            });
        }
        db.BudgetFishGrowthProfiles.Add(profile);
        await db.SaveChangesAsync();

        var result = await fixture.Service.CalculateGrowthAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        var projections = result.Data!;
        Assert.Equal(
            new[] { 721m, 722m, 748.71739848m, 785.75098429m, 838.24609217m, 878.57483386m, 927.77534627m, 974.63972351m, 1015.97025237m, 1051.74871352m, 1052.74871352m, 1053.74871352m, 1054.74871352m },
            projections.Select(x => x.ClosingAverageGram).ToArray());
        Assert.Equal(
            new[] { 721m, 722m, 748.72m, 785.75m, 838.25m, 878.57m, 927.78m, 974.64m, 1015.97m, 1051.75m, 1052.75m, 1053.75m, 1054.75m },
            projections.Select(x => decimal.Round(x.ClosingAverageGram, 2, MidpointRounding.AwayFromZero)).ToArray());
        Assert.Equal(21, projections[0].MonthIndex);
        var normalizedBatch = await db.BudgetPlanFishBatches.SingleAsync();
        Assert.Equal(720m, normalizedBatch.InitialAverageGram);
        Assert.Equal(307_776.24m, normalizedBatch.InitialBiomassKg);
        Assert.Equal(2024, normalizedBatch.GrowthStartYear);
        Assert.Equal(7, normalizedBatch.GrowthStartMonth);

        var legacySalesCounts = new[] { 0, 13_851, 6_677, 6_363, 5_964, 5_690, 5_389, 5_131, 4_921, 4_755, 4_748, 4_744, 4_742 };
        for (var index = 0; index < projections.Count; index++)
        {
            var projection = projections[index];
            var temperature = new BudgetWaterTemperature
            {
                Year = projection.Year,
                Month = projection.Month,
                WaterTemperatureCelsius = 18m
            };
            db.BudgetWaterTemperatures.Add(temperature);
            await db.SaveChangesAsync();

            db.BudgetFeedConsumptionRates.Add(new BudgetFeedConsumptionRate
            {
                WaterTemperatureId = temperature.Id,
                CalibrationDefinitionId = calibration.Id,
                FeedStockId = feedStock.Id,
                FeedAmount = 0m
            });
            db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
            {
                BudgetPlanId = plan.Id,
                BudgetPlanFishBatchId = projection.BudgetPlanFishBatchId,
                Year = projection.Year,
                Month = projection.Month,
                MarketType = BudgetMarketType.Domestic,
                SalesTon = legacySalesCounts[index] * projection.ClosingAverageGram / 1_000_000m,
                CurrencyCode = "EUR"
            });
        }
        db.BudgetMortalityRateDefinitions.Add(new BudgetMortalityRateDefinition
        {
            FishStockId = fishStock.Id,
            CalibrationDefinitionId = calibration.Id,
            MortalityRatePercent = 0.25m
        });
        plan.Status = BudgetPlanStatus.SalesPlanned;
        await db.SaveChangesAsync();

        var calculated = await fixture.Service.CalculateAsync(plan.Id);
        Assert.True(calculated.Success, calculated.Message);
        var calculatedRows = calculated.Data!.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();

        Assert.Equal(
            legacySalesCounts,
            calculatedRows.Select(x => x.SalesCount).ToArray());
        var recalculatedSales = await fixture.Service.GetSalesLinesAsync(plan.Id);
        Assert.True(recalculatedSales.Success, recalculatedSales.Message);
        Assert.Equal(legacySalesCounts, recalculatedSales.Data!.Select(x => x.SalesCount ?? 0).ToArray());
        for (var index = 0; index < calculatedRows.Count; index++)
        {
            var row = calculatedRows[index];
            var afterSalesCount = row.OpeningLiveCount - row.SalesCount;
            Assert.Equal(row.OpeningLiveCount, row.StockCount);
            Assert.Equal(
                Math.Round(row.OpeningLiveCount * (row.OpeningAverageGram + row.MonthlyGrowthGram) / 1000m, 3, MidpointRounding.AwayFromZero),
                row.StockKg);
            Assert.Equal(
                Math.Min(afterSalesCount, (int)Math.Round(afterSalesCount * 0.25m / 100m, MidpointRounding.AwayFromZero)),
                row.MortalityCount);
            Assert.Equal(row.OpeningLiveCount - row.SalesCount - row.MortalityCount, row.ClosingLiveCount);
            if (index > 0)
            {
                Assert.Equal(calculatedRows[index - 1].ClosingLiveCount, row.OpeningLiveCount);
                Assert.Equal(calculatedRows[index - 1].ClosingAverageGram, row.OpeningAverageGram);
            }
        }

        var report = await fixture.KpiService.GetReportAsync(plan.Id);
        Assert.True(report.Success, report.Message);
        var reportRows = report.Data!.MonthlyRows.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
        Assert.Equal(reportRows.Select(x => x.StockCount), calculatedRows.Select(x => x.StockCount));
        Assert.Equal(reportRows.Select(x => x.StockKg), calculatedRows.Select(x => x.StockKg));
        Assert.Equal(reportRows.Select(x => x.StockTon), calculatedRows.Select(x => x.StockTon));
        var reportMarch = reportRows[0];
        Assert.Equal(427_467, reportMarch.StockCount);
        Assert.Equal(721m, reportMarch.UnitGram);
        Assert.Equal(308.204m, reportMarch.StockTon);
    }

    [Fact]
    public async Task Calculate_AppliesGrowthQualityAndFeedMortalityReduction()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-ADJ", StockName = "Adjusted Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-ADJ", StockName = "Adjusted Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        await db.SaveChangesAsync();

        var calibration = await db.BudgetCalibrationDefinitions.SingleAsync();
        var januaryTemperature = await db.BudgetWaterTemperatures.SingleAsync(x => x.Year == 2026 && x.Month == 1);
        var mortality = await db.BudgetMortalityRateDefinitions.SingleAsync();
        mortality.MortalityRatePercent = 10m;

        db.BudgetFishGrowthQualities.Add(new BudgetFishGrowthQuality
        {
            FishStockId = fishStock.Id,
            GrowthMonthNo = 1,
            QualityPercent = 50m
        });
        db.BudgetFeedMortalityRates.Add(new BudgetFeedMortalityRate
        {
            WaterTemperatureId = januaryTemperature.Id,
            CalibrationDefinitionId = calibration.Id,
            FeedStockId = feedStock.Id,
            ReductionRatePercent = 50m
        });
        await db.SaveChangesAsync();

        var growth = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growth.Success, growth.Message);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2026,
            Month = 1,
            SalesTon = 0m
        });
        plan.Status = BudgetPlanStatus.SalesPlanned;
        await db.SaveChangesAsync();

        var result = await service.CalculateAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        var january = Assert.Single(result.Data!, x => x.Year == 2026 && x.Month == 1);
        Assert.Equal(10m, january.RawMonthlyGrowthGram);
        Assert.Equal(50m, january.GrowthQualityPercent);
        Assert.Equal(5m, january.MonthlyGrowthGram);
        Assert.Equal(50m, january.FeedMortalityReductionPercent);
        Assert.Equal(1.546m, january.FeedMortalityReductionKg);
        Assert.Equal(29.377m, january.FeedKg);

        var feedingLine = await db.BudgetPlanFeedingLines.SingleAsync(x =>
            x.BudgetPlanId == plan.Id && x.Year == 2026 && x.Month == 1);
        Assert.Equal(january.FeedMortalityReductionKg, feedingLine.MortalityReductionKg);
        Assert.Equal(january.FeedMortalityReductionPercent, feedingLine.MortalityReductionPercent);
    }

    [Fact]
    public async Task Calculate_FiveYearBudgetWithFullDefinitions_ProducesConsistentReports()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;
        var kpiService = fixture.KpiService;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-002", StockName = "Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-001", StockName = "Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        plan.EndYear = 2030;
        plan.EndMonth = 12;
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);

        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        await db.SaveChangesAsync();

        var growth = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growth.Success, growth.Message);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2030,
            Month = 12,
            SalesTon = 0.05m
        });
        plan.Status = BudgetPlanStatus.SalesPlanned;
        await db.SaveChangesAsync();

        var result = await service.CalculateAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(60, result.Data!.Count);
        Assert.Equal(60, await db.BudgetPlanFeedingLines.CountAsync(x => x.BudgetPlanId == plan.Id));
        Assert.Equal(60, await db.BudgetPlanMortalityLines.CountAsync(x => x.BudgetPlanId == plan.Id));

        var lastProjection = result.Data
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .First();
        Assert.Equal(2030, lastProjection.Year);
        Assert.Equal(12, lastProjection.Month);
        Assert.Equal(60, lastProjection.MonthIndex);
        Assert.Equal(0.05m, lastProjection.SalesTon);
        Assert.Equal(
            lastProjection.OpeningLiveCount - lastProjection.SalesCount - lastProjection.MortalityCount,
            lastProjection.ClosingLiveCount);
        Assert.Equal(
            Round(lastProjection.ClosingLiveCount * lastProjection.ClosingAverageGram / 1000m),
            lastProjection.ClosingBiomassKg);
        Assert.True(lastProjection.FeedKg > 0m);

        var planningKpi = await service.GetKpiSummaryAsync(plan.Id);
        var report = await kpiService.GetReportAsync(plan.Id);

        Assert.True(planningKpi.Success, planningKpi.Message);
        Assert.True(report.Success, report.Message);
        Assert.NotNull(planningKpi.Data);
        Assert.NotNull(report.Data);
        Assert.Equal(60, report.Data!.MonthlyRows.Count);
        Assert.Equal(Round(result.Data.Sum(x => x.FeedKg)), report.Data.Summary.FeedKg);
        Assert.Equal(Round(result.Data.Sum(x => x.SalesTon)), report.Data.Summary.SalesTon);
        Assert.Equal(Round(result.Data.Sum(x => x.MortalityKg)), report.Data.Summary.MortalityKg);
        Assert.Equal(planningKpi.Data!.Fcr, report.Data.Summary.Fcr);
        Assert.Equal(planningKpi.Data.FinalBiomassKg, report.Data.Summary.FinalBiomassKg);
        Assert.True(report.Data.Summary.ProducedBiomassKg > 0m);
        Assert.True(report.Data.Summary.Fcr > 0m);
    }

    [Fact]
    public async Task Calculate_BlocksWhenFeedRateDefinitionIsMissing()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-003", StockName = "Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-003", StockName = "Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);

        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id, includeFeedRates: false);
        await db.SaveChangesAsync();

        var growth = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growth.Success, growth.Message);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2026,
            Month = 12,
            SalesTon = 0.01m
        });
        plan.Status = BudgetPlanStatus.SalesPlanned;
        await db.SaveChangesAsync();

        var result = await service.CalculateAsync(plan.Id);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("yem tuketim orani tanimi yok", result.Message);
        Assert.Equal(12, await db.BudgetPlanMonthlyProjections.CountAsync(x => x.BudgetPlanId == plan.Id));
        Assert.False(await db.BudgetPlanFeedingLines.AnyAsync(x => x.BudgetPlanId == plan.Id));
        Assert.False(await db.BudgetPlanMortalityLines.AnyAsync(x => x.BudgetPlanId == plan.Id));
    }

    [Fact]
    public async Task UpsertSalesTon_ConvertsTonToKgAndCalculatesSalesCount()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-TON", StockName = "Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-TON", StockName = "Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        await db.SaveChangesAsync();

        var growthResult = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growthResult.Success, growthResult.Message);
        var savedGrowthIncrements = growthResult.Data!
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .Select(x => x.MonthlyGrowthGram)
            .ToArray();

        var projection = await db.BudgetPlanMonthlyProjections
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .FirstAsync(x => x.BudgetPlanId == plan.Id);

        var result = await service.UpsertSalesTonAsync(plan.Id, new UpsertBudgetPlanSalesTonDto
        {
            BudgetPlanFishBatchId = projection.BudgetPlanFishBatchId,
            Year = projection.Year,
            Month = projection.Month,
            SalesTon = 0.05m
        });

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(0.05m, result.Data!.SalesTon);
        Assert.Equal(455, result.Data.SalesCount);

        var saved = await db.BudgetPlanSalesLines.SingleAsync(x => x.BudgetPlanId == plan.Id);
        Assert.Equal(0.05m, saved.SalesTon);
        Assert.Equal(455, saved.SalesCount);

        saved.SalesCount = 999;
        var growthLines = await db.Set<BudgetFishGrowthProfileLine>().ToListAsync();
        foreach (var growthLine in growthLines)
        {
            growthLine.MonthlyGrowthGram += 1_000m;
        }
        var mortalityRate = await db.BudgetMortalityRateDefinitions.SingleAsync();
        mortalityRate.MortalityRatePercent = 10m;
        await db.SaveChangesAsync();

        var calculation = await service.CalculateAsync(plan.Id);

        Assert.True(calculation.Success, calculation.Message);
        var finalRows = calculation.Data!.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
        Assert.Equal(savedGrowthIncrements, finalRows.Select(x => x.MonthlyGrowthGram).ToArray());
        var recalculatedProjection = Assert.Single(calculation.Data!, x => x.Year == projection.Year && x.Month == projection.Month);
        Assert.Equal(projection.Id, recalculatedProjection.Id);
        Assert.Equal(455, recalculatedProjection.SalesCount);
        var grownAverageGram = recalculatedProjection.OpeningAverageGram + recalculatedProjection.MonthlyGrowthGram;
        var expectedAfterSalesAverageGram = decimal.Round(
            Round(Round(recalculatedProjection.OpeningLiveCount * grownAverageGram / 1000m) - 50m) * 1000m /
            (recalculatedProjection.OpeningLiveCount - recalculatedProjection.SalesCount),
            8,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedAfterSalesAverageGram, recalculatedProjection.ClosingAverageGram);
        var afterSalesCount = recalculatedProjection.OpeningLiveCount - recalculatedProjection.SalesCount;
        Assert.Equal(
            (int)Math.Round(afterSalesCount * 10m / 100m, MidpointRounding.AwayFromZero),
            recalculatedProjection.MortalityCount);
        var afterSalesBiomassKg = Round(
            Round(recalculatedProjection.OpeningLiveCount * grownAverageGram / 1000m) - 50m);
        var expectedFeedKg = Round(
            ((afterSalesBiomassKg + recalculatedProjection.ClosingBiomassKg) / 2m) * 0.01m *
            DateTime.DaysInMonth(recalculatedProjection.Year, recalculatedProjection.Month));
        Assert.Equal(expectedFeedKg, recalculatedProjection.FeedKg);
        Assert.Equal(recalculatedProjection.ClosingLiveCount, finalRows[1].OpeningLiveCount);
        Assert.Equal(recalculatedProjection.ClosingAverageGram, finalRows[1].OpeningAverageGram);
        Assert.Equal(
            Round(finalRows[1].OpeningLiveCount * finalRows[1].OpeningAverageGram / 1000m),
            finalRows[1].OpeningBiomassKg);
        Assert.Equal(455, await db.BudgetPlanSalesLines
            .Where(x => x.BudgetPlanId == plan.Id)
            .Select(x => x.SalesCount)
            .SingleAsync());
    }

    [Fact]
    public async Task Calculate_DistributesDomesticAndForeignSalesByCalibrationAndPrice()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-MARKET", StockName = "Levrek", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-MARKET", StockName = "Yem", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        await db.SaveChangesAsync();

        var growth = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growth.Success, growth.Message);
        var january = growth.Data!.Single(x => x.Year == 2026 && x.Month == 1);
        var calibration = await db.BudgetCalibrationDefinitions.SingleAsync();

        db.BudgetPlanFishPrices.AddRange(
            new BudgetPlanFishPrice
            {
                BudgetPlanId = plan.Id,
                FishStockId = fishStock.Id,
                CalibrationDefinitionId = calibration.Id,
                Year = 2026,
                Month = 1,
                PriceType = BudgetFishPriceType.Sales,
                MarketType = BudgetMarketType.Domestic,
                CurrencyCode = "EUR",
                UnitPrice = 6m
            },
            new BudgetPlanFishPrice
            {
                BudgetPlanId = plan.Id,
                FishStockId = fishStock.Id,
                CalibrationDefinitionId = calibration.Id,
                Year = 2026,
                Month = 1,
                PriceType = BudgetFishPriceType.Sales,
                MarketType = BudgetMarketType.Foreign,
                CurrencyCode = "EUR",
                UnitPrice = 7m
            });
        db.BudgetPlanExchangeRates.Add(new BudgetPlanExchangeRate
        {
            BudgetPlanId = plan.Id,
            Year = 2026,
            Month = 1,
            CurrencyCode = "EUR",
            RateType = "Budget",
            ExchangeRate = 50m,
            SourceType = "Manual",
            IsManualOverride = true
        });
        await db.SaveChangesAsync();

        var domestic = await service.UpsertSalesTonAsync(plan.Id, new UpsertBudgetPlanSalesTonDto
        {
            BudgetPlanFishBatchId = january.BudgetPlanFishBatchId,
            Year = 2026,
            Month = 1,
            MarketType = BudgetMarketType.Domestic,
            SalesTon = 0.03m
        });
        var foreign = await service.UpsertSalesTonAsync(plan.Id, new UpsertBudgetPlanSalesTonDto
        {
            BudgetPlanFishBatchId = january.BudgetPlanFishBatchId,
            Year = 2026,
            Month = 1,
            MarketType = BudgetMarketType.Foreign,
            SalesTon = 0.02m
        });

        Assert.True(domestic.Success, domestic.Message);
        Assert.True(foreign.Success, foreign.Message);
        Assert.Equal(6m, domestic.Data!.UnitPriceEuro);
        Assert.Equal(7m, foreign.Data!.UnitPriceEuro);

        var calculation = await service.CalculateAsync(plan.Id);

        Assert.True(calculation.Success, calculation.Message);
        var projection = calculation.Data!.Single(x => x.Year == 2026 && x.Month == 1);
        Assert.Equal(0.05m, projection.SalesTon);
        Assert.Equal(455, projection.SalesCount);

        var distributions = await service.GetSalesDistributionsAsync(plan.Id);
        Assert.True(distributions.Success, distributions.Message);
        Assert.Equal(2, distributions.Data!.Count);
        Assert.Equal(455, distributions.Data.Sum(x => x.SalesCount));
        Assert.All(distributions.Data, x => Assert.Equal("K-ALL", x.CalibrationCode));
        Assert.Equal(180m, distributions.Data.Single(x => x.MarketType == BudgetMarketType.Domestic).AmountEuro);
        Assert.Equal(140m, distributions.Data.Single(x => x.MarketType == BudgetMarketType.Foreign).AmountEuro);
        Assert.Equal(16000m, distributions.Data.Sum(x => x.AmountTry));

        var report = await fixture.KpiService.GetReportAsync(plan.Id);
        Assert.True(report.Success, report.Message);
        var reportRow = report.Data!.MonthlyRows.Single(x => x.Year == 2026 && x.Month == 1);
        Assert.Equal(0.03m, reportRow.DomesticSalesTon);
        Assert.Equal(0.02m, reportRow.ForeignSalesTon);
        Assert.Equal(320m, reportRow.AmountEuro);
        Assert.Equal(16000m, reportRow.AmountTry);
    }

    [Fact]
    public async Task ImportSalesTons_RollsBackAllRowsWhenOneRowIsInvalid()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-IMPORT", StockName = "Budget Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-IMPORT", StockName = "Budget Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.LiveImported);
        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
        await db.SaveChangesAsync();
        var growth = await service.CalculateGrowthAsync(plan.Id);
        Assert.True(growth.Success, growth.Message);

        var projection = await db.BudgetPlanMonthlyProjections
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .FirstAsync(x => x.BudgetPlanId == plan.Id);
        var result = await service.ImportSalesTonsAsync(plan.Id, new ImportBudgetPlanSalesTonsDto
        {
            Lines =
            {
                new UpsertBudgetPlanSalesTonDto
                {
                    BudgetPlanFishBatchId = projection.BudgetPlanFishBatchId,
                    Year = projection.Year,
                    Month = projection.Month,
                    SalesTon = 0.05m
                },
                new UpsertBudgetPlanSalesTonDto
                {
                    BudgetPlanFishBatchId = projection.BudgetPlanFishBatchId,
                    Year = 2099,
                    Month = 1,
                    SalesTon = 0.01m
                }
            }
        });

        Assert.False(result.Success);
        Assert.Contains("Excel 3. satır", result.Message);
        db.ChangeTracker.Clear();
        Assert.False(await db.BudgetPlanSalesLines.AnyAsync(x => x.BudgetPlanId == plan.Id));
    }

    [Fact]
    public async Task KpiReport_UsesDevirFcrProducedBiomassFormula()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-ZERO", StockName = "Budget Fish", Unit = "AD" };
        db.Stocks.Add(fishStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.Calculated);
        var batch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        db.BudgetPlanMonthlyProjections.Add(new BudgetPlanMonthlyProjection
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = batch.Id,
            Year = 2026,
            Month = 1,
            MonthIndex = 1,
            OpeningLiveCount = 1000,
            OpeningAverageGram = 100m,
            OpeningBiomassKg = 100m,
            MonthlyGrowthGram = 0m,
            ClosingAverageGram = 100m,
            FeedKg = 25m,
            ClosingLiveCount = 1000,
            ClosingBiomassKg = 100m
        });
        await db.SaveChangesAsync();

        var result = await fixture.KpiService.GetReportAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(100m, result.Data!.Summary.ProducedBiomassKg);
        Assert.Equal(0.25m, result.Data.Summary.Fcr);
        Assert.Equal(100m, result.Data.MonthlyRows.Single().ProducedBiomassKg);
        Assert.Equal(0.25m, result.Data.MonthlyRows.Single().Fcr);
    }

    [Fact]
    public async Task KpiReport_ReturnsProjectSalesStockAndPricingDetails()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;

        var fishStock = new Stock { ErpStockCode = "FISH-KPI-DETAIL", StockName = "Levrek", Unit = "AD" };
        db.Stocks.Add(fishStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.Calculated);
        var batch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        var project = await db.BudgetPlanProjects.SingleAsync(x => x.BudgetPlanId == plan.Id);

        db.BudgetPlanMonthlyProjections.Add(new BudgetPlanMonthlyProjection
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = batch.Id,
            Year = 2026,
            Month = 3,
            MonthIndex = 1,
            OpeningLiveCount = 427467,
            OpeningAverageGram = 721m,
            OpeningBiomassKg = 308203.707m,
            ClosingAverageGram = 721m,
            SalesTon = 5m,
            SalesCount = 6935,
            MortalityCount = 1069,
            MortalityKg = 770.749m,
            FeedKg = 12500m,
            ClosingLiveCount = 419463,
            ClosingBiomassKg = 302432.823m
        });
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = batch.Id,
            Year = 2026,
            Month = 3,
            SalesTon = 5m,
            SalesCount = 6935,
            UnitPrice = 6m
        });
        db.BudgetPlanExchangeRates.Add(new BudgetPlanExchangeRate
        {
            BudgetPlanId = plan.Id,
            Year = 2026,
            Month = 3,
            CurrencyCode = "EUR",
            RateType = "Budget",
            ExchangeRate = 50m,
            SourceType = "Manual",
            IsManualOverride = true
        });
        await db.SaveChangesAsync();

        var result = await fixture.KpiService.GetReportAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        var row = Assert.Single(result.Data!.MonthlyRows);
        Assert.Equal(project.ProjectCode, row.ProjectCode);
        Assert.Equal(project.ProjectName, row.ProjectName);
        Assert.Equal(6935, row.SalesCount);
        Assert.Equal(5m, row.SalesTon);
        Assert.Equal(5000m, row.SalesKg);
        Assert.Equal(427467, row.StockCount);
        Assert.Equal(308.204m, row.StockTon);
        Assert.Equal(308203.707m, row.StockKg);
        Assert.Equal(1069, row.MortalityCount);
        Assert.Equal(770.749m, row.MortalityKg);
        Assert.Equal(12500m, row.FeedKg);
        Assert.Equal(721m, row.UnitGram);
        Assert.Equal(6m, row.AveragePriceEuro);
        Assert.Equal(30000m, row.AmountEuro);
        Assert.Equal(50m, row.ExchangeRate);
        Assert.Equal(300m, row.AveragePriceTry);
        Assert.Equal(1500000m, row.AmountTry);
    }

    private static async Task<BudgetPlan> SeedBudgetPlanAsync(AquaDbContext db, long fishStockId, BudgetPlanStatus status)
    {
        var plan = new BudgetPlan
        {
            BudgetNo = $"BUD-TEST-{Guid.NewGuid():N}"[..18],
            BudgetCode = $"BUD-TEST-{Guid.NewGuid():N}"[..18],
            BudgetName = "Three Year Budget Test",
            StartYear = 2026,
            StartMonth = 1,
            EndYear = 2026,
            EndMonth = 12,
            Status = status
        };
        db.BudgetPlans.Add(plan);
        await db.SaveChangesAsync();

        var project = new BudgetPlanProject
        {
            BudgetPlanId = plan.Id,
            SourceType = BudgetPlanSourceType.Virtual,
            ProjectCode = "BUD-PRJ",
            ProjectName = "Budget Project"
        };
        db.BudgetPlanProjects.Add(project);
        await db.SaveChangesAsync();

        var batch = new BudgetPlanFishBatch
        {
            BudgetPlanId = plan.Id,
            BudgetPlanProjectId = project.Id,
            SourceType = BudgetPlanSourceType.Virtual,
            FishStockId = fishStockId,
            BatchCode = "BUD-BATCH",
            InitialLiveCount = 1000,
            InitialAverageGram = 100m,
            InitialBiomassKg = 100m,
            GrowthStartYear = 2026,
            GrowthStartMonth = 1
        };
        db.BudgetPlanFishBatches.Add(batch);
        await db.SaveChangesAsync();

        return plan;
    }

    private static async Task SeedCompleteBudgetDefinitionsAsync(
        AquaDbContext db,
        long fishStockId,
        long feedStockId,
        bool includeFeedRates = true,
        int growthStartMonth = 1)
    {
        var calibration = new BudgetCalibrationDefinition
        {
            CalibrationCode = "K-ALL",
            CalibrationInfo = "0 - 9999 gr"
        };
        db.BudgetCalibrationDefinitions.Add(calibration);
        await db.SaveChangesAsync();

        var profile = new BudgetFishGrowthProfile
        {
            StockId = fishStockId,
            StartMonth = growthStartMonth,
            Name = "60 Month Growth"
        };
        for (var month = 1; month <= 60; month++)
        {
            profile.Lines.Add(new BudgetFishGrowthProfileLine
            {
                GrowthMonthNo = month,
                MonthlyGrowthGram = 10m
            });
        }

        db.BudgetFishGrowthProfiles.Add(profile);

        for (var month = 1; month <= 12; month++)
        {
            var temperature = new BudgetWaterTemperature
            {
                Year = 2026,
                Month = month,
                WaterTemperatureCelsius = 14m + month
            };
            db.BudgetWaterTemperatures.Add(temperature);
            await db.SaveChangesAsync();

            if (includeFeedRates)
            {
                db.BudgetFeedConsumptionRates.Add(new BudgetFeedConsumptionRate
                {
                    WaterTemperatureId = temperature.Id,
                    CalibrationDefinitionId = calibration.Id,
                    FeedStockId = feedStockId,
                    FeedAmount = 1m
                });
            }
        }

        db.BudgetMortalityRateDefinitions.Add(new BudgetMortalityRateDefinition
        {
            MortalityRatePercent = 0m
        });
    }

    private static async Task<BudgetFixture> CreateFixtureAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new BudgetSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        return new BudgetFixture(
            connection,
            db,
            new BudgetPlanningService(unitOfWork),
            new BudgetAdjustmentRateDefinitionService(unitOfWork),
            new BudgetKpiService(db));
    }

    private sealed class BudgetFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public BudgetFixture(
            SqliteConnection connection,
            AquaDbContext db,
            BudgetPlanningService service,
            BudgetAdjustmentRateDefinitionService adjustmentDefinitionService,
            BudgetKpiService kpiService)
        {
            _connection = connection;
            Db = db;
            Service = service;
            AdjustmentDefinitionService = adjustmentDefinitionService;
            KpiService = kpiService;
        }

        public AquaDbContext Db { get; }
        public BudgetPlanningService Service { get; }
        public BudgetAdjustmentRateDefinitionService AdjustmentDefinitionService { get; }
        public BudgetKpiService KpiService { get; }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class BudgetSqliteAquaDbContext : AquaDbContext
    {
        public BudgetSqliteAquaDbContext(DbContextOptions<AquaDbContext> options)
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
        }
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }
}
