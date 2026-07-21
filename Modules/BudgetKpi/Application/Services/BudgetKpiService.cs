using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.BudgetKpi.Application.Services;

public class BudgetKpiService : IBudgetKpiService
{
    private readonly AquaDbContext _db;

    public BudgetKpiService(AquaDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<BudgetKpiReportDto>> GetReportAsync(long budgetPlanId)
    {
        var plan = await _db.BudgetPlans
            .AsNoTracking()
            .Include(x => x.FishBatches)
            .FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);

        if (plan == null)
        {
            return ApiResponse<BudgetKpiReportDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status != BudgetPlanStatus.Calculated)
        {
            return ApiResponse<BudgetKpiReportDto>.ErrorResult("KPI icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", "KPI icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", StatusCodes.Status400BadRequest);
        }

        var rows = await _db.BudgetPlanMonthlyProjections
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch)
                .ThenInclude(x => x.BudgetPlanProject)
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        var salesLines = await _db.BudgetPlanSalesLines
            .AsNoTracking()
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();
        var salesLineLookup = salesLines
            .GroupBy(x => (x.BudgetPlanFishBatchId, x.Year, x.Month))
            .ToDictionary(x => x.Key, x => x.OrderByDescending(row => row.Id).First());
        var salesDistributions = await _db.BudgetPlanSalesDistributions
            .AsNoTracking()
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        var exchangeRates = await _db.BudgetPlanExchangeRates
            .AsNoTracking()
            .Where(x => x.BudgetPlanId == budgetPlanId && x.CurrencyCode == "EUR" && !x.IsDeleted)
            .ToListAsync();
        var exchangeRateLookup = exchangeRates
            .GroupBy(x => (x.Year, x.Month))
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(row => row.IsManualOverride).ThenByDescending(row => row.Id).First().ExchangeRate);

        var initial = plan.FishBatches.Where(x => !x.IsDeleted).Sum(x => x.InitialBiomassKg);
        var final = rows
            .GroupBy(x => x.BudgetPlanFishBatchId)
            .Select(x => x.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).FirstOrDefault()?.ClosingBiomassKg ?? 0m)
            .Sum();
        var sales = rows.Sum(x => x.SalesTon * 1000m);
        var feed = rows.Sum(x => x.FeedKg);
        var mortality = rows.Sum(x => x.MortalityKg);
        var mortalityCount = rows.Sum(x => x.MortalityCount);
        var salesCount = rows.Sum(x => x.SalesCount);
        var finalLiveCount = rows
            .GroupBy(x => x.BudgetPlanFishBatchId)
            .Select(x => x.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).FirstOrDefault()?.ClosingLiveCount ?? 0)
            .Sum();
        var produced = Math.Max(0m, final + sales + mortality);
        var initialLiveCount = plan.FishBatches.Where(x => !x.IsDeleted).Sum(x => x.InitialLiveCount);

        var monthlyRows = rows
            .GroupBy(x => new
            {
                x.Year,
                x.Month,
                x.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
                x.BudgetPlanFishBatch.BudgetPlanProject.ProjectName
            })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .ThenBy(x => x.Key.ProjectCode)
            .Select(group =>
            {
                var opening = group.Sum(x => x.OpeningBiomassKg);
                var closing = group.Sum(x => x.ClosingBiomassKg);
                var groupSales = group.Sum(x => x.SalesTon * 1000m);
                var groupSalesCount = group.Sum(x => x.SalesCount);
                var groupStockCount = group.Sum(x => x.OpeningLiveCount);
                var groupStockKg = group.Sum(x => x.OpeningLiveCount * x.ClosingAverageGram / 1000m);
                var groupFeed = group.Sum(x => x.FeedKg);
                var groupMortality = group.Sum(x => x.MortalityKg);
                var producedBiomassKg = Math.Max(0m, closing + groupSales + groupMortality);
                var batchIds = group.Select(x => x.BudgetPlanFishBatchId).ToHashSet();
                var groupDistributions = salesDistributions
                    .Where(x => x.Year == group.Key.Year && x.Month == group.Key.Month && batchIds.Contains(x.BudgetPlanFishBatchId))
                    .ToList();
                var amountEuro = groupDistributions.Count > 0
                    ? groupDistributions.Sum(x => x.AmountEuro)
                    : group.Sum(projection =>
                        salesLineLookup.TryGetValue((projection.BudgetPlanFishBatchId, projection.Year, projection.Month), out var salesLine)
                            ? projection.SalesTon * 1000m * (salesLine.UnitPrice ?? 0m)
                            : 0m);
                var averagePriceEuro = groupSales <= 0m ? 0m : amountEuro / groupSales;
                var exchangeRate = exchangeRateLookup.GetValueOrDefault((group.Key.Year, group.Key.Month));
                var hasExchangeRate = exchangeRate > 0m;
                var hasDistributionAmountTry = groupDistributions.Count > 0 && groupDistributions.All(x => x.AmountTry.HasValue);
                var amountTry = hasDistributionAmountTry
                    ? groupDistributions.Sum(x => x.AmountTry!.Value)
                    : hasExchangeRate ? amountEuro * exchangeRate : (decimal?)null;
                return new BudgetKpiMonthlyDto
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    ProjectCode = group.Key.ProjectCode,
                    ProjectName = group.Key.ProjectName,
                    OpeningBiomassKg = Round(opening),
                    ClosingBiomassKg = Round(closing),
                    GrowthBiomassKg = Round(producedBiomassKg),
                    ProducedBiomassKg = Round(producedBiomassKg),
                    SalesCount = groupSalesCount,
                    SalesTon = Round(groupSales / 1000m),
                    DomesticSalesTon = Round(groupDistributions.Count > 0
                        ? groupDistributions.Where(x => x.MarketType == BudgetMarketType.Domestic).Sum(x => x.SalesTon)
                        : groupSales / 1000m),
                    ForeignSalesTon = Round(groupDistributions.Count > 0
                        ? groupDistributions.Where(x => x.MarketType == BudgetMarketType.Foreign).Sum(x => x.SalesTon)
                        : 0m),
                    SalesKg = Round(groupSales),
                    StockCount = groupStockCount,
                    StockTon = Round(groupStockKg / 1000m),
                    StockKg = Round(groupStockKg),
                    FeedKg = Round(groupFeed),
                    MortalityKg = Round(groupMortality),
                    MortalityCount = group.Sum(x => x.MortalityCount),
                    UnitGram = groupStockCount <= 0 ? 0m : Round(groupStockKg * 1000m / groupStockCount),
                    AveragePriceEuro = Round(averagePriceEuro),
                    AmountEuro = Round(amountEuro),
                    ExchangeRate = hasExchangeRate ? Round(exchangeRate) : null,
                    AveragePriceTry = hasExchangeRate ? Round(averagePriceEuro * exchangeRate) : null,
                    AmountTry = amountTry.HasValue ? Round(amountTry.Value) : null,
                    Fcr = producedBiomassKg <= 0 ? 0m : Round(groupFeed / producedBiomassKg)
                };
            })
            .ToList();

        return ApiResponse<BudgetKpiReportDto>.SuccessResult(new BudgetKpiReportDto
        {
            Summary = new BudgetKpiSummaryDto
            {
                BudgetPlanId = plan.Id,
                BudgetNo = plan.BudgetNo,
                BudgetCode = plan.BudgetCode,
                InitialBiomassKg = Round(initial),
                FinalBiomassKg = Round(final),
                SalesTon = Round(sales / 1000m),
                FeedKg = Round(feed),
                MortalityKg = Round(mortality),
                MortalityCount = mortalityCount,
                InitialLiveCount = initialLiveCount,
                SalesCount = salesCount,
                FinalLiveCount = finalLiveCount,
                ProducedBiomassKg = Round(produced),
                Fcr = produced <= 0 ? 0m : Round(feed / produced),
                MortalityRatePercent = initialLiveCount <= 0 ? 0m : Round(mortalityCount * 100m / initialLiveCount)
            },
            MonthlyRows = monthlyRows
        }, "Islem basarili.");
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }
}
