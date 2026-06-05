using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.KpiReport.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.KpiReport.Application.Services;

public class KpiReportService : IKpiReportService
{
    private const int ForecastDays = 30;
    private const decimal DefaultTargetHarvestGram = 400m;
    private const int FeedCostFallbackFifo = 1;
    private const int FeedCostFallbackLastPurchase = 2;
    private const int LegacyOpenEndedYearThreshold = 1901;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDevirFcrReportService _devirFcrReportService;
    private readonly ILocalizationService _localizationService;

    public KpiReportService(
        IUnitOfWork unitOfWork,
        IDevirFcrReportService devirFcrReportService,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _devirFcrReportService = devirFcrReportService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<KpiReportProjectOptionDto>>> GetProjectsAsync()
    {
        try
        {
            var projects = await _unitOfWork.Db.Projects
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new KpiReportProjectOptionDto
                {
                    Id = x.Id,
                    ProjectCode = x.ProjectCode,
                    ProjectName = x.ProjectName,
                    StartDate = x.StartDate
                })
                .ToListAsync();

            return ApiResponse<List<KpiReportProjectOptionDto>>.SuccessResult(projects, L("KpiReportService.ProjectsLoaded"));
        }
        catch (Exception ex)
        {
            return ApiResponse<List<KpiReportProjectOptionDto>>.ErrorResult(
                L("KpiReportService.ProjectsLoadFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public Task<ApiResponse<DevirFcrReportDto>> GetDevirFcrReportAsync(DevirFcrReportRequestDto request)
    {
        return _devirFcrReportService.GetReportAsync(request);
    }

    public async Task<ApiResponse<RawKpiReportDto>> GetRawKpiReportAsync(long projectId)
    {
        try
        {
            if (projectId <= 0)
            {
                return ApiResponse<RawKpiReportDto>.ErrorResult(
                    L("KpiReportService.InvalidProject"),
                    L("KpiReportService.ProjectIdGreaterThanZero"),
                    StatusCodes.Status400BadRequest);
            }

            var project = await _unitOfWork.Db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == projectId);

            if (project == null)
            {
                return ApiResponse<RawKpiReportDto>.ErrorResult(
                    L("KpiReportService.ProjectNotFound"),
                    L("KpiReportService.ProjectNotFound"),
                    StatusCodes.Status404NotFound);
            }

            var projectCages = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Cage)
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .ToListAsync();

            var activeProjectCages = projectCages
                .Where(x => !x.ReleasedDate.HasValue || x.ReleasedDate.Value.Year <= LegacyOpenEndedYearThreshold)
                .ToList();
            var reportProjectCages = activeProjectCages.Count > 0 && !project.EndDate.HasValue
                ? activeProjectCages
                : projectCages;
            var projectCageIds = reportProjectCages.Select(x => x.Id).Distinct().ToList();

            var movements = projectCageIds.Count == 0
                ? new List<BatchMovement>()
                : await _unitOfWork.Db.BatchMovements
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value))
                    .ToListAsync();

            var balances = projectCageIds.Count == 0
                ? new List<BatchCageBalance>()
                : await _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var latestBalanceByCage = balances
                .GroupBy(x => new { x.ProjectCageId, x.FishBatchId })
                .Select(x => x.OrderByDescending(y => y.AsOfDate).ThenByDescending(y => y.Id).First())
                .GroupBy(x => x.ProjectCageId)
                .ToDictionary(
                    x => x.Key,
                    x => new
                    {
                        LiveCount = x.Sum(y => y.LiveCount),
                        BiomassGram = x.Sum(y => y.BiomassGram)
                    });

            var feedings = await _unitOfWork.Db.Feedings
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var feedingIds = feedings.Select(x => x.Id).Distinct().ToList();
            var feedingLines = feedingIds.Count == 0
                ? new List<FeedingLine>()
                : await _unitOfWork.Db.FeedingLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && feedingIds.Contains(x.FeedingId))
                    .ToListAsync();
            var feedingLineIds = feedingLines.Select(x => x.Id).Distinct().ToList();
            var feedingDistributions = feedingLineIds.Count == 0
                ? new List<FeedingDistribution>()
                : await _unitOfWork.Db.FeedingDistributions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && feedingLineIds.Contains(x.FeedingLineId))
                    .ToListAsync();

