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
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        var initial = plan.FishBatches.Where(x => !x.IsDeleted).Sum(x => x.InitialBiomassKg);
        var final = rows
            .GroupBy(x => x.BudgetPlanFishBatchId)
            .Select(x => x.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).FirstOrDefault()?.ClosingBiomassKg ?? 0m)
            .Sum();
        var sales = rows.Sum(x => x.SalesKg);
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
            .GroupBy(x => new { x.Year, x.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(group =>
            {
                var opening = group.Sum(x => x.OpeningBiomassKg);
                var closing = group.Sum(x => x.ClosingBiomassKg);
                var groupSales = group.Sum(x => x.SalesKg);
                var groupFeed = group.Sum(x => x.FeedKg);
                var groupMortality = group.Sum(x => x.MortalityKg);
                var producedBiomassKg = Math.Max(0m, closing + groupSales + groupMortality);
                return new BudgetKpiMonthlyDto
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    OpeningBiomassKg = Round(opening),
                    ClosingBiomassKg = Round(closing),
                    GrowthBiomassKg = Round(producedBiomassKg),
                    ProducedBiomassKg = Round(producedBiomassKg),
                    SalesKg = Round(groupSales),
                    FeedKg = Round(groupFeed),
                    MortalityKg = Round(groupMortality),
                    MortalityCount = group.Sum(x => x.MortalityCount),
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
                SalesKg = Round(sales),
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
