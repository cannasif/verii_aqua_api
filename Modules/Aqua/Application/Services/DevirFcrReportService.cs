using aqua_api.Modules.Aqua.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class DevirFcrReportService : IDevirFcrReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public DevirFcrReportService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<DevirFcrReportDto>> GetReportAsync(DevirFcrReportRequestDto request)
        {
            try
            {
                var projectIds = (request.ProjectIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (projectIds.Count == 0)
                {
                    return ApiResponse<DevirFcrReportDto>.SuccessResult(new DevirFcrReportDto
                    {
                        FromDate = request.FromDate.Date,
                        ToDate = request.ToDate.Date,
                    }, L("DevirFcrReportService.ReportLoaded"));
                }

                var toDate = DateTimeProvider.Now.Date;

                var projects = await _unitOfWork.Db.Projects
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.Id))
                    .ToListAsync();

                var projectCages = await _unitOfWork.Db.ProjectCages
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId))
                    .ToListAsync();

                var projectCageIds = projectCages.Select(x => x.Id).Distinct().ToList();
                var projectIdByCageId = projectCages
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.Key, x => x.First().ProjectId);

                var movements = projectCageIds.Count == 0
                    ? new List<BatchMovement>()
                    : await _unitOfWork.Db.BatchMovements
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value))
                        .ToListAsync();

                var projectStartById = projects.ToDictionary(
                    x => x.Id,
                    x => ResolveProjectLifecycleStart(
                        x,
                        movements.Where(m =>
                            m.ProjectCageId.HasValue &&
                            projectIdByCageId.GetValueOrDefault(m.ProjectCageId.Value) == x.Id)));
                var fromDate = projectStartById.Count == 0
                    ? toDate
                    : projectStartById.Values.Min();

                var feedings = await _unitOfWork.Db.Feedings
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId) && x.Status == DocumentStatus.Posted)
                    .ToListAsync();
                var feedingIds = feedings
                    .Where(x => IsInRange(x.FeedingDate, fromDate, toDate))
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

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
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId) && x.Status == DocumentStatus.Posted)
                    .ToListAsync();
                var mortalityIds = mortalities
                    .Where(x => IsInRange(x.MortalityDate, fromDate, toDate))
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

                var mortalityLines = mortalityIds.Count == 0
                    ? new List<MortalityLine>()
                    : await _unitOfWork.Db.MortalityLines
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && mortalityIds.Contains(x.MortalityId))
                        .ToListAsync();

                var shipments = await _unitOfWork.Db.Shipments
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId) && x.Status == DocumentStatus.Posted)
                    .ToListAsync();
                var shipmentIds = shipments
                    .Where(x => IsInRange(x.ShipmentDate, fromDate, toDate))
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

                var shipmentLines = shipmentIds.Count == 0
                    ? new List<ShipmentLine>()
                    : await _unitOfWork.Db.ShipmentLines
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && shipmentIds.Contains(x.ShipmentId))
                        .ToListAsync();

                var feedingIdByLineId = feedingLines.ToDictionary(x => x.Id, x => x.FeedingId);
                var feedingProjectById = feedings.ToDictionary(x => x.Id, x => x.ProjectId);
                var mortalityProjectById = mortalities.ToDictionary(x => x.Id, x => x.ProjectId);
                var shipmentProjectById = shipments.ToDictionary(x => x.Id, x => x.ProjectId);

                var rows = projects
                    .Select(project => BuildRow(
                        project,
                        projectStartById.GetValueOrDefault(project.Id, project.StartDate.Date),
                        toDate,
                        movements.Where(x => x.ProjectCageId.HasValue && projectIdByCageId.GetValueOrDefault(x.ProjectCageId.Value) == project.Id),
                        feedingDistributions.Where(x =>
                            feedingIdByLineId.TryGetValue(x.FeedingLineId, out var feedingId) &&
                            feedingProjectById.GetValueOrDefault(feedingId) == project.Id),
                        mortalityLines.Where(x => mortalityProjectById.GetValueOrDefault(x.MortalityId) == project.Id),
                        shipmentLines.Where(x => shipmentProjectById.GetValueOrDefault(x.ShipmentId) == project.Id)))
                    .OrderBy(x => x.ProjectCode)
                    .ToList();

                var response = new DevirFcrReportDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Rows = rows,
                    Totals = BuildTotals(rows),
                };

                return ApiResponse<DevirFcrReportDto>.SuccessResult(response, L("DevirFcrReportService.ReportLoaded"));
            }
            catch (Exception ex)
            {
                return ApiResponse<DevirFcrReportDto>.ErrorResult(
                    L("DevirFcrReportService.ReportLoadFailed"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private string L(string key) => _localizationService.GetLocalizedString(key);

        private static DevirFcrReportRowDto BuildRow(
            Project project,
            DateTime projectFromDate,
            DateTime toDate,
            IEnumerable<BatchMovement> movements,
            IEnumerable<FeedingDistribution> feedingDistributions,
            IEnumerable<MortalityLine> mortalityLines,
            IEnumerable<ShipmentLine> shipmentLines)
        {
            var movementList = movements.ToList();
            var openingFishCount = movementList
                .Where(x => IsOpeningSnapshotMovement(x, projectFromDate))
                .Sum(x => x.SignedCount);
            var endingFishCount = movementList
                .Where(x => x.MovementDate.Date <= toDate)
                .Sum(x => x.SignedCount);
            var openingBiomassGram = movementList
                .Where(x => IsOpeningSnapshotMovement(x, projectFromDate))
                .Sum(x => x.SignedBiomassGram);
            var endingBiomassGram = movementList
                .Where(x => x.MovementDate.Date <= toDate)
                .Sum(x => x.SignedBiomassGram);

            var shipmentList = shipmentLines.ToList();
            var mortalityList = mortalityLines.ToList();
            var mortalityMovementBiomassGram = movementList
                .Where(x => x.MovementType == BatchMovementType.Mortality && IsInRange(x.MovementDate, projectFromDate, toDate))
                .Sum(x => Math.Max(0m, -x.SignedBiomassGram));

            var shippedBiomassGram = shipmentList.Sum(x => x.BiomassGram);
            var totalFeedGram = feedingDistributions.Sum(x => x.FeedGram);
            var producedBiomassKg = Math.Max(0m, (endingBiomassGram + shippedBiomassGram - openingBiomassGram) / 1000m);
            var openingFish = Math.Max(0, openingFishCount);
            var endingFish = Math.Max(0, endingFishCount);
            var mortalityFish = Math.Max(0, mortalityList.Sum(x => x.DeadCount));

            return new DevirFcrReportRowDto
            {
                ProjectId = project.Id,
                ProjectCode = string.IsNullOrWhiteSpace(project.ProjectCode) ? project.Id.ToString() : project.ProjectCode.Trim(),
                ProjectName = string.IsNullOrWhiteSpace(project.ProjectName) ? "-" : project.ProjectName.Trim(),
                OpeningFishCount = openingFish,
                ShipmentFishCount = Math.Max(0, shipmentList.Sum(x => x.FishCount)),
                MortalityFishCount = mortalityFish,
                MortalityPct = openingFish > 0 ? Round((decimal)mortalityFish / openingFish * 100m) : null,
                EndingFishCount = endingFish,
                EndingAverageGram = endingFish > 0 ? Round(endingBiomassGram / endingFish) : 0m,
                OpeningBiomassKg = Round(Math.Max(0m, openingBiomassGram / 1000m)),
                EndingBiomassKg = Round(Math.Max(0m, endingBiomassGram / 1000m)),
                ShippedBiomassKg = Round(Math.Max(0m, shippedBiomassGram / 1000m)),
                MortalityBiomassKg = Round(Math.Max(0m, mortalityMovementBiomassGram / 1000m)),
                TotalFeedKg = Round(Math.Max(0m, totalFeedGram / 1000m)),
                ProducedBiomassKg = Round(producedBiomassKg),
                Fcr = producedBiomassKg > 0 ? Round((totalFeedGram / 1000m) / producedBiomassKg) : null,
            };
        }

        private static DevirFcrReportTotalDto BuildTotals(IReadOnlyCollection<DevirFcrReportRowDto> rows)
        {
            var totals = new DevirFcrReportTotalDto
            {
                OpeningFishCount = rows.Sum(x => x.OpeningFishCount),
                ShipmentFishCount = rows.Sum(x => x.ShipmentFishCount),
                MortalityFishCount = rows.Sum(x => x.MortalityFishCount),
                EndingFishCount = rows.Sum(x => x.EndingFishCount),
                OpeningBiomassKg = Round(rows.Sum(x => x.OpeningBiomassKg)),
                EndingBiomassKg = Round(rows.Sum(x => x.EndingBiomassKg)),
                ShippedBiomassKg = Round(rows.Sum(x => x.ShippedBiomassKg)),
                MortalityBiomassKg = Round(rows.Sum(x => x.MortalityBiomassKg)),
                TotalFeedKg = Round(rows.Sum(x => x.TotalFeedKg)),
                ProducedBiomassKg = Round(rows.Sum(x => x.ProducedBiomassKg)),
            };

            totals.MortalityPct = totals.OpeningFishCount > 0
                ? Round((decimal)totals.MortalityFishCount / totals.OpeningFishCount * 100m)
                : null;
            totals.EndingAverageGram = totals.EndingFishCount > 0
                ? Round((totals.EndingBiomassKg * 1000m) / totals.EndingFishCount)
                : 0m;
            totals.Fcr = totals.ProducedBiomassKg > 0
                ? Round(totals.TotalFeedKg / totals.ProducedBiomassKg)
                : null;

            return totals;
        }

        private static bool IsInRange(DateTime value, DateTime fromDate, DateTime toDate)
        {
            var date = value.Date;
            return date >= fromDate && date <= toDate;
        }

        private static bool IsOpeningSnapshotMovement(BatchMovement movement, DateTime fromDate)
        {
            var date = movement.MovementDate.Date;
            if (date < fromDate)
            {
                return true;
            }

            return date == fromDate &&
                   movement.SignedCount > 0 &&
                   movement.MovementType is BatchMovementType.OpeningImport or BatchMovementType.Stocking;
        }

        private static DateTime ResolveProjectLifecycleStart(Project project, IEnumerable<BatchMovement> movements)
        {
            var dates = movements
                .Where(x => x.SignedCount > 0 && x.MovementType is BatchMovementType.OpeningImport or BatchMovementType.Stocking)
                .Select(x => x.MovementDate.Date)
                .ToList();

            if (project.StartDate != default)
            {
                dates.Add(project.StartDate.Date);
            }

            return dates.Count == 0 ? DateTimeProvider.Now.Date : dates.Min();
        }

        private static decimal Round(decimal value) => decimal.Round(value, 3, MidpointRounding.AwayFromZero);
    }
}
