using aqua_api.Modules.Aqua.Application.Dtos;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.KpiReport.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.KpiReport.Application.Services;

public class KpiReportService : IKpiReportService
{
    private const int ForecastDays = 30;
    private const int LegacyOpenEndedYearThreshold = 1901;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDevirFcrReportService _devirFcrReportService;

    public KpiReportService(IUnitOfWork unitOfWork, IDevirFcrReportService devirFcrReportService)
    {
        _unitOfWork = unitOfWork;
        _devirFcrReportService = devirFcrReportService;
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
                    ProjectName = x.ProjectName
                })
                .ToListAsync();

            return ApiResponse<List<KpiReportProjectOptionDto>>.SuccessResult(projects, "KPI report projects loaded.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<KpiReportProjectOptionDto>>.ErrorResult(
                "KPI report projects could not be loaded.",
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
                    "Invalid project.",
                    "ProjectId must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            var project = await _unitOfWork.Db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == projectId);

            if (project == null)
            {
                return ApiResponse<RawKpiReportDto>.ErrorResult(
                    "Project not found.",
                    "Project not found.",
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

            return ApiResponse<RawKpiReportDto>.SuccessResult(report, "Raw KPI report loaded.");
        }
        catch (Exception ex)
        {
            return ApiResponse<RawKpiReportDto>.ErrorResult(
                "Raw KPI report could not be loaded.",
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
}