            var mortalities = await _unitOfWork.Db.Mortalities
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var mortalityIds = mortalities.Select(x => x.Id).Distinct().ToList();
            var mortalityLines = mortalityIds.Count == 0
                ? new List<MortalityLine>()
                : await _unitOfWork.Db.MortalityLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && mortalityIds.Contains(x.MortalityId))
                    .ToListAsync();

            var warehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .ToListAsync();
            var latestWarehouseBalances = warehouseBalances
                .GroupBy(x => new { x.ProjectId, x.FishBatchId, x.WarehouseId })
                .Select(x => x.OrderByDescending(y => y.AsOfDate).ThenByDescending(y => y.Id).First())
                .ToList();

            var feedingLineById = feedingLines.ToDictionary(x => x.Id, x => x);
            var rows = reportProjectCages
                .Select(projectCage => BuildRawRow(
                    project,
                    projectCage,
                    movements.Where(x => x.ProjectCageId == projectCage.Id),
                    latestBalanceByCage.GetValueOrDefault(projectCage.Id)?.LiveCount,
                    latestBalanceByCage.GetValueOrDefault(projectCage.Id)?.BiomassGram,
                    feedingDistributions.Where(x => x.ProjectCageId == projectCage.Id && feedingLineById.ContainsKey(x.FeedingLineId)),
                    mortalityLines.Where(x => x.ProjectCageId == projectCage.Id)))
                .OrderBy(x => x.CageLabel)
                .ToList();

            var stockedFish = rows.Sum(x => x.StockedFish);
            var liveFish = rows.Sum(x => x.LiveFish);
            var deadFish = rows.Sum(x => x.DeadFish);
            var currentBiomassKg = Round(rows.Sum(x => x.CurrentBiomassKg));
            var warehouseFish = latestWarehouseBalances.Sum(x => x.LiveCount);
            var warehouseBiomassKg = Round(latestWarehouseBalances.Sum(x => x.BiomassGram) / 1000m);
            var totalFeedKg = Round(rows.Sum(x => x.TotalFeedKg));
            var biomassGainKg = Round(rows.Sum(x => x.BiomassGainKg));
            var totalCapacityGram = reportProjectCages.Sum(x => x.Cage?.CapacityGram ?? 0m);
            var daysInSea = Math.Max(1, DaysBetween(project.StartDate, DateTimeProvider.Now.Date));
            var initialBiomassKg = Round(rows.Sum(x => x.StockedFish * x.InitialAverageGram) / 1000m);
            var initialAverageGram = WeightedAverage(initialBiomassKg, stockedFish);
            var currentAverageGram = WeightedAverage(currentBiomassKg, liveFish);
            var dailyBiomassGainKg = Math.Max(0m, liveFish * ((currentAverageGram - initialAverageGram) / Math.Max(daysInSea, 1)) / 1000m);

            var report = new RawKpiReportDto
            {
                ProjectId = project.Id,
                ProjectCode = string.IsNullOrWhiteSpace(project.ProjectCode) ? "-" : project.ProjectCode,
                ProjectName = string.IsNullOrWhiteSpace(project.ProjectName) ? "-" : project.ProjectName,
                DaysInSea = daysInSea,
                StockedFish = stockedFish,
                LiveFish = liveFish,
                WarehouseFish = warehouseFish,
                TotalSystemFish = liveFish + warehouseFish,
                DeadFish = deadFish,
                InitialAverageGram = initialAverageGram,
                CurrentAverageGram = currentAverageGram,
                CurrentBiomassKg = currentBiomassKg,
                WarehouseBiomassKg = warehouseBiomassKg,
                TotalSystemBiomassKg = Round(currentBiomassKg + warehouseBiomassKg),
                TotalFeedKg = totalFeedKg,
                BiomassGainKg = biomassGainKg,
                SurvivalPct = SafePercent(liveFish, stockedFish),
                MortalityPct = SafePercent(deadFish, stockedFish),
                AdgGramPerDay = daysInSea > 0 ? Round((currentAverageGram - initialAverageGram) / daysInSea) : null,
                SgrPctPerDay = daysInSea > 0 && initialAverageGram > 0 && currentAverageGram > 0
                    ? Round(100m * (decimal)(Math.Log((double)currentAverageGram) - Math.Log((double)initialAverageGram)) / daysInSea)
                    : null,
                Fcr = biomassGainKg > 0 ? Round(totalFeedKg / biomassGainKg) : null,
                DensityPct = totalCapacityGram > 0 ? Round((currentBiomassKg * 1000m / totalCapacityGram) * 100m) : null,
                ForecastBiomassKg30d = Round(currentBiomassKg + dailyBiomassGainKg * ForecastDays),
                Rows = rows,
                MetricDefinitions = GetRawMetricDefinitions()
            };

            return ApiResponse<RawKpiReportDto>.SuccessResult(report, L("KpiReportService.RawKpiLoaded"));
        }
        catch (Exception ex)
        {
            return ApiResponse<RawKpiReportDto>.ErrorResult(
                L("KpiReportService.RawKpiLoadFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<ProjectDetailReportDto>> GetProjectDetailReportAsync(long projectId)
    {
        try
        {
            if (projectId <= 0)
            {
                return ApiResponse<ProjectDetailReportDto>.ErrorResult(
                    L("KpiReportService.InvalidProject"),
                    L("KpiReportService.ProjectIdGreaterThanZero"),
                    StatusCodes.Status400BadRequest);
            }

            var project = await _unitOfWork.Db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == projectId);

            if (project == null)
            {
                return ApiResponse<ProjectDetailReportDto>.ErrorResult(
                    L("KpiReportService.ProjectNotFound"),
                    L("KpiReportService.ProjectNotFound"),
                    StatusCodes.Status404NotFound);
            }

            var projectCages = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Cage)
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .ToListAsync();

            var activeProjectCages = projectCages
                .Where(x => !x.ReleasedDate.HasValue || x.ReleasedDate.Value.Year <= LegacyOpenEndedYearThreshold)
                .ToList();
            var reportProjectCages = activeProjectCages.Count > 0 && !project.EndDate.HasValue
                ? activeProjectCages
                : projectCages;
            var reportProjectCageIds = reportProjectCages.Select(x => x.Id).Distinct().ToList();
            var allProjectCageIds = projectCages.Select(x => x.Id).Distinct().ToList();

            var feedings = await _unitOfWork.Db.Feedings
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var feedingIds = feedings.Select(x => x.Id).Distinct().ToList();
            var feedingLines = feedingIds.Count == 0
                ? new List<FeedingLine>()
                : await _unitOfWork.Db.FeedingLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && feedingIds.Contains(x.FeedingId))
                    .ToListAsync();
            var feedingLineIds = feedingLines.Select(x => x.Id).Distinct().ToList();
            var feedingDistributions = feedingLineIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<FeedingDistribution>()
                : await _unitOfWork.Db.FeedingDistributions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && feedingLineIds.Contains(x.FeedingLineId) && allProjectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var mortalities = await _unitOfWork.Db.Mortalities
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var mortalityIds = mortalities.Select(x => x.Id).Distinct().ToList();
            var mortalityLines = mortalityIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<MortalityLine>()
                : await _unitOfWork.Db.MortalityLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && mortalityIds.Contains(x.MortalityId) && allProjectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var netOperations = await _unitOfWork.Db.NetOperations
                .AsNoTracking()
                .Include(x => x.OperationType)
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var netOperationIds = netOperations.Select(x => x.Id).Distinct().ToList();
            var netOperationLines = netOperationIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<NetOperationLine>()
                : await _unitOfWork.Db.NetOperationLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && netOperationIds.Contains(x.NetOperationId) && allProjectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var transfers = await _unitOfWork.Db.Transfers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var transferIds = transfers.Select(x => x.Id).Distinct().ToList();
            var transferLines = transferIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<TransferLine>()
                : await _unitOfWork.Db.TransferLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && transferIds.Contains(x.TransferId)
                        && (allProjectCageIds.Contains(x.FromProjectCageId) || allProjectCageIds.Contains(x.ToProjectCageId)))
                    .ToListAsync();

            var shipments = await _unitOfWork.Db.Shipments
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var shipmentIds = shipments.Select(x => x.Id).Distinct().ToList();
            var shipmentLines = shipmentIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<ShipmentLine>()
                : await _unitOfWork.Db.ShipmentLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && shipmentIds.Contains(x.ShipmentId) && allProjectCageIds.Contains(x.FromProjectCageId))
                    .ToListAsync();

            var weighings = await _unitOfWork.Db.Weighings
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var weighingIds = weighings.Select(x => x.Id).Distinct().ToList();
            var weighingLines = weighingIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<WeighingLine>()
                : await _unitOfWork.Db.WeighingLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && weighingIds.Contains(x.WeighingId) && allProjectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var stockConverts = await _unitOfWork.Db.StockConverts
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
                .ToListAsync();
            var stockConvertIds = stockConverts.Select(x => x.Id).Distinct().ToList();
            var stockConvertLines = stockConvertIds.Count == 0 || allProjectCageIds.Count == 0
                ? new List<StockConvertLine>()
                : await _unitOfWork.Db.StockConvertLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && stockConvertIds.Contains(x.StockConvertId)
                        && (allProjectCageIds.Contains(x.FromProjectCageId) || allProjectCageIds.Contains(x.ToProjectCageId)))
                    .ToListAsync();

            var dailyWeathers = await _unitOfWork.Db.DailyWeathers
                .AsNoTracking()
                .Include(x => x.WeatherType)
                .Include(x => x.WeatherSeverity)
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .ToListAsync();

            var batchMovements = allProjectCageIds.Count == 0
                ? new List<BatchMovement>()
                : await _unitOfWork.Db.BatchMovements
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted
                        && ((x.ProjectCageId.HasValue && allProjectCageIds.Contains(x.ProjectCageId.Value))
                            || (x.FromProjectCageId.HasValue && allProjectCageIds.Contains(x.FromProjectCageId.Value))
                            || (x.ToProjectCageId.HasValue && allProjectCageIds.Contains(x.ToProjectCageId.Value))))
                    .ToListAsync();

            var batchCageBalances = allProjectCageIds.Count == 0
                ? new List<BatchCageBalance>()
                : await _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && allProjectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var latestWarehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .GroupBy(x => new { x.ProjectId, x.FishBatchId, x.WarehouseId })
                .Select(x => x.OrderByDescending(y => y.AsOfDate).ThenByDescending(y => y.Id).First())
                .ToListAsync();

            var stockIds = feedingLines.Select(x => x.StockId)
                .Concat(batchMovements.Where(x => x.FromStockId.HasValue).Select(x => x.FromStockId!.Value))
                .Concat(batchMovements.Where(x => x.ToStockId.HasValue).Select(x => x.ToStockId!.Value))
                .Distinct()
                .ToList();
            var stocks = stockIds.Count == 0
                ? new Dictionary<long, string>()
                : await _unitOfWork.Db.Stocks
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && stockIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => FormatStockLabel(x.ErpStockCode, x.StockName, x.Id));

            var fishBatches = await _unitOfWork.Db.FishBatches
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ProjectId == projectId)
                .ToDictionaryAsync(x => x.Id, x => string.IsNullOrWhiteSpace(x.BatchCode) ? x.Id.ToString() : x.BatchCode);

            var report = BuildProjectDetailReport(
                project,
                projectCages,
                reportProjectCages,
                reportProjectCageIds,
                feedings,
                feedingLines,
                feedingDistributions,
                mortalities,
                mortalityLines,
                netOperations,
                netOperationLines,
                transfers,
                transferLines,
                shipments,
                shipmentLines,
                weighings,
                weighingLines,
                stockConverts,
                stockConvertLines,
                dailyWeathers,
                batchMovements,
                batchCageBalances,
                latestWarehouseBalances,
                stocks,
                fishBatches);

            return ApiResponse<ProjectDetailReportDto>.SuccessResult(report, L("KpiReportService.ProjectDetailLoaded"));
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDetailReportDto>.ErrorResult(
                L("KpiReportService.ProjectDetailLoadFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<BusinessKpiReportDto>> GetBusinessKpiReportAsync(long projectId)
    {
        try
        {
            var rawResponse = await GetRawKpiReportAsync(projectId);
            if (!rawResponse.Success || rawResponse.Data == null)
            {
                return ApiResponse<BusinessKpiReportDto>.ErrorResult(
                    rawResponse.Message ?? L("KpiReportService.BusinessKpiLoadFailed"),
                    rawResponse.ExceptionMessage ?? L("KpiReportService.RawKpiLoadFailed"),
                    rawResponse.StatusCode);
            }

            var rawReport = rawResponse.Data;
            var settings = await _unitOfWork.Db.AquaSettings
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            var feedCostStrategy = settings?.FeedCostFallbackStrategy ?? 0;

            var feedCostInputs = await GetFeedCostInputsAsync(projectId, feedCostStrategy);
            var salePriceInputs = await GetSalePriceInputsAsync(projectId);
            var activityInputs = await GetBusinessActivityInputsAsync(projectId);

            var businessRows = rawReport.Rows
                .Select(row =>
                {
                    var feedCost = feedCostInputs.FeedCostByProjectCage.GetValueOrDefault(row.ProjectCageId);
                    var feedCostPerKg = row.TotalFeedKg > 0 && feedCost > 0
                        ? feedCost / row.TotalFeedKg
                        : feedCostInputs.ProjectAverageFeedCostPerKg;
                    var salePricePerKg = salePriceInputs.SalePriceByProjectCage.GetValueOrDefault(
                        row.ProjectCageId,
                        salePriceInputs.ProjectAverageSalePricePerKg);
                    var activity = activityInputs.GetValueOrDefault(row.ProjectCageId, new BusinessActivityInput());

                    return ToBusinessRow(row, feedCostPerKg, salePricePerKg, activity);
                })
                .OrderBy(x => x.CageLabel)
                .ToList();

            var estimatedFeedCost = Round(businessRows.Sum(x => x.EstimatedFeedCost));
            var projectedHarvestBiomassKg = Round(businessRows.Sum(x => x.ProjectedHarvestBiomassKg));
            var projectedRevenue = Round(businessRows.Sum(x => x.ProjectedRevenue));
            var projectedGrossMargin = Round(projectedRevenue - estimatedFeedCost);
            var targetWeightProgressPct = Clamp(Round(rawReport.CurrentAverageGram / DefaultTargetHarvestGram * 100m), 0m, 999m);
            var daysToTarget = CalculateDaysToTarget(rawReport.CurrentAverageGram, rawReport.AdgGramPerDay);

            var report = new BusinessKpiReportDto
            {
                ProjectId = rawReport.ProjectId,
                ProjectCode = rawReport.ProjectCode,
                ProjectName = rawReport.ProjectName,
                EstimatedFeedCost = estimatedFeedCost,
                FeedCostPerCurrentKg = rawReport.CurrentBiomassKg > 0 ? Round(estimatedFeedCost / rawReport.CurrentBiomassKg) : null,
                ProjectedHarvestBiomassKg = projectedHarvestBiomassKg,
                ProjectedRevenue = projectedRevenue,
                ProjectedGrossMargin = projectedGrossMargin,
                ProjectedMarginPct = projectedRevenue > 0 ? Round(projectedGrossMargin / projectedRevenue * 100m) : null,
                TargetWeightProgressPct = targetWeightProgressPct,
                DaysToTarget = daysToTarget,
                EstimatedHarvestDate = EstimateHarvestDate(daysToTarget),
                ForecastConfidencePct = businessRows.Count > 0 ? Round(businessRows.Average(x => x.ForecastConfidencePct)) : 0m,
                HarvestReadinessPct = businessRows.Count > 0 ? Round(businessRows.Average(x => x.HarvestReadinessPct)) : 0m,
                Assumptions = new BusinessKpiAssumptionsDto
                {
                    ForecastDays = ForecastDays,
                    TargetHarvestGram = DefaultTargetHarvestGram,
                    FeedCostPerKg = feedCostInputs.ProjectAverageFeedCostPerKg,
                    SalePricePerKg = salePriceInputs.ProjectAverageSalePricePerKg
                },
                Rows = businessRows,
                MetricDefinitions = GetBusinessMetricDefinitions()
            };

            return ApiResponse<BusinessKpiReportDto>.SuccessResult(report, L("KpiReportService.BusinessKpiLoaded"));
        }
        catch (Exception ex)
        {
            return ApiResponse<BusinessKpiReportDto>.ErrorResult(
                L("KpiReportService.BusinessKpiLoadFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    private static RawKpiRowDto BuildRawRow(
        Project project,
        ProjectCage projectCage,
        IEnumerable<BatchMovement> movements,
        int? balanceLiveCount,
        decimal? balanceBiomassGram,
        IEnumerable<FeedingDistribution> feedingDistributions,
        IEnumerable<MortalityLine> mortalityLines)
    {
        var movementList = movements.ToList();
        var initialFish = Math.Max(0, movementList
            .Where(x => x.MovementType == BatchMovementType.Stocking && x.SignedCount > 0)
            .Sum(x => x.SignedCount));
        var initialBiomassGram = Math.Max(0m, movementList
            .Where(x => x.MovementType == BatchMovementType.Stocking && x.SignedBiomassGram > 0)
            .Sum(x => x.SignedBiomassGram));
        var initialAverageGram = initialFish > 0 ? Round(initialBiomassGram / initialFish) : 0m;
        var deadFish = Math.Max(0, mortalityLines.Sum(x => x.DeadCount));
        var totalFeedGram = feedingDistributions.Sum(x => x.FeedGram);
        var totalCountDelta = movementList.Sum(x => x.SignedCount);
        var totalBiomassDelta = movementList.Sum(x => x.SignedBiomassGram);
        var hasMovementSnapshot = movementList.Count > 0;
        var liveFish = hasMovementSnapshot
            ? Math.Max(0, totalCountDelta)
            : Math.Max(0, balanceLiveCount ?? initialFish - deadFish);
        var currentBiomassGram = hasMovementSnapshot
            ? Math.Max(0m, totalBiomassDelta)
            : Math.Max(0m, balanceBiomassGram ?? initialBiomassGram - deadFish * initialAverageGram);
        var currentAverageGram = liveFish > 0 ? Round(currentBiomassGram / liveFish) : 0m;
        var currentBiomassKg = Round(currentBiomassGram / 1000m);
        var totalFeedKg = Round(totalFeedGram / 1000m);
        var biomassGainKg = Round(Math.Max(0m, (currentBiomassGram - initialBiomassGram) / 1000m));
        var daysInSea = Math.Max(1, DaysBetween(projectCage.AssignedDate == default ? project.StartDate : projectCage.AssignedDate, DateTimeProvider.Now.Date));
        var dailyBiomassGainKg = Math.Max(0m, ((currentAverageGram - initialAverageGram) / Math.Max(daysInSea, 1)) * liveFish / 1000m);

        return new RawKpiRowDto
        {
            ProjectCageId = projectCage.Id,
            CageLabel = projectCage.Cage?.CageCode ?? projectCage.Cage?.CageName ?? projectCage.Id.ToString(),
            DaysInSea = daysInSea,
            StockedFish = initialFish,
            LiveFish = liveFish,
            DeadFish = deadFish,
            InitialAverageGram = initialAverageGram,
            CurrentAverageGram = currentAverageGram,
            CurrentBiomassKg = currentBiomassKg,
            TotalFeedKg = totalFeedKg,
            BiomassGainKg = biomassGainKg,
            SurvivalPct = SafePercent(liveFish, initialFish),
            MortalityPct = SafePercent(deadFish, initialFish),
            AdgGramPerDay = Round((currentAverageGram - initialAverageGram) / daysInSea),
            SgrPctPerDay = initialAverageGram > 0 && currentAverageGram > 0
                ? Round(100m * (decimal)(Math.Log((double)currentAverageGram) - Math.Log((double)initialAverageGram)) / daysInSea)
                : null,
            Fcr = biomassGainKg > 0 ? Round(totalFeedKg / biomassGainKg) : null,
            DensityPct = projectCage.Cage?.CapacityGram > 0
                ? Round((currentBiomassGram / projectCage.Cage.CapacityGram.Value) * 100m)
                : null,
            ForecastBiomassKg30d = Round(currentBiomassKg + dailyBiomassGainKg * ForecastDays)
        };
    }

    private async Task<FeedCostInput> GetFeedCostInputsAsync(long projectId, int strategy)
    {
        var feedings = await _unitOfWork.Db.Feedings
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
            .ToListAsync();
        var feedingIds = feedings.Select(x => x.Id).Distinct().ToList();
        var feedingLines = feedingIds.Count == 0
            ? new List<FeedingLine>()
            : await _unitOfWork.Db.FeedingLines
                .AsNoTracking()
                .Where(x => !x.IsDeleted && feedingIds.Contains(x.FeedingId))
                .ToListAsync();
        var feedingLineIds = feedingLines.Select(x => x.Id).Distinct().ToList();
        var feedingDistributions = feedingLineIds.Count == 0
            ? new List<FeedingDistribution>()
            : await _unitOfWork.Db.FeedingDistributions
                .AsNoTracking()
                .Where(x => !x.IsDeleted && feedingLineIds.Contains(x.FeedingLineId))
                .ToListAsync();

        var postedGoodsReceiptIds = await _unitOfWork.Db.GoodsReceipts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == DocumentStatus.Posted)
            .Select(x => x.Id)
            .ToListAsync();
        var feedReceiptLines = postedGoodsReceiptIds.Count == 0
            ? new List<GoodsReceiptLine>()
            : await _unitOfWork.Db.GoodsReceiptLines
                .AsNoTracking()
                .Where(x => !x.IsDeleted && postedGoodsReceiptIds.Contains(x.GoodsReceiptId) && x.ItemType == GoodsReceiptItemType.Feed)
                .ToListAsync();

        var stockCostPerKg = feedReceiptLines
            .GroupBy(x => x.StockId)
            .ToDictionary(x => x.Key, x => ComputeFallbackFeedCostPerKg(x.ToList(), strategy));
        var globalFallbackCostPerKg = ComputeFallbackFeedCostPerKg(feedReceiptLines, strategy);
        var feedingLineById = feedingLines.ToDictionary(x => x.Id, x => x);
        var feedCostByProjectCage = new Dictionary<long, decimal>();
        var totalFeedKg = 0m;

        foreach (var distribution in feedingDistributions)
        {
            if (!feedingLineById.TryGetValue(distribution.FeedingLineId, out var line)) continue;
            var feedKg = Math.Max(0m, distribution.FeedGram / 1000m);
            if (feedKg <= 0) continue;

            var costPerKg = stockCostPerKg.GetValueOrDefault(line.StockId, globalFallbackCostPerKg);
            AddValue(feedCostByProjectCage, distribution.ProjectCageId, feedKg * costPerKg);
            totalFeedKg += feedKg;
        }

        var totalFeedCost = feedCostByProjectCage.Values.Sum();
        return new FeedCostInput(
            feedCostByProjectCage,
            totalFeedKg > 0 ? Round(totalFeedCost / totalFeedKg) : Round(globalFallbackCostPerKg));
    }

    private async Task<SalePriceInput> GetSalePriceInputsAsync(long projectId)
    {
        var shipments = await _unitOfWork.Db.Shipments
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
            .ToListAsync();
        var shipmentIds = shipments.Select(x => x.Id).Distinct().ToList();
        var shipmentLines = shipmentIds.Count == 0
            ? new List<ShipmentLine>()
            : await _unitOfWork.Db.ShipmentLines
                .AsNoTracking()
                .Where(x => !x.IsDeleted && shipmentIds.Contains(x.ShipmentId))
                .ToListAsync();

        var totalsByCage = new Dictionary<long, SaleTotals>();
        foreach (var line in shipmentLines)
        {
            var kg = Math.Max(0m, line.BiomassGram / 1000m);
            if (kg <= 0) continue;

            var amount = GetLocalLineAmount(line.LocalLineAmount, line.LineAmount, line.LocalUnitPrice, line.UnitPrice, line.ExchangeRate, kg);
            if (amount <= 0) continue;

            var existing = totalsByCage.GetValueOrDefault(line.FromProjectCageId, new SaleTotals());
            existing.Kg += kg;
            existing.Amount += amount;
            totalsByCage[line.FromProjectCageId] = existing;
        }

        var salePriceByProjectCage = totalsByCage.ToDictionary(
            x => x.Key,
            x => x.Value.Kg > 0 ? Round(x.Value.Amount / x.Value.Kg) : 0m);
        var totalKg = totalsByCage.Values.Sum(x => x.Kg);
        var totalAmount = totalsByCage.Values.Sum(x => x.Amount);

        return new SalePriceInput(
            salePriceByProjectCage,
            totalKg > 0 ? Round(totalAmount / totalKg) : 0m);
    }

    private async Task<Dictionary<long, BusinessActivityInput>> GetBusinessActivityInputsAsync(long projectId)
    {
        var feedings = await _unitOfWork.Db.Feedings
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
            .ToListAsync();
        var feedingIds = feedings.Select(x => x.Id).Distinct().ToList();
        var feedingLines = feedingIds.Count == 0
            ? new List<FeedingLine>()
            : await _unitOfWork.Db.FeedingLines
                .AsNoTracking()
                .Where(x => !x.IsDeleted && feedingIds.Contains(x.FeedingId))
                .ToListAsync();
        var feedingLineById = feedingLines.ToDictionary(x => x.Id, x => x);
        var feedingById = feedings.ToDictionary(x => x.Id, x => x);
        var feedingLineIds = feedingLines.Select(x => x.Id).Distinct().ToList();
        var feedingDistributions = feedingLineIds.Count == 0
            ? new List<FeedingDistribution>()
            : await _unitOfWork.Db.FeedingDistributions
                .AsNoTracking()
                .Where(x => !x.IsDeleted && feedingLineIds.Contains(x.FeedingLineId))
                .ToListAsync();

        var weighings = await _unitOfWork.Db.Weighings
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Status == DocumentStatus.Posted)
            .ToListAsync();
        var weighingIds = weighings.Select(x => x.Id).Distinct().ToList();
        var weighingById = weighings.ToDictionary(x => x.Id, x => x);
        var weighingLines = weighingIds.Count == 0
            ? new List<WeighingLine>()
            : await _unitOfWork.Db.WeighingLines
                .AsNoTracking()
                .Where(x => !x.IsDeleted && weighingIds.Contains(x.WeighingId))
                .ToListAsync();

        var result = new Dictionary<long, BusinessActivityInput>();
        foreach (var distribution in feedingDistributions)
        {
            if (!feedingLineById.TryGetValue(distribution.FeedingLineId, out var line)) continue;
            if (!feedingById.TryGetValue(line.FeedingId, out var feeding)) continue;

            var input = result.GetValueOrDefault(distribution.ProjectCageId, new BusinessActivityInput());
            var date = feeding.FeedingDate.Date;
            input.FeedDates.Add(date);
            result[distribution.ProjectCageId] = input;
        }

        foreach (var line in weighingLines)
        {
            if (!weighingById.TryGetValue(line.WeighingId, out var weighing)) continue;

            var input = result.GetValueOrDefault(line.ProjectCageId, new BusinessActivityInput());
            var date = weighing.WeighingDate.Date;
            if (!input.LastWeighingDate.HasValue || date > input.LastWeighingDate.Value)
            {
                input.LastWeighingDate = date;
            }
            result[line.ProjectCageId] = input;
        }

        return result;
    }

    private ProjectDetailReportDto BuildProjectDetailReport(
        Project project,
        List<ProjectCage> projectCages,
        List<ProjectCage> reportProjectCages,
        List<long> reportProjectCageIds,
        List<Feeding> feedings,
        List<FeedingLine> feedingLines,
        List<FeedingDistribution> feedingDistributions,
        List<Mortality> mortalities,
        List<MortalityLine> mortalityLines,
        List<NetOperation> netOperations,
        List<NetOperationLine> netOperationLines,
        List<Transfer> transfers,
        List<TransferLine> transferLines,
        List<Shipment> shipments,
        List<ShipmentLine> shipmentLines,
        List<Weighing> weighings,
        List<WeighingLine> weighingLines,
        List<StockConvert> stockConverts,
        List<StockConvertLine> stockConvertLines,
        List<DailyWeather> dailyWeathers,
        List<BatchMovement> batchMovements,
        List<BatchCageBalance> batchCageBalances,
        List<BatchWarehouseBalance> latestWarehouseBalances,
        Dictionary<long, string> stockLabelById,
        Dictionary<long, string> fishBatchLabelById)
    {
        var reportCageIdSet = reportProjectCageIds.ToHashSet();
        var cageLabelById = projectCages.ToDictionary(x => x.Id, x => CageLabel(x));
        var feedingById = feedings.ToDictionary(x => x.Id, x => x);
        var feedingLineById = feedingLines.ToDictionary(x => x.Id, x => x);
        var mortalityDateById = mortalities.ToDictionary(x => x.Id, x => DateKey(x.MortalityDate));
        var netOperationById = netOperations.ToDictionary(x => x.Id, x => x);
        var transferById = transfers.ToDictionary(x => x.Id, x => x);
        var shipmentById = shipments.ToDictionary(x => x.Id, x => x);
        var weighingById = weighings.ToDictionary(x => x.Id, x => x);
        var stockConvertById = stockConverts.ToDictionary(x => x.Id, x => x);

        var initialByCage = new Dictionary<long, int>();
        var initialBiomassByCage = new Dictionary<long, decimal>();
        var mortalityByCage = new Dictionary<long, int>();
        var movementCountByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var movementBiomassByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
        var deadBiomassByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
        var stockConvertMovementsByRefId = new Dictionary<long, List<BatchMovement>>();

        foreach (var movement in batchMovements)
        {
            if (movement.MovementType == BatchMovementType.StockConvert)
            {
                if (!stockConvertMovementsByRefId.TryGetValue(movement.ReferenceId, out var convertMovements))
                {
                    convertMovements = new List<BatchMovement>();
                    stockConvertMovementsByRefId[movement.ReferenceId] = convertMovements;
                }
                convertMovements.Add(movement);
            }

            if (!movement.ProjectCageId.HasValue || !reportCageIdSet.Contains(movement.ProjectCageId.Value))
            {
                continue;
            }

            var cageId = movement.ProjectCageId.Value;
            var date = DateKey(movement.MovementDate);
            AddValue(movementCountByCageDate, cageId, date, movement.SignedCount);
            AddValue(movementBiomassByCageDate, cageId, date, movement.SignedBiomassGram);

            if (movement.MovementType == BatchMovementType.Stocking)
            {
                if (movement.SignedCount > 0)
                {
                    AddValue(initialByCage, cageId, movement.SignedCount);
                }
                if (movement.SignedBiomassGram > 0)
                {
                    AddValue(initialBiomassByCage, cageId, movement.SignedBiomassGram);
                }
            }

            if (movement.MovementType == BatchMovementType.Mortality)
            {
                var deadCount = Math.Max(0, -movement.SignedCount);
                var deadBiomass = Math.Max(0m, -movement.SignedBiomassGram);
                if (deadCount > 0)
                {
                    AddValue(mortalityByCage, cageId, deadCount);
                }
                if (deadBiomass > 0)
                {
                    AddValue(deadBiomassByCageDate, cageId, date, deadBiomass);
                }
            }
        }

        var latestBalanceByCage = batchCageBalances
            .Where(x => reportCageIdSet.Contains(x.ProjectCageId))
            .GroupBy(x => new { x.ProjectCageId, x.FishBatchId })
            .Select(x => x.OrderByDescending(y => y.AsOfDate).ThenByDescending(y => y.Id).First())
            .GroupBy(x => x.ProjectCageId)
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    LiveCount = x.Sum(y => y.LiveCount),
                    BiomassGram = x.Sum(y => y.BiomassGram)
                });

        var feedByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
        var feedDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        var feedStocksByCageDate = new Dictionary<long, Dictionary<string, HashSet<long>>>();
        foreach (var distribution in feedingDistributions.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
        {
            if (!feedingLineById.TryGetValue(distribution.FeedingLineId, out var line)) continue;
            if (!feedingById.TryGetValue(line.FeedingId, out var feeding)) continue;

            var date = DateKey(feeding.FeedingDate);
            AddValue(feedByCageDate, distribution.ProjectCageId, date, distribution.FeedGram);
            AddSetValue(feedStocksByCageDate, distribution.ProjectCageId, date, line.StockId);

            var stockText = stockLabelById.GetValueOrDefault(line.StockId, line.StockId.ToString());
            AppendDetail(
                feedDetailsByCageDate,
                distribution.ProjectCageId,
                date,
                JoinDetail(
                    feeding.FeedingNo,
                    Detail("Slot", feeding.FeedingSlot),
                    Detail("Stock", stockText),
                    Detail("Feed", $"{Round(distribution.FeedGram)}g"),
                    feeding.Note));
        }

        var deadByCageDate = new Dictionary<long, Dictionary<string, int>>();
        foreach (var line in mortalityLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
        {
            if (!mortalityDateById.TryGetValue(line.MortalityId, out var date)) continue;
            AddValue(mortalityByCage, line.ProjectCageId, line.DeadCount);
            AddValue(deadByCageDate, line.ProjectCageId, date, line.DeadCount);
        }

        var weatherByDate = dailyWeathers
            .GroupBy(x => DateKey(x.WeatherDate))
            .Select(x => new { Date = x.Key, Weather = x.Last() })
            .ToDictionary(
                x => x.Date,
                x => JoinDetail(
                    x.Weather.WeatherSeverity?.Name,
                    x.Weather.WeatherSeverity != null ? Detail("RiskBase", x.Weather.WeatherSeverity.Score) : null,
                    x.Weather.WeatherType?.Name,
                    x.Weather.TemperatureC.HasValue ? $"{Round(x.Weather.TemperatureC.Value)}C" : null,
                    x.Weather.WindKnot.HasValue ? $"{Round(x.Weather.WindKnot.Value)}kt" : null));

        var netOpsByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var netDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        foreach (var line in netOperationLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
        {
            if (!netOperationById.TryGetValue(line.NetOperationId, out var header)) continue;
            var date = DateKey(header.OperationDate);
            AddValue(netOpsByCageDate, line.ProjectCageId, date, 1);
            AppendDetail(netDetailsByCageDate, line.ProjectCageId, date, JoinDetail(header.OperationNo, header.OperationType?.Name, line.Note, header.Note));
        }

        var transferByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var transferDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        foreach (var line in transferLines)
        {
            if (!transferById.TryGetValue(line.TransferId, out var header)) continue;
            var date = DateKey(header.TransferDate);
            var fromLabel = cageLabelById.GetValueOrDefault(line.FromProjectCageId, line.FromProjectCageId.ToString());
            var toLabel = cageLabelById.GetValueOrDefault(line.ToProjectCageId, line.ToProjectCageId.ToString());
            var batchText = fishBatchLabelById.GetValueOrDefault(line.FishBatchId, line.FishBatchId.ToString());
            var detail = JoinDetail(
                header.TransferNo,
                $"{fromLabel} -> {toLabel}",
                Detail("Batch", batchText),
                Detail("Count", line.FishCount),
                Detail("Average", $"{Round(line.AverageGram)}g"),
                Detail("Biomass", $"{Round(line.BiomassGram)}g"),
                header.Note);

            if (reportCageIdSet.Contains(line.FromProjectCageId))
            {
                AddValue(transferByCageDate, line.FromProjectCageId, date, line.FishCount);
                AppendDetail(transferDetailsByCageDate, line.FromProjectCageId, date, detail);
            }
            if (reportCageIdSet.Contains(line.ToProjectCageId))
            {
                AddValue(transferByCageDate, line.ToProjectCageId, date, line.FishCount);
                if (line.ToProjectCageId != line.FromProjectCageId)
                {
                    AppendDetail(transferDetailsByCageDate, line.ToProjectCageId, date, detail);
                }
            }
        }

        var weighingByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var weighingDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        foreach (var line in weighingLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
        {
            if (!weighingById.TryGetValue(line.WeighingId, out var header)) continue;
            var date = DateKey(header.WeighingDate);
            AddValue(weighingByCageDate, line.ProjectCageId, date, 1);
            AppendDetail(
                weighingDetailsByCageDate,
                line.ProjectCageId,
                date,
                JoinDetail(
                    header.WeighingNo,
                    Detail("Count", line.MeasuredCount),
                    Detail("Average", $"{Round(line.MeasuredAverageGram)}g"),
                    Detail("Biomass", $"{Round(line.MeasuredBiomassGram)}g"),
                    header.Note));
        }

        var shipmentByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var shipmentDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        var shipmentFishByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var shipmentBiomassByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
        foreach (var line in shipmentLines.Where(x => reportCageIdSet.Contains(x.FromProjectCageId)))
        {
            if (!shipmentById.TryGetValue(line.ShipmentId, out var header)) continue;
            var date = DateKey(header.ShipmentDate);
            var fromLabel = cageLabelById.GetValueOrDefault(line.FromProjectCageId, line.FromProjectCageId.ToString());
            AddValue(shipmentByCageDate, line.FromProjectCageId, date, 1);
            AddValue(shipmentFishByCageDate, line.FromProjectCageId, date, line.FishCount);
            AddValue(shipmentBiomassByCageDate, line.FromProjectCageId, date, line.BiomassGram);
            AppendDetail(
                shipmentDetailsByCageDate,
                line.FromProjectCageId,
                date,
                JoinDetail(
                    header.ShipmentNo,
                    $"{fromLabel} -> {header.TargetWarehouseId?.ToString() ?? L("KpiReportService.Detail.ColdStorage")}",
                    Detail("Count", line.FishCount),
                    Detail("Average", $"{Round(line.AverageGram)}g"),
                    Detail("Biomass", $"{Round(line.BiomassGram)}g"),
                    header.Note));
        }

        var convertByCageDate = new Dictionary<long, Dictionary<string, int>>();
        var convertDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
        foreach (var line in stockConvertLines)
        {
            if (!stockConvertById.TryGetValue(line.StockConvertId, out var header)) continue;
            var date = DateKey(header.ConvertDate);
            var fromLabel = cageLabelById.GetValueOrDefault(line.FromProjectCageId, line.FromProjectCageId.ToString());
            var toLabel = cageLabelById.GetValueOrDefault(line.ToProjectCageId, line.ToProjectCageId.ToString());
            var movement = stockConvertMovementsByRefId.GetValueOrDefault(line.StockConvertId)?.FirstOrDefault();
            var stockTransition = FormatStockTransition(movement?.FromStockId, movement?.ToStockId, stockLabelById);
            var toAverageGram = line.AverageGram + line.NewAverageGram;
            var detail = JoinDetail(
                header.ConvertNo,
                $"{fromLabel} -> {toLabel}",
                stockTransition,
                Detail("Count", line.FishCount),
                Detail("Average", $"{Round(line.AverageGram)}g + {Round(line.NewAverageGram)}g = {Round(toAverageGram)}g"),
                Detail("Biomass", $"{Round(line.BiomassGram)}g"),
                header.Note);

            if (reportCageIdSet.Contains(line.FromProjectCageId))
            {
                AddValue(convertByCageDate, line.FromProjectCageId, date, 1);
                AppendDetail(convertDetailsByCageDate, line.FromProjectCageId, date, detail);
            }
            if (reportCageIdSet.Contains(line.ToProjectCageId))
            {
                AddValue(convertByCageDate, line.ToProjectCageId, date, 1);
                if (line.ToProjectCageId != line.FromProjectCageId)
                {
                    AppendDetail(convertDetailsByCageDate, line.ToProjectCageId, date, detail);
                }
            }
        }

        var today = DateTimeProvider.Now.Date;
        var cages = reportProjectCages
            .OrderBy(x => CageLabel(x))
            .Select(projectCage =>
            {
                var cageId = projectCage.Id;
                var feedByDate = feedByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var deadByDate = deadByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var countDeltaByDate = movementCountByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var biomassDeltaByDate = movementBiomassByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var activityDates = new HashSet<string>(
                    feedByDate.Keys
                        .Concat(deadByDate.Keys)
                        .Concat(countDeltaByDate.Keys)
                        .Concat(biomassDeltaByDate.Keys)
                        .Concat(netOpsByCageDate.GetValueOrDefault(cageId)?.Keys ?? Enumerable.Empty<string>())
                        .Concat(transferByCageDate.GetValueOrDefault(cageId)?.Keys ?? Enumerable.Empty<string>())
                        .Concat(shipmentByCageDate.GetValueOrDefault(cageId)?.Keys ?? Enumerable.Empty<string>())
                        .Concat(weighingByCageDate.GetValueOrDefault(cageId)?.Keys ?? Enumerable.Empty<string>())
                        .Concat(convertByCageDate.GetValueOrDefault(cageId)?.Keys ?? Enumerable.Empty<string>())
                        .Concat(weatherByDate.Keys));

                var dailyRows = activityDates
                    .Select(date => new ProjectDetailCageDailyRowDto
                    {
                        Date = date,
                        FeedGram = Round(feedByDate.GetValueOrDefault(date)),
                        FeedStockCount = feedStocksByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date)?.Count ?? 0,
                        FeedDetails = feedDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        DeadCount = deadByDate.GetValueOrDefault(date),
                        DeadBiomassGram = Round(deadBiomassByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0m),
                        CountDelta = countDeltaByDate.GetValueOrDefault(date),
                        BiomassDelta = Round(biomassDeltaByDate.GetValueOrDefault(date)),
                        Weather = weatherByDate.GetValueOrDefault(date, "-"),
                        NetOperationCount = netOpsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        NetOperationDetails = netDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        TransferCount = transferByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        TransferDetails = transferDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        WeighingCount = weighingByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        WeighingDetails = weighingDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        StockConvertCount = convertByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        StockConvertDetails = convertDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        ShipmentCount = shipmentByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        ShipmentDetails = shipmentDetailsByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? new List<string>(),
                        ShipmentFishCount = shipmentFishByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0,
                        ShipmentBiomassGram = Round(shipmentBiomassByCageDate.GetValueOrDefault(cageId)?.GetValueOrDefault(date) ?? 0m),
                        Fed = feedByDate.GetValueOrDefault(date) > 0
                    })
                    .OrderByDescending(x => x.Date)
                    .ToList();

                var initialFish = initialByCage.GetValueOrDefault(cageId);
                var initialBiomass = initialBiomassByCage.GetValueOrDefault(cageId);
                var totalDead = mortalityByCage.GetValueOrDefault(cageId);
                var totalCountDelta = countDeltaByDate.Values.Sum();
                var totalBiomassDelta = biomassDeltaByDate.Values.Sum();
                var currentCount = latestBalanceByCage.GetValueOrDefault(cageId)?.LiveCount ?? Math.Max(0, totalCountDelta);
                var currentBiomass = latestBalanceByCage.GetValueOrDefault(cageId)?.BiomassGram ?? Math.Max(0m, totalBiomassDelta);
                var initialAverage = initialFish > 0 ? initialBiomass / initialFish : 0m;
                var currentAverage = currentCount > 0 ? currentBiomass / currentCount : 0m;
                var missingStart = new[] { project.StartDate.Date, projectCage.AssignedDate.Date, today.AddDays(-60) }.Max();
                var missingFeedingDays = EnumerateDates(missingStart, today)
                    .Where(date => feedByDate.GetValueOrDefault(date) <= 0)
                    .ToList();

                return new ProjectDetailCageReportDto
                {
                    ProjectCageId = cageId,
                    CageLabel = CageLabel(projectCage),
                    InitialFishCount = initialFish,
                    InitialAverageGram = Round(initialAverage),
                    InitialBiomassGram = Round(initialBiomass),
                    CurrentFishCount = Math.Max(0, currentCount),
                    CurrentAverageGram = Round(currentAverage),
                    CurrentBiomassGram = Round(Math.Max(0m, currentBiomass)),
                    TotalDeadCount = totalDead,
                    TotalFeedGram = Round(feedByDate.Values.Sum()),
                    TotalCountDelta = totalCountDelta,
                    TotalBiomassDelta = Round(totalBiomassDelta),
                    MissingFeedingDays = missingFeedingDays,
                    DailyRows = dailyRows
                };
            })
            .ToList();

        var inactiveCageHistory = projectCages
            .Where(x => !reportCageIdSet.Contains(x.Id))
            .Select(x => new ProjectDetailCageHistoryItemDto
            {
                ProjectCageId = x.Id,
                CageLabel = CageLabel(x),
                AssignedDate = x.AssignedDate,
                ReleasedDate = x.ReleasedDate
            })
            .OrderByDescending(x => x.ReleasedDate)
            .ToList();

        var warehouseFishCount = latestWarehouseBalances.Sum(x => x.LiveCount);
        var warehouseBiomassGram = Round(latestWarehouseBalances.Sum(x => x.BiomassGram));
        var activeWarehouseCount = latestWarehouseBalances
            .Where(x => x.LiveCount > 0 || x.BiomassGram > 0)
            .Select(x => x.WarehouseId)
            .Distinct()
            .Count();
        var cageFishCount = cages.Sum(x => x.CurrentFishCount);
        var cageBiomassGram = cages.Sum(x => x.CurrentBiomassGram);

        return new ProjectDetailReportDto
        {
            Project = new ProjectDetailProjectDto
            {
                Id = project.Id,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = (byte)project.Status
            },
            Cages = cages,
            CageHistory = inactiveCageHistory,
            WarehouseSummary = new ProjectDetailWarehouseSummaryDto
            {
                ActiveWarehouseCount = activeWarehouseCount,
                WarehouseFishCount = warehouseFishCount,
                WarehouseBiomassGram = warehouseBiomassGram,
                TotalSystemFishCount = cageFishCount + warehouseFishCount,
                TotalSystemBiomassGram = Round(cageBiomassGram + warehouseBiomassGram)
            }
        };
    }

    private static string CageLabel(ProjectCage projectCage)
    {
        return !string.IsNullOrWhiteSpace(projectCage.Cage?.CageCode)
            ? projectCage.Cage!.CageCode
            : !string.IsNullOrWhiteSpace(projectCage.Cage?.CageName)
                ? projectCage.Cage!.CageName
                : projectCage.Id.ToString();
    }

    private static string DateKey(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    private static IEnumerable<string> EnumerateDates(DateTime startDate, DateTime endDate)
    {
        for (var cursor = startDate.Date; cursor <= endDate.Date; cursor = cursor.AddDays(1))
        {
            yield return DateKey(cursor);
        }
    }

    private static string JoinDetail(params string?[] parts)
    {
        var filtered = parts
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToList();
        return filtered.Count == 0 ? "-" : string.Join(" | ", filtered);
    }

    private static string FormatStockLabel(string? code, string? name, long fallbackId)
    {
        var label = string.Join(" - ", new[] { code, name }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrWhiteSpace(label) ? fallbackId.ToString() : label;
    }

    private string? FormatStockTransition(long? fromStockId, long? toStockId, Dictionary<long, string> stockLabelById)
    {
        if (!fromStockId.HasValue && !toStockId.HasValue) return null;
        var fromText = fromStockId.HasValue ? stockLabelById.GetValueOrDefault(fromStockId.Value, fromStockId.Value.ToString()) : "?";
        var toText = toStockId.HasValue ? stockLabelById.GetValueOrDefault(toStockId.Value, toStockId.Value.ToString()) : "?";
        return Detail("Stock", $"{fromText} -> {toText}");
    }

    private string Detail(string key, object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return $"{L($"KpiReportService.Detail.{key}")}: {value}";
    }

    private string L(string key) => _localizationService.GetLocalizedString(key);

    private static decimal ComputeFallbackFeedCostPerKg(List<GoodsReceiptLine> lines, int strategy)
    {
        var pricedLines = lines
            .Select(line =>
            {
                var quantityKg = GetGoodsReceiptLineKg(line);
                var localLineAmount = GetLocalLineAmount(line.LocalLineAmount, line.LineAmount, line.LocalUnitPrice, line.UnitPrice, line.ExchangeRate, quantityKg);
                var unitCost = GetLineCostPerKg(line);
                return new FeedReceiptPriceInput(line, quantityKg, localLineAmount, unitCost);
            })
            .Where(x => x.QuantityKg > 0 && (x.LocalLineAmount > 0 || (x.UnitCost ?? 0) > 0))
            .ToList();

        if (pricedLines.Count == 0) return 0m;

        if (strategy == FeedCostFallbackLastPurchase)
        {
            var latest = pricedLines.OrderByDescending(x => x.Line.Id).First();
            return Round(latest.UnitCost ?? 0m);
        }

        if (strategy == FeedCostFallbackFifo)
        {
            var earliest = pricedLines.OrderBy(x => x.Line.Id).First();
            return Round(earliest.UnitCost ?? 0m);
        }

        var totalQuantityKg = pricedLines.Sum(x => x.QuantityKg);
        var totalAmount = pricedLines.Sum(x => x.LocalLineAmount > 0 ? x.LocalLineAmount : (x.UnitCost ?? 0m) * x.QuantityKg);
        return totalQuantityKg > 0 ? Round(totalAmount / totalQuantityKg) : 0m;
    }

    private static decimal GetGoodsReceiptLineKg(GoodsReceiptLine line)
    {
        if ((line.TotalGram ?? 0m) > 0) return line.TotalGram!.Value / 1000m;
        return Math.Max(0m, line.QtyUnit ?? 0m);
    }

    private static decimal? GetLineCostPerKg(GoodsReceiptLine line)
    {
        var quantityKg = GetGoodsReceiptLineKg(line);
        if (quantityKg <= 0) return null;

        var localLineAmount = GetLocalLineAmount(line.LocalLineAmount, line.LineAmount, line.LocalUnitPrice, line.UnitPrice, line.ExchangeRate, quantityKg);
        var localUnitPrice = GetLocalUnitPrice(line.LocalUnitPrice, line.UnitPrice, line.ExchangeRate);
        if (localLineAmount > 0) return localLineAmount / quantityKg;
        if (localUnitPrice > 0) return localUnitPrice;
        return null;
    }

    private static decimal GetLocalLineAmount(decimal? localLineAmount, decimal? lineAmount, decimal? localUnitPrice, decimal? unitPrice, decimal? exchangeRate, decimal quantityKg)
    {
        if ((localLineAmount ?? 0m) > 0) return localLineAmount!.Value;
        var rate = (exchangeRate ?? 0m) > 0 ? exchangeRate!.Value : 1m;
        if ((lineAmount ?? 0m) > 0) return lineAmount!.Value * rate;
        var localPrice = GetLocalUnitPrice(localUnitPrice, unitPrice, exchangeRate);
        return localPrice > 0 && quantityKg > 0 ? localPrice * quantityKg : 0m;
    }

    private static decimal GetLocalUnitPrice(decimal? localUnitPrice, decimal? unitPrice, decimal? exchangeRate)
    {
        if ((localUnitPrice ?? 0m) > 0) return localUnitPrice!.Value;
        var rate = (exchangeRate ?? 0m) > 0 ? exchangeRate!.Value : 1m;
        return (unitPrice ?? 0m) * rate;
    }

    private static BusinessKpiRowDto ToBusinessRow(RawKpiRowDto raw, decimal feedCostPerKg, decimal salePricePerKg, BusinessActivityInput activity)
    {
        var targetWeightProgressPct = Clamp(Round(raw.CurrentAverageGram / DefaultTargetHarvestGram * 100m), 0m, 999m);
        var daysToTarget = CalculateDaysToTarget(raw.CurrentAverageGram, raw.AdgGramPerDay);
        var recentWeighingDays = activity.LastWeighingDate.HasValue
            ? DaysBetween(activity.LastWeighingDate.Value, DateTimeProvider.Now.Date)
            : (int?)null;
        var weekStart = DateTimeProvider.Now.Date.AddDays(-6);
        var feedDaysInLastWeek = activity.FeedDates.Count(x => x >= weekStart);
        var forecastConfidencePct = Clamp(
            35m
            + (recentWeighingDays.HasValue && recentWeighingDays.Value <= 14 ? 30m : 0m)
            + (raw.DaysInSea >= 14 ? 15m : 0m)
            + (feedDaysInLastWeek >= 4 ? 20m : 0m),
            25m,
            100m);
        var fcrScore = raw.Fcr == null
            ? 50m
            : Clamp(Round((2.2m - raw.Fcr.Value) / 1.2m * 100m), 0m, 100m);
        var harvestReadinessPct = Clamp(
            Round(targetWeightProgressPct * 0.55m + (raw.SurvivalPct ?? 0m) * 0.25m + fcrScore * 0.2m),
            0m,
            100m);
        var estimatedFeedCost = Round(raw.TotalFeedKg * Math.Max(0m, feedCostPerKg));
        var feedCostPerCurrentKg = raw.CurrentBiomassKg > 0 ? Round(estimatedFeedCost / raw.CurrentBiomassKg) : (decimal?)null;
        var projectedHarvestBiomassKg = raw.ForecastBiomassKg30d;
        var projectedRevenue = Round(projectedHarvestBiomassKg * Math.Max(0m, salePricePerKg));
        var projectedGrossMargin = Round(projectedRevenue - estimatedFeedCost);

        return new BusinessKpiRowDto
        {
            ProjectCageId = raw.ProjectCageId,
            CageLabel = raw.CageLabel,
            TargetWeightProgressPct = targetWeightProgressPct,
            DaysToTarget = daysToTarget,
            EstimatedHarvestDate = EstimateHarvestDate(daysToTarget),
            ForecastConfidencePct = forecastConfidencePct,
            HarvestReadinessPct = harvestReadinessPct,
            EstimatedFeedCost = estimatedFeedCost,
            FeedCostPerCurrentKg = feedCostPerCurrentKg,
            ProjectedHarvestBiomassKg = projectedHarvestBiomassKg,
            ProjectedRevenue = projectedRevenue,
            ProjectedGrossMargin = projectedGrossMargin,
            ProjectedMarginPct = projectedRevenue > 0 ? Round(projectedGrossMargin / projectedRevenue * 100m) : null
        };
    }

    private static int? CalculateDaysToTarget(decimal currentAverageGram, decimal? adgGramPerDay)
    {
        if (currentAverageGram >= DefaultTargetHarvestGram) return 0;
        return adgGramPerDay.HasValue && adgGramPerDay.Value > 0
            ? (int)Math.Ceiling((DefaultTargetHarvestGram - currentAverageGram) / adgGramPerDay.Value)
            : null;
    }

    private static string? EstimateHarvestDate(int? daysToTarget)
    {
        return daysToTarget.HasValue
            ? DateKey(DateTimeProvider.Now.Date.AddDays(daysToTarget.Value))
            : null;
    }

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        return Math.Min(max, Math.Max(min, value));
    }

    private static void AddValue(Dictionary<long, int> target, long key, int value)
    {
        target[key] = target.GetValueOrDefault(key) + value;
    }

    private static void AddValue(Dictionary<long, decimal> target, long key, decimal value)
    {
        target[key] = target.GetValueOrDefault(key) + value;
    }

    private static void AddValue(Dictionary<long, Dictionary<string, int>> target, long key, string date, int value)
    {
        if (!target.TryGetValue(key, out var byDate))
        {
            byDate = new Dictionary<string, int>();
            target[key] = byDate;
        }
        byDate[date] = byDate.GetValueOrDefault(date) + value;
    }

    private static void AddValue(Dictionary<long, Dictionary<string, decimal>> target, long key, string date, decimal value)
    {
        if (!target.TryGetValue(key, out var byDate))
        {
            byDate = new Dictionary<string, decimal>();
            target[key] = byDate;
        }
        byDate[date] = byDate.GetValueOrDefault(date) + value;
    }

    private static void AddSetValue(Dictionary<long, Dictionary<string, HashSet<long>>> target, long key, string date, long value)
    {
        if (!target.TryGetValue(key, out var byDate))
        {
            byDate = new Dictionary<string, HashSet<long>>();
            target[key] = byDate;
        }
        if (!byDate.TryGetValue(date, out var values))
        {
            values = new HashSet<long>();
            byDate[date] = values;
        }
        values.Add(value);
    }

    private static void AppendDetail(Dictionary<long, Dictionary<string, List<string>>> target, long key, string date, string detail)
    {
        if (!target.TryGetValue(key, out var byDate))
        {
            byDate = new Dictionary<string, List<string>>();
            target[key] = byDate;
        }
        if (!byDate.TryGetValue(date, out var details))
        {
            details = new List<string>();
            byDate[date] = details;
        }
        details.Add(detail);
    }

    private static List<KpiMetricDefinitionDto> GetRawMetricDefinitions()
    {
        return new List<KpiMetricDefinitionDto>
        {
            Metric("survivalPct"),
            Metric("mortalityPct"),
            Metric("fcr"),
            Metric("adgGramPerDay"),
            Metric("sgrPctPerDay"),
            Metric("densityPct"),
            Metric("forecastBiomassKg30d")
        };
    }

    private static KpiMetricDefinitionDto Metric(string key)
    {
        return new KpiMetricDefinitionDto
        {
            Key = key,
            LabelKey = $"aqua.rawKpiReport.metrics.{key}",
            DescriptionKey = $"aqua.rawKpiReport.descriptions.{key}",
            FormulaKey = $"aqua.rawKpiReport.formulas.{key}"
        };
    }

    private static List<KpiMetricDefinitionDto> GetBusinessMetricDefinitions()
    {
        return new List<KpiMetricDefinitionDto>
        {
            BusinessMetric("estimatedFeedCost"),
            BusinessMetric("feedCostPerCurrentKg"),
            BusinessMetric("projectedHarvestBiomassKg"),
            BusinessMetric("projectedRevenue"),
            BusinessMetric("projectedGrossMargin"),
            BusinessMetric("daysToTarget"),
            BusinessMetric("harvestReadinessPct"),
            BusinessMetric("forecastConfidencePct")
        };
    }

    private static KpiMetricDefinitionDto BusinessMetric(string key)
    {
        return new KpiMetricDefinitionDto
        {
            Key = key,
            LabelKey = $"aqua.businessKpiReport.metrics.{key}",
            DescriptionKey = $"aqua.businessKpiReport.descriptions.{key}",
            FormulaKey = $"aqua.businessKpiReport.formulas.{key}"
        };
    }

    private static int DaysBetween(DateTime startDate, DateTime endDate)
    {
        return Math.Max(1, (int)Math.Floor((endDate.Date - startDate.Date).TotalDays));
    }

    private static decimal WeightedAverage(decimal biomassKg, int fishCount)
    {
        return fishCount > 0 ? Round((biomassKg * 1000m) / fishCount) : 0m;
    }

    private static decimal? SafePercent(decimal numerator, decimal denominator)
    {
        return denominator > 0 ? Round(numerator / denominator * 100m) : null;
    }

    private static decimal Round(decimal value)
    {
        return decimal.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private sealed record FeedCostInput(Dictionary<long, decimal> FeedCostByProjectCage, decimal ProjectAverageFeedCostPerKg);

    private sealed record SalePriceInput(Dictionary<long, decimal> SalePriceByProjectCage, decimal ProjectAverageSalePricePerKg);

    private sealed record FeedReceiptPriceInput(GoodsReceiptLine Line, decimal QuantityKg, decimal LocalLineAmount, decimal? UnitCost);

    private sealed class SaleTotals
    {
        public decimal Kg { get; set; }
        public decimal Amount { get; set; }
    }

    private sealed class BusinessActivityInput
    {
        public HashSet<DateTime> FeedDates { get; } = new();
        public DateTime? LastWeighingDate { get; set; }
    }
}
