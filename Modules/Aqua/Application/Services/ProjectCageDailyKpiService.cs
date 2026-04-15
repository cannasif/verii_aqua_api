using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class ProjectCageDailyKpiService : IProjectCageDailyKpiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectCageDailyKpiService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>> GetLatestAsync(long? projectId, DateTime? snapshotDate)
        {
            try
            {
                var effectiveDate = (snapshotDate ?? DateTimeProvider.Now).Date;

                var query = _unitOfWork.ProjectCageDailyKpiSnapshots
                    .Query()
                    .Where(x => !x.IsDeleted && x.SnapshotDate.Date == effectiveDate);

                if (projectId.HasValue)
                {
                    query = query.Where(x => x.ProjectId == projectId.Value);
                }

                var items = await query
                    .OrderBy(x => x.ProjectId)
                    .ThenBy(x => x.ProjectCageId)
                    .ThenBy(x => x.FishBatchId)
                    .ToListAsync();

                return ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>.SuccessResult(
                    items.Select(_mapper.Map<ProjectCageDailyKpiSnapshotDto>).ToList(),
                    "KPI snapshot kayitlari getirildi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>.ErrorResult(
                    "KPI snapshot kayitlari getirilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>> CreateSnapshotAsync(CreateProjectCageDailyKpiSnapshotRequest request, long userId)
        {
            try
            {
                var snapshotDate = request.SnapshotDate.Date;
                var balancesQuery = _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Include(x => x.FishBatch)
                    .Where(x => !x.IsDeleted && x.AsOfDate.Date == snapshotDate);

                if (request.ProjectId.HasValue)
                {
                    balancesQuery = balancesQuery.Where(x => x.ProjectCage != null && x.ProjectCage.ProjectId == request.ProjectId.Value);
                }

                var balances = await balancesQuery.ToListAsync();
                var snapshots = new List<ProjectCageDailyKpiSnapshot>();

                foreach (var balance in balances)
                {
                    var firstBalance = await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted
                            && x.ProjectCageId == balance.ProjectCageId
                            && x.FishBatchId == balance.FishBatchId)
                        .OrderBy(x => x.AsOfDate)
                        .FirstOrDefaultAsync();

                    var lookbackDate = snapshotDate.AddDays(-30);

                    var previousBalance = await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted
                            && x.ProjectCageId == balance.ProjectCageId
                            && x.FishBatchId == balance.FishBatchId
                            && x.AsOfDate.Date >= lookbackDate
                            && x.AsOfDate.Date < snapshotDate)
                        .OrderByDescending(x => x.AsOfDate)
                        .FirstOrDefaultAsync();

                    previousBalance ??= firstBalance ?? balance;

                    var feedKg = await _unitOfWork.Db.FeedingDistributions
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted
                            && x.ProjectCageId == balance.ProjectCageId
                            && x.FishBatchId == balance.FishBatchId
                            && x.FeedingLine != null
                            && x.FeedingLine.Feeding != null
                            && x.FeedingLine.Feeding.FeedingDate.Date >= lookbackDate
                            && x.FeedingLine.Feeding.FeedingDate.Date <= snapshotDate)
                        .SumAsync(x => (decimal?)x.FeedGram) ?? 0m;

                    var deadCount = await _unitOfWork.Db.MortalityLines
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted
                            && x.ProjectCageId == balance.ProjectCageId
                            && x.FishBatchId == balance.FishBatchId
                            && x.Mortality != null
                            && x.Mortality.MortalityDate.Date >= lookbackDate
                            && x.Mortality.MortalityDate.Date <= snapshotDate)
                        .SumAsync(x => (int?)x.DeadCount) ?? 0;

                    var days = Math.Max(1d, (snapshotDate - (previousBalance?.AsOfDate.Date ?? snapshotDate)).TotalDays);
                    var initialCount = Math.Max(1, firstBalance?.LiveCount ?? balance.LiveCount);
                    var biomassKg = balance.BiomassGram / 1000m;
                    var previousBiomassKg = (previousBalance?.BiomassGram ?? balance.BiomassGram) / 1000m;
                    var biomassGainKg = Math.Max(0m, biomassKg - previousBiomassKg);
                    var feedKgPeriod = feedKg / 1000m;
                    var survivalPct = Math.Round((decimal)balance.LiveCount / initialCount * 100m, 2);
                    var mortalityPctPeriod = Math.Round((decimal)deadCount / initialCount * 100m, 2);
                    var adg = Math.Round((balance.AverageGram - (previousBalance?.AverageGram ?? balance.AverageGram)) / (decimal)days, 4);
                    var currentWeight = Math.Max(0.0001d, (double)balance.AverageGram);
                    var previousWeight = Math.Max(0.0001d, (double)(previousBalance?.AverageGram ?? balance.AverageGram));
                    var sgr = Math.Round((decimal)(100d * (Math.Log(currentWeight) - Math.Log(previousWeight)) / days), 4);
                    var fcr = biomassGainKg > 0 ? Math.Round(feedKgPeriod / biomassGainKg, 4) : 0m;
                    var capacityGram = balance.ProjectCage?.Cage?.CapacityGram ?? 0m;
                    var capacityUsagePct = capacityGram > 0 ? Math.Round(balance.BiomassGram / capacityGram * 100m, 2) : 0m;
                    var forecastBiomassKg30Days = Math.Round(Math.Max(0m, biomassKg + (adg * balance.LiveCount * 30m / 1000m)), 3);
                    var targetHarvestGram = balance.FishBatch?.TargetHarvestAverageGram ?? 0m;
                    var harvestReadinessScore = targetHarvestGram > 0
                        ? Math.Min(100m, Math.Round(balance.AverageGram / targetHarvestGram * 100m, 2))
                        : 0m;

                    var dataQualityScore = 100m;
                    if (previousBalance == null || previousBalance.AsOfDate.Date == snapshotDate)
                        dataQualityScore -= 35m;
                    if (feedKgPeriod <= 0)
                        dataQualityScore -= 20m;
                    if (deadCount == 0)
                        dataQualityScore -= 10m;
                    if (targetHarvestGram <= 0)
                        dataQualityScore -= 15m;
                    dataQualityScore = Math.Max(0m, dataQualityScore);

                    snapshots.Add(new ProjectCageDailyKpiSnapshot
                    {
                        ProjectId = balance.ProjectCage?.ProjectId ?? request.ProjectId ?? 0,
                        ProjectCageId = balance.ProjectCageId,
                        FishBatchId = balance.FishBatchId,
                        SnapshotDate = snapshotDate,
                        InitialCount = initialCount,
                        LiveCount = balance.LiveCount,
                        DeadCountPeriod = deadCount,
                        AverageGram = balance.AverageGram,
                        BiomassKg = biomassKg,
                        FeedKgPeriod = feedKgPeriod,
                        BiomassGainKgPeriod = biomassGainKg,
                        SurvivalPct = survivalPct,
                        MortalityPctPeriod = mortalityPctPeriod,
                        Fcr = fcr,
                        Adg = adg,
                        Sgr = sgr,
                        CapacityUsagePct = capacityUsagePct,
                        ForecastBiomassKg30Days = forecastBiomassKg30Days,
                        HarvestReadinessScore = harvestReadinessScore,
                        DataQualityScore = dataQualityScore,
                        FormulaNote = "Survival=LiveCount/InitialCount*100, FCR=FeedKg/BiomassGainKg, ADG=(AvgGram-PrevAvgGram)/Day, SGR=100*(ln(Current)-ln(Prev))/Day, CapacityUsage=Biomass/Capacity*100",
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedDate = DateTimeProvider.Now,
                        UpdatedDate = DateTimeProvider.Now,
                        IsDeleted = false
                    });
                }

                var existing = await _unitOfWork.ProjectCageDailyKpiSnapshots
                    .Query()
                    .Where(x => !x.IsDeleted
                        && x.SnapshotDate.Date == snapshotDate
                        && (!request.ProjectId.HasValue || x.ProjectId == request.ProjectId.Value))
                    .ToListAsync();

                foreach (var item in existing)
                {
                    item.IsDeleted = true;
                    item.UpdatedBy = userId;
                    item.UpdatedDate = DateTimeProvider.Now;
                    await _unitOfWork.ProjectCageDailyKpiSnapshots.UpdateAsync(item);
                }

                foreach (var snapshot in snapshots)
                {
                    await _unitOfWork.ProjectCageDailyKpiSnapshots.AddAsync(snapshot);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>.SuccessResult(
                    snapshots.Select(_mapper.Map<ProjectCageDailyKpiSnapshotDto>).ToList(),
                    "KPI snapshot kayitlari olusturuldu.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>.ErrorResult(
                    "KPI snapshot kayitlari olusturulamadi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
