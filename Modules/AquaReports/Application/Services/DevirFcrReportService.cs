using aqua_api.Modules.Aqua.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.AquaReports.Application.Services
{
    public class DevirFcrReportService : IDevirFcrReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;
        private readonly IAquaSettingsService? _aquaSettingsService;

        public DevirFcrReportService(
            IUnitOfWork unitOfWork,
            ILocalizationService localizationService,
            IAquaSettingsService? aquaSettingsService = null)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
            _aquaSettingsService = aquaSettingsService;
        }

        public async Task<ApiResponse<DevirFcrReportDto>> GetReportAsync(DevirFcrReportRequestDto request)
        {
            try
            {
                var requestedFromDate = request.FromDate == default ? (DateTime?)null : request.FromDate.Date;
                var requestedToDate = request.ToDate == default ? (DateTime?)null : request.ToDate.Date;

                if (requestedFromDate.HasValue &&
                    requestedToDate.HasValue &&
                    requestedFromDate.Value > requestedToDate.Value)
                {
                    return ApiResponse<DevirFcrReportDto>.ErrorResult(
                        L("DevirFcrReportService.ToDateGreaterThanOrEqualFromDate"),
                        L("DevirFcrReportService.InvalidDateRange"),
                        StatusCodes.Status400BadRequest);
                }

                var projectIds = (request.ProjectIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (projectIds.Count == 0)
                {
                    return ApiResponse<DevirFcrReportDto>.SuccessResult(new DevirFcrReportDto
                    {
                        FromDate = requestedFromDate ?? DateTimeProvider.Now.Date,
                        ToDate = requestedToDate ?? DateTimeProvider.Now.Date,
                    }, L("DevirFcrReportService.ReportLoaded"));
                }

                var toDate = requestedToDate ?? DateTimeProvider.Now.Date;

                var projects = await _unitOfWork.Db.Projects
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.Id))
                    .ToListAsync();

                var fishBatches = await _unitOfWork.Db.FishBatches
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId))
                    .Select(x => new { x.Id, x.ProjectId })
                    .ToListAsync();
                var fishBatchIds = fishBatches.Select(x => x.Id).Distinct().ToList();
                var projectIdByFishBatchId = fishBatches
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.Key, x => x.First().ProjectId);

                var movements = fishBatchIds.Count == 0
                    ? new List<BatchMovement>()
                    : await _unitOfWork.Db.BatchMovements
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && fishBatchIds.Contains(x.FishBatchId))
                        .ToListAsync();

                var projectStartById = projects.ToDictionary(
                    x => x.Id,
                    x => ResolveProjectLifecycleStart(
                        x,
                        movements.Where(m => projectIdByFishBatchId.GetValueOrDefault(m.FishBatchId) == x.Id)));
                var fromDate = requestedFromDate ?? (projectStartById.Count == 0
                    ? toDate
                    : projectStartById.Values.Min());

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
                var mortalityDateById = mortalities.ToDictionary(x => x.Id, x => x.MortalityDate);
                var shipmentProjectById = shipments.ToDictionary(x => x.Id, x => x.ProjectId);
                var mortalityBiomassCalculationMode = _aquaSettingsService == null
                    ? MortalityBiomassCalculationMode.HistoricalEventWeight
                    : await _aquaSettingsService.GetMortalityBiomassCalculationModeAsync();

                var rows = projects
                    .Select(project => BuildRow(
                        project,
                        requestedFromDate ?? projectStartById.GetValueOrDefault(project.Id, project.StartDate.Date),
                        toDate,
                        movements.Where(x => projectIdByFishBatchId.GetValueOrDefault(x.FishBatchId) == project.Id),
                        feedingDistributions.Where(x =>
                            feedingIdByLineId.TryGetValue(x.FeedingLineId, out var feedingId) &&
                            feedingProjectById.GetValueOrDefault(feedingId) == project.Id),
                        mortalityLines
                            .Where(x => mortalityProjectById.GetValueOrDefault(x.MortalityId) == project.Id)
                            .Select(x => new MortalityLineSnapshot(
                                x.FishBatchId,
                                x.ProjectCageId,
                                x.DeadCount,
                                mortalityDateById.GetValueOrDefault(x.MortalityId, toDate))),
                        shipmentLines.Where(x => shipmentProjectById.GetValueOrDefault(x.ShipmentId) == project.Id),
                        mortalityBiomassCalculationMode))
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
            IEnumerable<MortalityLineSnapshot> mortalityLines,
            IEnumerable<ShipmentLine> shipmentLines,
            MortalityBiomassCalculationMode mortalityBiomassCalculationMode)
        {
            var movementList = movements.ToList();
            var openingMovements = ResolveOpeningMovements(movementList, projectFromDate, toDate);
            var openingSnapshot = BatchReportMassCalculator.CalculateSnapshot(openingMovements);
            var openingShipmentMirrorSnapshot = CalculateShipmentWarehouseMirrorSnapshot(openingMovements);
            var endingSnapshot = BatchReportMassCalculator.CalculateSnapshot(
                movementList.Where(x => x.MovementDate.Date <= toDate));
            var endingShipmentMirrorSnapshot = CalculateShipmentWarehouseMirrorSnapshot(
                movementList.Where(x => x.MovementDate.Date <= toDate));

            // Posted shipments create both a cage exit and, when a target warehouse exists,
            // a warehouse inventory mirror. That mirror is useful for traceability, but the
            // sold fish must not remain in Devir/FCR opening or ending live stock.
            var openingFishCount = Math.Max(
                0,
                openingSnapshot.LiveCount - openingShipmentMirrorSnapshot.LiveCount);
            var endingFishCount = Math.Max(
                0,
                endingSnapshot.LiveCount - endingShipmentMirrorSnapshot.LiveCount);
            var openingBiomassGram = Math.Max(
                0m,
                openingSnapshot.BiomassGram - openingShipmentMirrorSnapshot.BiomassGram);
            var endingBiomassGram = Math.Max(
                0m,
                endingSnapshot.BiomassGram - endingShipmentMirrorSnapshot.BiomassGram);

            var shipmentList = shipmentLines.ToList();
            var mortalityList = mortalityLines.ToList();
            var mortalityMovementGroups = movementList
                .Where(x =>
                    x.MovementType == BatchMovementType.Mortality &&
                    x.ProjectCageId.HasValue &&
                    IsInRange(x.MovementDate, projectFromDate, toDate))
                .GroupBy(x => (x.FishBatchId, ProjectCageId: x.ProjectCageId!.Value))
                .ToDictionary(x => x.Key, x => x.ToList());
            var mortalityLineGroups = mortalityList
                .GroupBy(x => (x.FishBatchId, x.ProjectCageId))
                .ToDictionary(x => x.Key, x => x.ToList());
            var shipmentMovementFishCount = movementList
                .Where(x => x.MovementType == BatchMovementType.Shipment && IsInRange(x.MovementDate, projectFromDate, toDate))
                .Sum(x => Math.Max(0, -x.SignedCount));
            var shipmentMovementBiomassGram = movementList
                .Where(x => x.MovementType == BatchMovementType.Shipment && IsInRange(x.MovementDate, projectFromDate, toDate))
                .Sum(x => Math.Max(0m, -BatchReportMassCalculator.CalculateSignedBiomassGram(x)));

            var shipmentFishCount = shipmentList.Sum(x => x.FishCount);
            var unrepresentedShipmentFishCount = Math.Max(0, shipmentFishCount - shipmentMovementFishCount);
            var shipmentLineAverageGram = shipmentFishCount > 0
                ? shipmentList.Sum(x => BatchReportMassCalculator.CalculateBiomassGram(x.FishCount, x.AverageGram)) / shipmentFishCount
                : 0m;
            var unrepresentedShipmentBiomassGram = BatchReportMassCalculator.CalculateBiomassGram(
                unrepresentedShipmentFishCount,
                shipmentLineAverageGram);
            var shippedBiomassGram = shipmentMovementBiomassGram + unrepresentedShipmentBiomassGram;
            var totalFeedGram = feedingDistributions.Sum(x => x.FeedGram);
            var openingFish = Math.Max(0, openingFishCount);
            var endingFish = Math.Max(0, endingFishCount - unrepresentedShipmentFishCount);
            var adjustedEndingBiomassGram = endingFish > 0
                ? Math.Max(0m, endingBiomassGram - unrepresentedShipmentBiomassGram)
                : 0m;
            var mortalityKeys = mortalityLineGroups.Keys
                .Union(mortalityMovementGroups.Keys)
                .ToList();
            var mortalityFish = mortalityKeys.Sum(key =>
                mortalityLineGroups.TryGetValue(key, out var lines)
                    ? Math.Max(0, lines.Sum(x => x.DeadCount))
                    : Math.Max(0, -mortalityMovementGroups[key].Sum(x => x.SignedCount)));
            var endingAverageGram = endingFish > 0 ? Round(adjustedEndingBiomassGram / endingFish) : 0m;
            var reportedMortalityBiomassGram = mortalityKeys.Sum(key =>
            {
                var lines = mortalityLineGroups.GetValueOrDefault(key) ?? new List<MortalityLineSnapshot>();
                var groupMovements = mortalityMovementGroups.GetValueOrDefault(key) ?? new List<BatchMovement>();
                var deadCount = lines.Count > 0
                    ? Math.Max(0, lines.Sum(x => x.DeadCount))
                    : Math.Max(0, -groupMovements.Sum(x => x.SignedCount));
                var historicalActualBiomassGram = groupMovements.Count > 0
                    ? Math.Max(0m, -groupMovements.Sum(BatchReportMassCalculator.CalculateInventorySignedBiomassGram))
                    : lines.Sum(x => Math.Round(
                        x.DeadCount * ResolveAverageGramAtMortalityDate(movementList, x, endingAverageGram),
                        3,
                        MidpointRounding.AwayFromZero));

                return MortalityReportBiomassCalculator.CalculateReportedBiomassGram(
                    mortalityBiomassCalculationMode,
                    deadCount,
                    historicalActualBiomassGram,
                    movementList,
                    key.FishBatchId,
                    key.ProjectCageId,
                    toDate);
            });
            var carriedOutputBiomassKg = Math.Max(0m, (adjustedEndingBiomassGram + reportedMortalityBiomassGram + shippedBiomassGram) / 1000m);
            var producedBiomassKg = carriedOutputBiomassKg;

            return new DevirFcrReportRowDto
            {
                ProjectId = project.Id,
                ProjectCode = string.IsNullOrWhiteSpace(project.ProjectCode) ? project.Id.ToString() : project.ProjectCode.Trim(),
                ProjectName = string.IsNullOrWhiteSpace(project.ProjectName) ? "-" : project.ProjectName.Trim(),
                OpeningFishCount = openingFish,
                ShipmentFishCount = Math.Max(0, shipmentFishCount),
                MortalityFishCount = mortalityFish,
                MortalityPct = openingFish > 0 ? Round((decimal)mortalityFish / openingFish * 100m) : null,
                EndingFishCount = endingFish,
                EndingAverageGram = endingAverageGram,
                OpeningBiomassKg = Round(Math.Max(0m, openingBiomassGram / 1000m)),
                EndingBiomassKg = Round(Math.Max(0m, adjustedEndingBiomassGram / 1000m)),
                ShippedBiomassKg = Round(Math.Max(0m, shippedBiomassGram / 1000m)),
                MortalityBiomassKg = Round(reportedMortalityBiomassGram / 1000m),
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
                   (movement.SignedCount != 0 || movement.SignedBiomassGram != 0m) &&
                   movement.MovementType is BatchMovementType.OpeningImport or BatchMovementType.Stocking;
        }

        private static List<BatchMovement> ResolveOpeningMovements(
            IReadOnlyCollection<BatchMovement> movements,
            DateTime fromDate,
            DateTime toDate)
        {
            var snapshotMovements = movements
                .Where(x => IsOpeningSnapshotMovement(x, fromDate))
                .ToList();

            if (snapshotMovements.Sum(x => x.SignedCount) > 0)
            {
                return snapshotMovements;
            }

            return movements
                .Where(x =>
                    IsInRange(x.MovementDate, fromDate, toDate) &&
                    x.SignedCount > 0 &&
                    x.MovementType is BatchMovementType.OpeningImport or BatchMovementType.Stocking)
                .ToList();
        }

        private static BatchReportMassSnapshot CalculateShipmentWarehouseMirrorSnapshot(
            IEnumerable<BatchMovement> movements)
        {
            return BatchReportMassCalculator.CalculateSnapshot(movements.Where(x =>
                x.MovementType == BatchMovementType.Shipment &&
                x.SignedCount > 0 &&
                x.WarehouseId.HasValue));
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

        private static decimal ResolveAverageGramAtMortalityDate(
            IReadOnlyCollection<BatchMovement> movements,
            MortalityLineSnapshot mortalityLine,
            decimal fallbackAverageGram)
        {
            var balanceMovements = movements
                .Where(x =>
                    x.FishBatchId == mortalityLine.FishBatchId &&
                    x.ProjectCageId == mortalityLine.ProjectCageId &&
                    x.MovementDate.Date <= mortalityLine.MortalityDate.Date &&
                    x.MovementType != BatchMovementType.Mortality)
                .ToList();

            var snapshot = BatchReportMassCalculator.CalculateSnapshot(balanceMovements);
            if (snapshot.AverageGram > 0m)
            {
                return snapshot.AverageGram;
            }

            return fallbackAverageGram;
        }

        private sealed record MortalityLineSnapshot(long FishBatchId, long ProjectCageId, int DeadCount, DateTime MortalityDate);
    }
}
