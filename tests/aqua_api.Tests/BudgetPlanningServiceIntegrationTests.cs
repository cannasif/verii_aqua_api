using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Application.Dtos;
using aqua_api.Modules.BudgetPlanning.Application.Services;
using aqua_api.Modules.BudgetKpi.Application.Services;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace aqua_api.Tests;

public class BudgetPlanningServiceIntegrationTests
{
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
            SalesKg = 50m
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
        Assert.Equal(50m, lastProjection.SalesKg);
        Assert.Equal(7300.597m, Round(result.Data.Sum(x => x.FeedKg)));

        var planningKpi = await service.GetKpiSummaryAsync(plan.Id);
        var report = await kpiService.GetReportAsync(plan.Id);

        Assert.True(planningKpi.Success, planningKpi.Message);
        Assert.True(report.Success, report.Message);
        Assert.NotNull(planningKpi.Data);
        Assert.NotNull(report.Data);
        Assert.Equal(60, report.Data!.MonthlyRows.Count);
        Assert.Equal(Round(result.Data.Sum(x => x.FeedKg)), report.Data.Summary.FeedKg);
        Assert.Equal(Round(result.Data.Sum(x => x.SalesKg)), report.Data.Summary.SalesKg);
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
            SalesKg = 10m
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
        Assert.Equal(50m, result.Data!.SalesKg);
        Assert.Equal(455, result.Data.SalesCount);

        var saved = await db.BudgetPlanSalesLines.SingleAsync(x => x.BudgetPlanId == plan.Id);
        Assert.Equal(50m, saved.SalesKg);
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
        return new BudgetFixture(connection, db, new BudgetPlanningService(unitOfWork), new BudgetKpiService(db));
    }

    private sealed class BudgetFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public BudgetFixture(SqliteConnection connection, AquaDbContext db, BudgetPlanningService service, BudgetKpiService kpiService)
        {
            _connection = connection;
            Db = db;
            Service = service;
            KpiService = kpiService;
        }

        public AquaDbContext Db { get; }
        public BudgetPlanningService Service { get; }
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
