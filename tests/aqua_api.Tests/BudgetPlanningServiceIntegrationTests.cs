using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Modules.Budget.Application.Dtos;
using aqua_api.Modules.Budget.Application.Services;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Application.Dtos;
using aqua_api.Modules.BudgetPlanning.Application.Services;
using aqua_api.Modules.BudgetKpi.Application.Services;
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
    public async Task Calculate_AppliesGrowthQualityAndFeedMortalityReduction()
    {
        await using var fixture = await CreateFixtureAsync();
        var db = fixture.Db;
        var service = fixture.Service;

        var fishStock = new Stock { ErpStockCode = "FISH-BUD-ADJ", StockName = "Adjusted Fish", Unit = "AD" };
        var feedStock = new Stock { ErpStockCode = "FEED-BUD-ADJ", StockName = "Adjusted Feed", Unit = "KG", GrupKodu = "YEM" };
        db.Stocks.AddRange(fishStock, feedStock);
        await db.SaveChangesAsync();

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.SalesPlanned);
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2026,
            Month = 1,
            SalesTon = 0m
        });
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

        var result = await service.CalculateAsync(plan.Id);

        Assert.True(result.Success, result.Message);
        var january = Assert.Single(result.Data!, x => x.Year == 2026 && x.Month == 1);
        Assert.Equal(10m, january.RawMonthlyGrowthGram);
        Assert.Equal(50m, january.GrowthQualityPercent);
        Assert.Equal(5m, january.MonthlyGrowthGram);
        Assert.Equal(50m, january.FeedMortalityReductionPercent);
        Assert.Equal(1.507m, january.FeedMortalityReductionKg);
        Assert.Equal(28.641m, january.FeedKg);

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

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.SalesPlanned);
        plan.EndYear = 2030;
        plan.EndMonth = 12;
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2030,
            Month = 12,
            SalesTon = 0.05m
        });

        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id);
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
        Assert.Equal(700m, lastProjection.ClosingAverageGram);
        Assert.Equal(929, lastProjection.ClosingLiveCount);
        Assert.Equal(650.3m, lastProjection.ClosingBiomassKg);
        Assert.Equal(207.747m, lastProjection.FeedKg);
        Assert.Equal(0.05m, lastProjection.SalesTon);
        Assert.Equal(7300.597m, Round(result.Data.Sum(x => x.FeedKg)));

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
        Assert.Equal(700.3m, report.Data.Summary.ProducedBiomassKg);
        Assert.Equal(10.425m, report.Data.Summary.Fcr);
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

        var plan = await SeedBudgetPlanAsync(db, fishStock.Id, BudgetPlanStatus.SalesPlanned);
        var budgetBatch = await db.BudgetPlanFishBatches.SingleAsync(x => x.BudgetPlanId == plan.Id);
        db.BudgetPlanSalesLines.Add(new BudgetPlanSalesLine
        {
            BudgetPlanId = plan.Id,
            BudgetPlanFishBatchId = budgetBatch.Id,
            Year = 2026,
            Month = 12,
            SalesTon = 0.01m
        });

        await SeedCompleteBudgetDefinitionsAsync(db, fishStock.Id, feedStock.Id, includeFeedRates: false);
        await db.SaveChangesAsync();

        var result = await service.CalculateAsync(plan.Id);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("yem tuketim orani tanimi yok", result.Message);
        Assert.False(await db.BudgetPlanMonthlyProjections.AnyAsync(x => x.BudgetPlanId == plan.Id));
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

    private static async Task SeedCompleteBudgetDefinitionsAsync(AquaDbContext db, long fishStockId, long feedStockId, bool includeFeedRates = true)
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
            StartMonth = 1,
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
