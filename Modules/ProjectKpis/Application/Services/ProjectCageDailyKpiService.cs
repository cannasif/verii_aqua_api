using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.ProjectKpis.Application.Services
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
                var lookbackDate = snapshotDate.AddDays(-30);
                var projectCageQuery = _unitOfWork.Db.ProjectCages
                    .AsNoTracking()
                    .Include(x => x.Cage)
                    .Where(x => !x.IsDeleted);

                if (request.ProjectId.HasValue)
                {
                    projectCageQuery = projectCageQuery.Where(x => x.ProjectId == request.ProjectId.Value);
                }

                var projectCages = await projectCageQuery.ToListAsync();
                var projectCageIds = projectCages.Select(x => x.Id).ToList();
                var projectCageById = projectCages.ToDictionary(x => x.Id);

                var movements = projectCageIds.Count == 0
                    ? new List<BatchMovement>()
                    : await _unitOfWork.Db.BatchMovements
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsDeleted &&
                            x.ProjectCageId.HasValue &&
                            projectCageIds.Contains(x.ProjectCageId.Value) &&
                            x.MovementDate.Date <= snapshotDate)
                        .OrderBy(x => x.MovementDate)
                        .ThenBy(x => x.Id)
                        .ToListAsync();

                var movementGroups = movements
                    .GroupBy(x => new
                    {
                        ProjectCageId = x.ProjectCageId!.Value,
                        x.FishBatchId
                    })
                    .ToList();
                var fishBatchIds = movementGroups.Select(x => x.Key.FishBatchId).Distinct().ToList();
                var fishBatchById = fishBatchIds.Count == 0
                    ? new Dictionary<long, FishBatch>()
                    : await _unitOfWork.Db.FishBatches
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && fishBatchIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id);

                var feedRows = projectCageIds.Count == 0
                    ? []
                    : await (
                        from distribution in _unitOfWork.Db.FeedingDistributions.AsNoTracking()
                        join line in _unitOfWork.Db.FeedingLines.AsNoTracking()
                            on distribution.FeedingLineId equals line.Id
                        join feeding in _unitOfWork.Db.Feedings.AsNoTracking()
                            on line.FeedingId equals feeding.Id
                        where !distribution.IsDeleted
                            && !line.IsDeleted
                            && !feeding.IsDeleted
                            && feeding.Status == DocumentStatus.Posted
                            && projectCageIds.Contains(distribution.ProjectCageId)
                            && feeding.FeedingDate.Date >= lookbackDate
                            && feeding.FeedingDate.Date <= snapshotDate
                        select new
                        {
                            distribution.ProjectCageId,
                            distribution.FishBatchId,
                            distribution.FeedGram
                        })
                        .ToListAsync();
                var feedGramByPair = feedRows
                    .GroupBy(x => (x.ProjectCageId, x.FishBatchId))
                    .ToDictionary(x => x.Key, x => x.Sum(y => y.FeedGram));

                var mortalityRows = projectCageIds.Count == 0
                    ? []
                    : await (
                        from line in _unitOfWork.Db.MortalityLines.AsNoTracking()
                        join mortality in _unitOfWork.Db.Mortalities.AsNoTracking()
                            on line.MortalityId equals mortality.Id
                        where !line.IsDeleted
                            && !mortality.IsDeleted
                            && mortality.Status == DocumentStatus.Posted
                            && projectCageIds.Contains(line.ProjectCageId)
                            && mortality.MortalityDate.Date >= lookbackDate
                            && mortality.MortalityDate.Date <= snapshotDate
                        select new
                        {
                            line.ProjectCageId,
                            line.FishBatchId,
                            line.DeadCount
                        })
                        .ToListAsync();
                var deadCountByPair = mortalityRows
                    .GroupBy(x => (x.ProjectCageId, x.FishBatchId))
                    .ToDictionary(x => x.Key, x => x.Sum(y => y.DeadCount));

                var snapshots = new List<ProjectCageDailyKpiSnapshot>();

                foreach (var movementGroup in movementGroups)
                {
                    var pair = (movementGroup.Key.ProjectCageId, movementGroup.Key.FishBatchId);
                    var orderedMovements = movementGroup.ToList();
                    var currentState = BatchReportMassCalculator.CalculateSnapshot(orderedMovements);
                    if (currentState.LiveCount <= 0)
                    {
                        continue;
                    }

                    var inboundMovements = orderedMovements
                        .Where(x => x.SignedCount > 0)
                        .ToList();
                    if (inboundMovements.Count == 0)
                    {
                        continue;
                    }

                    var firstInboundDate = inboundMovements.Min(x => x.MovementDate.Date);
                    var initialMovements = inboundMovements
                        .Where(x => x.MovementDate.Date == firstInboundDate)
                        .ToList();
                    var previousMovements = orderedMovements
                        .Where(x => x.MovementDate.Date < lookbackDate)
                        .ToList();
                    var initialState = BatchReportMassCalculator.CalculateSnapshot(initialMovements);
                    var previousState = previousMovements.Count > 0
                        ? BatchReportMassCalculator.CalculateSnapshot(previousMovements)
                        : initialState;
                    var previousDate = previousMovements.Count > 0
                        ? previousMovements.Max(x => x.MovementDate.Date)
                        : firstInboundDate;
                    var days = Math.Max(1d, (snapshotDate - previousDate).TotalDays);
                    var initialCount = Math.Max(1, inboundMovements.Sum(x => x.SignedCount));
                    var biomassGram = currentState.BiomassGram;
                    var previousBiomassGram = previousState.BiomassGram;
                    var biomassKg = biomassGram / 1000m;
                    var previousBiomassKg = previousBiomassGram / 1000m;
                    var biomassGainKg = Math.Max(0m, biomassKg - previousBiomassKg);
                    var feedKgPeriod = feedGramByPair.GetValueOrDefault(pair) / 1000m;
                    var deadCount = deadCountByPair.GetValueOrDefault(pair);
                    var survivalPct = Math.Round((decimal)currentState.LiveCount / initialCount * 100m, 2);
                    var mortalityPctPeriod = Math.Round((decimal)deadCount / initialCount * 100m, 2);
                    var adg = Math.Round((currentState.AverageGram - previousState.AverageGram) / (decimal)days, 4);
                    var currentWeight = Math.Max(0.0001d, (double)currentState.AverageGram);
                    var previousWeight = Math.Max(0.0001d, (double)previousState.AverageGram);
                    var sgr = Math.Round((decimal)(100d * (Math.Log(currentWeight) - Math.Log(previousWeight)) / days), 4);
                    var fcr = biomassGainKg > 0 ? Math.Round(feedKgPeriod / biomassGainKg, 4) : 0m;
                    var projectCage = projectCageById[movementGroup.Key.ProjectCageId];
                    var fishBatch = fishBatchById.GetValueOrDefault(movementGroup.Key.FishBatchId);
                    var capacityGram = projectCage.Cage?.CapacityGram ?? 0m;
                    var capacityUsagePct = capacityGram > 0 ? Math.Round(biomassGram / capacityGram * 100m, 2) : 0m;
                    var forecastBiomassKg30Days = Math.Round(Math.Max(0m, biomassKg + (adg * currentState.LiveCount * 30m / 1000m)), 3);
                    var targetHarvestGram = fishBatch?.TargetHarvestAverageGram ?? 0m;
                    var harvestReadinessScore = targetHarvestGram > 0
                        ? Math.Min(100m, Math.Round(currentState.AverageGram / targetHarvestGram * 100m, 2))
                        : 0m;

                    var dataQualityScore = 100m;
                    if (previousMovements.Count == 0)
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
                        ProjectId = projectCage.ProjectId,
                        ProjectCageId = movementGroup.Key.ProjectCageId,
                        FishBatchId = movementGroup.Key.FishBatchId,
                        SnapshotDate = snapshotDate,
                        InitialCount = initialCount,
                        LiveCount = currentState.LiveCount,
                        DeadCountPeriod = deadCount,
                        AverageGram = currentState.AverageGram,
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
