using aqua_api.Modules.Aqua.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class DashboardProjectReportService : IDashboardProjectReportService
    {
        private const int LegacyOpenEndedYearThreshold = 1901;

        private readonly IUnitOfWork _unitOfWork;

        public DashboardProjectReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<DashboardProjectsResponseDto>> GetProjectSummariesAsync(IEnumerable<long> projectIds)
        {
            try
            {
                var reports = await LoadProjectReportsAsync(projectIds);
                var yesterday = DateTimeProvider.Now.Date.AddDays(-1);
                var response = new DashboardProjectsResponseDto
                {
                    Projects = reports
                        .Select(MapProjectSummary)
                        .OrderBy(x => x.ProjectCode)
                        .ToList(),
                    YesterdayDate = yesterday,
                    YesterdayEntryMissing = reports.Count > 0 && !reports.Any(project =>
                        project.Cages.Any(cage =>
                            cage.DailyRows.Any(row => row.Date == ToDateOnly(yesterday) && HasDailyEntry(row))))
                };

                return ApiResponse<DashboardProjectsResponseDto>.SuccessResult(response, "Dashboard project summaries loaded.");
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardProjectsResponseDto>.ErrorResult(
                    "Dashboard project summaries could not be loaded.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<DashboardProjectDetailDto>> GetProjectDetailAsync(long projectId)
        {
            try
            {
                var report = (await LoadProjectReportsAsync(new[] { projectId })).FirstOrDefault();
                if (report == null)
                {
                    return ApiResponse<DashboardProjectDetailDto>.ErrorResult(
                        "Project not found.",
                        "Project not found.",
                        StatusCodes.Status404NotFound);
                }

                var result = new DashboardProjectDetailDto
                {
                    Cages = report.Cages
                        .OrderBy(x => x.CageLabel)
                        .Select(MapProjectDetailCage)
                        .ToList()
                };

                return ApiResponse<DashboardProjectDetailDto>.SuccessResult(result, "Dashboard project detail loaded.");
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardProjectDetailDto>.ErrorResult(
                    "Dashboard project detail could not be loaded.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<List<ProjectDashboardReport>> LoadProjectReportsAsync(IEnumerable<long> projectIds)
        {
            var uniqueProjectIds = projectIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (uniqueProjectIds.Count == 0)
            {
                return new List<ProjectDashboardReport>();
            }

            var projects = await _unitOfWork.Db.Projects
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.Id))
                .ToListAsync();

            var projectCages = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Cage)
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();

            var projectCageIds = projectCages.Select(x => x.Id).Distinct().ToList();

            var feedings = await _unitOfWork.Db.Feedings
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
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
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var mortalityIds = mortalities.Select(x => x.Id).Distinct().ToList();

            var mortalityLines = mortalityIds.Count == 0
                ? new List<MortalityLine>()
                : await _unitOfWork.Db.MortalityLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && mortalityIds.Contains(x.MortalityId))
                    .ToListAsync();

            var batchCageBalances = projectCageIds.Count == 0
                ? new List<BatchCageBalance>()
                : await _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && projectCageIds.Contains(x.ProjectCageId))
                    .ToListAsync();

            var batchWarehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();

            var dailyWeathers = await _unitOfWork.Db.DailyWeathers
                .AsNoTracking()
                .Include(x => x.WeatherType)
                .Include(x => x.WeatherSeverity)
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();

            var netOperations = await _unitOfWork.Db.NetOperations
                .AsNoTracking()
                .Include(x => x.OperationType)
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var netOperationIds = netOperations.Select(x => x.Id).Distinct().ToList();

            var netOperationLines = netOperationIds.Count == 0
                ? new List<NetOperationLine>()
                : await _unitOfWork.Db.NetOperationLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && netOperationIds.Contains(x.NetOperationId))
                    .ToListAsync();

            var transfers = await _unitOfWork.Db.Transfers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var transferIds = transfers.Select(x => x.Id).Distinct().ToList();

            var transferLines = transferIds.Count == 0
                ? new List<TransferLine>()
                : await _unitOfWork.Db.TransferLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && transferIds.Contains(x.TransferId))
                    .ToListAsync();

            var shipments = await _unitOfWork.Db.Shipments
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var shipmentIds = shipments.Select(x => x.Id).Distinct().ToList();
            var targetWarehouseIds = shipments.Where(x => x.TargetWarehouseId.HasValue).Select(x => x.TargetWarehouseId!.Value).Distinct().ToList();

            var shipmentLines = shipmentIds.Count == 0
                ? new List<ShipmentLine>()
                : await _unitOfWork.Db.ShipmentLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && shipmentIds.Contains(x.ShipmentId))
                    .ToListAsync();

            var weighings = await _unitOfWork.Db.Weighings
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var weighingIds = weighings.Select(x => x.Id).Distinct().ToList();

            var weighingLines = weighingIds.Count == 0
                ? new List<WeighingLine>()
                : await _unitOfWork.Db.WeighingLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && weighingIds.Contains(x.WeighingId))
                    .ToListAsync();

            var stockConverts = await _unitOfWork.Db.StockConverts
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();
            var stockConvertIds = stockConverts.Select(x => x.Id).Distinct().ToList();

            var stockConvertLines = stockConvertIds.Count == 0
                ? new List<StockConvertLine>()
                : await _unitOfWork.Db.StockConvertLines
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && stockConvertIds.Contains(x.StockConvertId))
                    .ToListAsync();

            var batchMovements = projectCageIds.Count == 0
                ? new List<BatchMovement>()
                : await _unitOfWork.Db.BatchMovements
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted
                        && ((x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value))
                            || (x.FromProjectCageId.HasValue && projectCageIds.Contains(x.FromProjectCageId.Value))
                            || (x.ToProjectCageId.HasValue && projectCageIds.Contains(x.ToProjectCageId.Value))))
                    .ToListAsync();

            var fishBatches = await _unitOfWork.Db.FishBatches
                .AsNoTracking()
                .Where(x => !x.IsDeleted && uniqueProjectIds.Contains(x.ProjectId))
                .ToListAsync();

            var stockIds = feedingLines.Select(x => x.StockId)
                .Concat(batchMovements.Where(x => x.FromStockId.HasValue).Select(x => x.FromStockId!.Value))
                .Concat(batchMovements.Where(x => x.ToStockId.HasValue).Select(x => x.ToStockId!.Value))
                .Concat(fishBatches.Select(x => x.FishStockId))
                .Distinct()
                .ToList();

            var stocks = stockIds.Count == 0
                ? new List<StockEntity>()
                : await _unitOfWork.Db.Stocks
                    .AsNoTracking()
                    .Where(x => stockIds.Contains(x.Id))
                    .ToListAsync();

            var warehouses = targetWarehouseIds.Count == 0
                ? new List<WarehouseEntity>()
                : await _unitOfWork.Db.Warehouses
                    .AsNoTracking()
                    .Where(x => targetWarehouseIds.Contains(x.Id))
                    .ToListAsync();

            return projects
                .OrderBy(x => x.ProjectCode)
                .Select(project => BuildProjectReport(
                    project,
                    projectCages.Where(x => x.ProjectId == project.Id).ToList(),
                    feedings.Where(x => x.ProjectId == project.Id).ToList(),
                    feedingLines,
                    feedingDistributions,
                    mortalities.Where(x => x.ProjectId == project.Id).ToList(),
                    mortalityLines,
                    batchCageBalances,
                    batchWarehouseBalances.Where(x => x.ProjectId == project.Id).ToList(),
                    dailyWeathers.Where(x => x.ProjectId == project.Id).ToList(),
                    netOperations.Where(x => x.ProjectId == project.Id).ToList(),
                    netOperationLines,
                    transfers.Where(x => x.ProjectId == project.Id).ToList(),
                    transferLines,
                    shipments.Where(x => x.ProjectId == project.Id).ToList(),
                    shipmentLines,
                    weighings.Where(x => x.ProjectId == project.Id).ToList(),
                    weighingLines,
                    stockConverts.Where(x => x.ProjectId == project.Id).ToList(),
                    stockConvertLines,
                    batchMovements,
                    stocks,
                    fishBatches.Where(x => x.ProjectId == project.Id).ToList(),
                    warehouses))
                .ToList();
        }

        private static ProjectDashboardReport BuildProjectReport(
            Project project,
            List<ProjectCage> projectCages,
            List<Feeding> feedings,
            List<FeedingLine> feedingLines,
            List<FeedingDistribution> feedingDistributions,
            List<Mortality> mortalities,
            List<MortalityLine> mortalityLines,
            List<BatchCageBalance> batchCageBalances,
            List<BatchWarehouseBalance> batchWarehouseBalances,
            List<DailyWeather> dailyWeathers,
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
            List<BatchMovement> batchMovements,
            List<StockEntity> stocks,
            List<FishBatch> fishBatches,
            List<WarehouseEntity> warehouses)
        {
            var activeProjectCages = projectCages.Where(x => IsActiveProjectCage(x.ReleasedDate)).ToList();
            var projectHasEnded = project.EndDate.HasValue;
            var reportProjectCages = activeProjectCages.Count > 0 && !projectHasEnded ? activeProjectCages : projectCages;
            var reportCageIdSet = reportProjectCages.Select(x => x.Id).ToHashSet();

            var postedFeedingIds = feedings.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var feedingIdToDate = feedings.Where(x => postedFeedingIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.FeedingDate));
            var feedingLineIdToFeedingId = feedingLines.ToDictionary(x => x.Id, x => x.FeedingId);
            var feedingLineById = feedingLines.ToDictionary(x => x.Id, x => x);
            var feedingById = feedings.ToDictionary(x => x.Id, x => x);

            var postedMortalityIds = mortalities.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var mortalityIdToDate = mortalities.Where(x => postedMortalityIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.MortalityDate));

            var postedNetOperationIds = netOperations.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var netOperationIdToDate = netOperations.Where(x => postedNetOperationIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.OperationDate));
            var netOperationById = netOperations.Where(x => postedNetOperationIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var postedTransferIds = transfers.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var transferIdToDate = transfers.Where(x => postedTransferIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.TransferDate));
            var transferById = transfers.Where(x => postedTransferIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var postedShipmentIds = shipments.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var shipmentIdToDate = shipments.Where(x => postedShipmentIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.ShipmentDate));
            var shipmentById = shipments.Where(x => postedShipmentIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var postedWeighingIds = weighings.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var weighingIdToDate = weighings.Where(x => postedWeighingIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.WeighingDate));
            var weighingById = weighings.Where(x => postedWeighingIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var postedStockConvertIds = stockConverts.Where(x => x.Status == DocumentStatus.Posted).Select(x => x.Id).ToHashSet();
            var stockConvertIdToDate = stockConverts.Where(x => postedStockConvertIds.Contains(x.Id)).ToDictionary(x => x.Id, x => ToDateOnly(x.ConvertDate));
            var stockConvertById = stockConverts.Where(x => postedStockConvertIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var stockLabelById = stocks.ToDictionary(
                x => x.Id,
                x => string.Join(" - ", new[] { x.ErpStockCode, x.StockName }.Where(part => !string.IsNullOrWhiteSpace(part))).Trim());
            var stockConvertMovementsByRefId = batchMovements
                .Where(x => x.MovementType == BatchMovementType.StockConvert)
                .GroupBy(x => x.ReferenceId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var cageLabelById = projectCages.ToDictionary(x => x.Id, x => x.Cage?.CageCode ?? x.Cage?.CageName ?? x.Id.ToString());
            var fishBatchLabelById = fishBatches.ToDictionary(x => x.Id, x => !string.IsNullOrWhiteSpace(x.BatchCode) ? x.BatchCode : x.Id.ToString());
            var warehouseLabelById = warehouses.ToDictionary(
                x => x.Id,
                x => !string.IsNullOrWhiteSpace(x.WarehouseName)
                    ? x.WarehouseName
                    : x.ErpWarehouseCode.ToString());

            var initialByCage = new Dictionary<long, int>();
            var initialBiomassByCage = new Dictionary<long, decimal>();
            var mortalityByCage = new Dictionary<long, int>();
            foreach (var row in batchMovements)
            {
                if (!row.ProjectCageId.HasValue || !reportCageIdSet.Contains(row.ProjectCageId.Value)) continue;
                var cageId = row.ProjectCageId.Value;

                if (row.MovementType == BatchMovementType.Stocking)
                {
                    if (row.SignedCount > 0)
                    {
                        initialByCage[cageId] = (initialByCage.GetValueOrDefault(cageId)) + row.SignedCount;
                    }
                    if (row.SignedBiomassGram > 0)
                    {
                        initialBiomassByCage[cageId] = initialBiomassByCage.GetValueOrDefault(cageId) + row.SignedBiomassGram;
                    }
                }

                if (row.MovementType == BatchMovementType.Mortality)
                {
                    var dead = Math.Max(0, -row.SignedCount);
                    if (dead > 0)
                    {
                        mortalityByCage[cageId] = mortalityByCage.GetValueOrDefault(cageId) + dead;
                    }
                }
            }

            var latestBalanceByBatchAndCage = batchCageBalances
                .Where(x => reportCageIdSet.Contains(x.ProjectCageId))
                .GroupBy(x => $"{x.ProjectCageId}:{x.FishBatchId}")
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.AsOfDate).ThenByDescending(y => y.Id).First());

            var currentCountByCage = new Dictionary<long, int>();
            var currentBiomassByCage = new Dictionary<long, decimal>();
            foreach (var row in latestBalanceByBatchAndCage.Values)
            {
                currentCountByCage[row.ProjectCageId] = currentCountByCage.GetValueOrDefault(row.ProjectCageId) + row.LiveCount;
                currentBiomassByCage[row.ProjectCageId] = currentBiomassByCage.GetValueOrDefault(row.ProjectCageId) + row.BiomassGram;
            }

            var dailyDeadByCage = new Dictionary<long, Dictionary<string, int>>();
            var dailyDeadBiomassByCage = new Dictionary<long, Dictionary<string, decimal>>();
            foreach (var movement in batchMovements.Where(x => x.MovementType == BatchMovementType.Mortality))
            {
                if (!movement.ProjectCageId.HasValue || !reportCageIdSet.Contains(movement.ProjectCageId.Value)) continue;
                var cageId = movement.ProjectCageId.Value;
                var date = ToDateOnly(movement.MovementDate);
                var deadBiomassGram = Math.Max(0m, -movement.SignedBiomassGram);
                if (deadBiomassGram <= 0) continue;
                AddByDate(dailyDeadBiomassByCage, cageId, date, deadBiomassGram);
            }

            foreach (var row in mortalityLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
            {
                if (!mortalityIdToDate.TryGetValue(row.MortalityId, out var date)) continue;
                mortalityByCage[row.ProjectCageId] = mortalityByCage.GetValueOrDefault(row.ProjectCageId) + row.DeadCount;
                AddByDate(dailyDeadByCage, row.ProjectCageId, date, row.DeadCount);
            }

            var dailyFeedByCage = new Dictionary<long, Dictionary<string, decimal>>();
            var feedDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            var feedStocksByCageDate = new Dictionary<long, Dictionary<string, HashSet<long>>>();
            foreach (var row in feedingDistributions.Where(x => reportCageIdSet.Contains(x.ProjectCageId)))
            {
                if (!feedingLineIdToFeedingId.TryGetValue(row.FeedingLineId, out var feedingId)) continue;
                if (!feedingIdToDate.TryGetValue(feedingId, out var date)) continue;

                AddByDate(dailyFeedByCage, row.ProjectCageId, date, row.FeedGram);

                var feedingLine = feedingLineById.GetValueOrDefault(row.FeedingLineId);
                var feedingHeader = feedingById.GetValueOrDefault(feedingId);
                var stockId = feedingLine?.StockId;
                var stockText = stockId.HasValue ? stockLabelById.GetValueOrDefault(stockId.Value, stockId.Value.ToString()) : "?";
                var detailParts = new List<string?>
                {
                    feedingHeader?.FeedingNo ?? $"#{feedingId}",
                    feedingHeader != null ? $"slot:{feedingHeader.FeedingSlot}" : null,
                    $"stock:{stockText}",
                    $"feed:{row.FeedGram}g",
                    feedingHeader?.Note
                };
                AppendDetail(feedDetailsByCageDate, row.ProjectCageId, date, JoinDetail(detailParts));

                var stocksByDate = GetOrCreate(feedStocksByCageDate, row.ProjectCageId);
                var stockSet = stocksByDate.GetValueOrDefault(date) ?? new HashSet<long>();
                if (stockId.HasValue) stockSet.Add(stockId.Value);
                stocksByDate[date] = stockSet;
            }

            var weatherByDate = new Dictionary<string, string>();
            foreach (var row in dailyWeathers)
            {
                var date = ToDateOnly(row.WeatherDate);
                var parts = new List<string?>
                {
                    row.WeatherSeverity?.Name,
                    row.WeatherSeverity?.Score != null ? $"risk-base:{row.WeatherSeverity.Score}" : null,
                    row.WeatherType?.Name,
                    row.TemperatureC != null ? $"{row.TemperatureC}C" : null,
                    row.WindKnot != null ? $"{row.WindKnot}kt" : null
                };
                var detail = JoinDetail(parts);
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    weatherByDate[date] = detail;
                }
            }

            var netOpsByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var netOpDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            foreach (var row in netOperationLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId) && postedNetOperationIds.Contains(x.NetOperationId)))
            {
                if (!netOperationIdToDate.TryGetValue(row.NetOperationId, out var date)) continue;
                AddByDate(netOpsByCageDate, row.ProjectCageId, date, 1);
                var header = netOperationById.GetValueOrDefault(row.NetOperationId);
                AppendDetail(netOpDetailsByCageDate, row.ProjectCageId, date, JoinDetail(new List<string?>
                {
                    header?.OperationNo ?? $"#{row.NetOperationId}",
                    header?.OperationType?.Name,
                    row.Note ?? header?.Note
                }));
            }

            var transferByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var transferDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            foreach (var row in transferLines.Where(x => postedTransferIds.Contains(x.TransferId)))
            {
                if (!transferIdToDate.TryGetValue(row.TransferId, out var date)) continue;
                var header = transferById.GetValueOrDefault(row.TransferId);
                var fromLabel = cageLabelById.GetValueOrDefault(row.FromProjectCageId, row.FromProjectCageId.ToString());
                var toLabel = cageLabelById.GetValueOrDefault(row.ToProjectCageId, row.ToProjectCageId.ToString());
                var fishBatchText = fishBatchLabelById.GetValueOrDefault(row.FishBatchId, row.FishBatchId.ToString());
                var detail = JoinDetail(new List<string?>
                {
                    header?.TransferNo ?? $"#{row.TransferId}",
                    $"{fromLabel} -> {toLabel}",
                    $"batch:{fishBatchText}",
                    $"count:{row.FishCount}",
                    $"avg:{row.AverageGram}g",
                    $"biomass:{row.BiomassGram}g",
                    header?.Note
                });

                if (reportCageIdSet.Contains(row.FromProjectCageId))
                {
                    AddByDate(transferByCageDate, row.FromProjectCageId, date, row.FishCount);
                    AppendDetail(transferDetailsByCageDate, row.FromProjectCageId, date, detail);
                }
                if (reportCageIdSet.Contains(row.ToProjectCageId))
                {
                    AddByDate(transferByCageDate, row.ToProjectCageId, date, row.FishCount);
                    if (row.ToProjectCageId != row.FromProjectCageId)
                    {
                        AppendDetail(transferDetailsByCageDate, row.ToProjectCageId, date, detail);
                    }
                }
            }

            var weighingByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var weighingDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            foreach (var row in weighingLines.Where(x => reportCageIdSet.Contains(x.ProjectCageId) && postedWeighingIds.Contains(x.WeighingId)))
            {
                if (!weighingIdToDate.TryGetValue(row.WeighingId, out var date)) continue;
                AddByDate(weighingByCageDate, row.ProjectCageId, date, 1);
                var header = weighingById.GetValueOrDefault(row.WeighingId);
                AppendDetail(weighingDetailsByCageDate, row.ProjectCageId, date, JoinDetail(new List<string?>
                {
                    header?.WeighingNo ?? $"#{row.WeighingId}",
                    $"count:{row.MeasuredCount}",
                    $"avg:{row.MeasuredAverageGram}g",
                    $"biomass:{row.MeasuredBiomassGram}g",
                    header?.Note
                }));
            }

            var shipmentByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var shipmentDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            var shipmentFishByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var shipmentBiomassByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
            foreach (var row in shipmentLines.Where(x => reportCageIdSet.Contains(x.FromProjectCageId) && postedShipmentIds.Contains(x.ShipmentId)))
            {
                if (!shipmentIdToDate.TryGetValue(row.ShipmentId, out var date)) continue;
                var header = shipmentById.GetValueOrDefault(row.ShipmentId);
                var fromLabel = cageLabelById.GetValueOrDefault(row.FromProjectCageId, row.FromProjectCageId.ToString());
                var targetWarehouseLabel = header?.TargetWarehouseId.HasValue == true
                    ? warehouseLabelById.GetValueOrDefault(header.TargetWarehouseId.Value, header.TargetWarehouseId.Value.ToString())
                    : "ColdStorage";
                var detail = JoinDetail(new List<string?>
                {
                    header?.ShipmentNo ?? $"#{row.ShipmentId}",
                    $"{fromLabel} -> {targetWarehouseLabel}",
                    row.FishCount > 0 ? $"count:{row.FishCount}" : null,
                    $"avg:{row.AverageGram}g",
                    row.BiomassGram > 0 ? $"biomass:{row.BiomassGram}g" : null,
                    header?.Note
                });
                AddByDate(shipmentByCageDate, row.FromProjectCageId, date, 1);
                AddByDate(shipmentFishByCageDate, row.FromProjectCageId, date, row.FishCount);
                AddByDate(shipmentBiomassByCageDate, row.FromProjectCageId, date, row.BiomassGram);
                AppendDetail(shipmentDetailsByCageDate, row.FromProjectCageId, date, detail);
            }

            var convertByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var convertDetailsByCageDate = new Dictionary<long, Dictionary<string, List<string>>>();
            foreach (var row in stockConvertLines.Where(x => postedStockConvertIds.Contains(x.StockConvertId)))
            {
                if (!stockConvertIdToDate.TryGetValue(row.StockConvertId, out var date)) continue;
                var header = stockConvertById.GetValueOrDefault(row.StockConvertId);
                var fromLabel = cageLabelById.GetValueOrDefault(row.FromProjectCageId, row.FromProjectCageId.ToString());
                var toLabel = cageLabelById.GetValueOrDefault(row.ToProjectCageId, row.ToProjectCageId.ToString());
                var movementCandidates = stockConvertMovementsByRefId.GetValueOrDefault(row.StockConvertId) ?? new List<BatchMovement>();
                var matchedMovement = movementCandidates.FirstOrDefault(x =>
                        x.FromProjectCageId == row.FromProjectCageId &&
                        x.ToProjectCageId == row.ToProjectCageId &&
                        x.SignedCount < 0)
                    ?? movementCandidates.FirstOrDefault(x =>
                        x.FromProjectCageId == row.FromProjectCageId &&
                        x.ToProjectCageId == row.ToProjectCageId)
                    ?? movementCandidates.FirstOrDefault();
                var fromAverageGram = row.AverageGram != 0 ? row.AverageGram : matchedMovement?.FromAverageGram ?? 0;
                var gramIncrease = row.NewAverageGram;
                var toAverageGram = row.NewAverageGram != 0
                    ? CalculateIncrementedAverageGram(fromAverageGram, gramIncrease)
                    : matchedMovement?.ToAverageGram ?? fromAverageGram;
                var fromBiomass = row.BiomassGram != 0 ? row.BiomassGram : row.FishCount * fromAverageGram;
                var toBiomass = row.FishCount > 0 ? CalculateBiomassGram(row.FishCount, toAverageGram) : 0;
                var biomassIncrease = RoundGram(toBiomass - fromBiomass);
                var detail = JoinDetail(new List<string?>
                {
                    header?.ConvertNo ?? $"#{row.StockConvertId}",
                    $"{fromLabel} -> {toLabel}",
                    FormatStockTransition(matchedMovement?.FromStockId, matchedMovement?.ToStockId, stockLabelById),
                    $"count:{row.FishCount}",
                    $"avg:{fromAverageGram}g + {gramIncrease}g = {toAverageGram}g",
                    $"biomass:{fromBiomass}g -> {toBiomass}g",
                    $"increase:{biomassIncrease}g",
                    header?.Note
                });

                if (reportCageIdSet.Contains(row.FromProjectCageId))
                {
                    AddByDate(convertByCageDate, row.FromProjectCageId, date, 1);
                    AppendDetail(convertDetailsByCageDate, row.FromProjectCageId, date, detail);
                }
                if (reportCageIdSet.Contains(row.ToProjectCageId))
                {
                    AddByDate(convertByCageDate, row.ToProjectCageId, date, 1);
                    if (row.ToProjectCageId != row.FromProjectCageId)
                    {
                        AppendDetail(convertDetailsByCageDate, row.ToProjectCageId, date, detail);
                    }
                }
            }

            var movementCountDeltaByCageDate = new Dictionary<long, Dictionary<string, int>>();
            var movementBiomassDeltaByCageDate = new Dictionary<long, Dictionary<string, decimal>>();
            foreach (var row in batchMovements)
            {
                if (!row.ProjectCageId.HasValue || !reportCageIdSet.Contains(row.ProjectCageId.Value)) continue;
                var cageId = row.ProjectCageId.Value;
                var date = ToDateOnly(row.MovementDate);
                AddByDate(movementCountDeltaByCageDate, cageId, date, row.SignedCount);
                AddByDate(movementBiomassDeltaByCageDate, cageId, date, row.SignedBiomassGram);
            }

            var startDate = project.StartDate.Date;
            var endDate = DateTimeProvider.Now.Date;
            var allDates = EnumerateDates(startDate, endDate);

            var cages = reportProjectCages.Select(projectCage =>
            {
                var cageId = projectCage.Id;
                var feedByDate = dailyFeedByCage.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var feedDetailsByDate = feedDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();
                var feedStocksByDate = feedStocksByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, HashSet<long>>();
                var deadByDate = dailyDeadByCage.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var deadBiomassByDate = dailyDeadBiomassByCage.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var countDeltaByDate = movementCountDeltaByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var biomassDeltaByDate = movementBiomassDeltaByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var netOpByDate = netOpsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var netOpDetailsByDate = netOpDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();
                var transferByDate = transferByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var transferDetailsByDate = transferDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();
                var shipmentByDate = shipmentByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var shipmentDetailsByDate = shipmentDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();
                var shipmentFishByDate = shipmentFishByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var shipmentBiomassByDate = shipmentBiomassByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, decimal>();
                var weighingByDate = weighingByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var weighingDetailsByDate = weighingDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();
                var convertByDate = convertByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, int>();
                var convertDetailsByDate = convertDetailsByCageDate.GetValueOrDefault(cageId) ?? new Dictionary<string, List<string>>();

                var activityDates = new HashSet<string>(
                    feedByDate.Keys
                        .Concat(deadByDate.Keys)
                        .Concat(countDeltaByDate.Keys)
                        .Concat(biomassDeltaByDate.Keys)
                        .Concat(netOpByDate.Keys)
                        .Concat(transferByDate.Keys)
                        .Concat(shipmentByDate.Keys)
                        .Concat(weighingByDate.Keys)
                        .Concat(convertByDate.Keys)
                        .Concat(weatherByDate.Keys));

                var dailyRows = activityDates
                    .Select(date => new CageDailyRow
                    {
                        Date = date,
                        FeedGram = feedByDate.GetValueOrDefault(date),
                        FeedStockCount = feedStocksByDate.GetValueOrDefault(date)?.Count ?? 0,
                        FeedDetails = feedDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        DeadCount = deadByDate.GetValueOrDefault(date),
                        DeadBiomassGram = deadBiomassByDate.GetValueOrDefault(date),
                        CountDelta = countDeltaByDate.GetValueOrDefault(date),
                        BiomassDelta = biomassDeltaByDate.GetValueOrDefault(date),
                        Weather = weatherByDate.GetValueOrDefault(date, "-"),
                        NetOperationCount = netOpByDate.GetValueOrDefault(date),
                        NetOperationDetails = netOpDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        TransferCount = transferByDate.GetValueOrDefault(date),
                        TransferDetails = transferDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        ShipmentCount = shipmentByDate.GetValueOrDefault(date),
                        ShipmentDetails = shipmentDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        ShipmentFishCount = shipmentFishByDate.GetValueOrDefault(date),
                        ShipmentBiomassGram = shipmentBiomassByDate.GetValueOrDefault(date),
                        WeighingCount = weighingByDate.GetValueOrDefault(date),
                        WeighingDetails = weighingDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        StockConvertCount = convertByDate.GetValueOrDefault(date),
                        StockConvertDetails = convertDetailsByDate.GetValueOrDefault(date) ?? new List<string>(),
                        Fed = feedByDate.GetValueOrDefault(date) > 0
                    })
                    .OrderByDescending(x => x.Date)
                    .ToList();

                var missingFeedingDays = allDates.Where(date => feedByDate.GetValueOrDefault(date) <= 0).ToList();
                var initialFish = initialByCage.GetValueOrDefault(cageId);
                var initialBiomass = initialBiomassByCage.GetValueOrDefault(cageId);
                var initialAvgGram = initialFish > 0 ? initialBiomass / initialFish : 0m;
                var totalCountDelta = countDeltaByDate.Values.Sum();
                var totalBiomassDelta = biomassDeltaByDate.Values.Sum();
                var totalDead = mortalityByCage.GetValueOrDefault(cageId);
                var currentFishFromBalance = currentCountByCage.GetValueOrDefault(cageId);
                var currentBiomassFromBalance = currentBiomassByCage.GetValueOrDefault(cageId);
                var fallbackCurrentFish = Math.Max(0, initialFish - totalDead);
                var fallbackCurrentBiomass = Math.Max(0m, initialBiomass - (totalDead * initialAvgGram));
                var currentFishFromMovement = Math.Max(0, totalCountDelta);
                var currentBiomassFromMovement = Math.Max(0m, totalBiomassDelta);
                var hasMovementSnapshot = countDeltaByDate.Count > 0 || biomassDeltaByDate.Count > 0;
                var currentFish = hasMovementSnapshot ? currentFishFromMovement : (currentFishFromBalance > 0 ? currentFishFromBalance : fallbackCurrentFish);
                var currentBiomass = hasMovementSnapshot ? currentBiomassFromMovement : (currentBiomassFromBalance > 0 ? currentBiomassFromBalance : fallbackCurrentBiomass);
                var currentAvgGram = currentFish > 0 ? currentBiomass / currentFish : 0m;

                return new CageProjectReport
                {
                    ProjectCageId = cageId,
                    CageLabel = projectCage.Cage?.CageCode ?? projectCage.Cage?.CageName ?? cageId.ToString(),
                    InitialFishCount = initialFish,
                    InitialAverageGram = RoundGram(initialAvgGram),
                    InitialBiomassGram = RoundGram(initialBiomass),
                    CurrentFishCount = currentFish,
                    CurrentAverageGram = RoundGram(currentAvgGram),
                    CurrentBiomassGram = RoundGram(Math.Max(0m, currentBiomass)),
                    TotalDeadCount = totalDead,
                    TotalFeedGram = feedByDate.Values.Sum(),
                    TotalCountDelta = totalCountDelta,
                    TotalBiomassDelta = RoundGram(totalBiomassDelta),
                    MissingFeedingDays = missingFeedingDays,
                    DailyRows = dailyRows
                };
            })
            .ToList();

            var warehouseFishCount = batchWarehouseBalances.Sum(x => x.LiveCount);
            var warehouseBiomassGram = RoundGram(batchWarehouseBalances.Sum(x => x.BiomassGram));
            var cageFishCount = cages.Sum(x => x.CurrentFishCount);
            var cageBiomassGram = cages.Sum(x => x.CurrentBiomassGram);

            return new ProjectDashboardReport
            {
                Project = project,
                Cages = cages,
                WarehouseSummary = new WarehouseSummary
                {
                    ActiveWarehouseCount = batchWarehouseBalances
                        .Where(x => x.LiveCount > 0 || x.BiomassGram > 0)
                        .Select(x => x.WarehouseId)
                        .Distinct()
                        .Count(),
                    WarehouseFishCount = warehouseFishCount,
                    WarehouseBiomassGram = warehouseBiomassGram,
                    TotalSystemFishCount = cageFishCount + warehouseFishCount,
                    TotalSystemBiomassGram = RoundGram(cageBiomassGram + warehouseBiomassGram)
                }
            };
        }

        private static DashboardProjectSummaryDto MapProjectSummary(ProjectDashboardReport report)
        {
            var cages = report.Cages
                .Select(MapCageSummary)
                .OrderBy(x => x.CageLabel)
                .ToList();

            var cageFish = cages.Sum(x => x.CurrentFishCount);
            var totalShipmentCount = cages.Sum(x => x.TotalShipmentCount);
            var totalShipmentBiomassGram = cages.Sum(x => x.TotalShipmentBiomassGram);
            var totalDeadCount = cages.Sum(x => x.TotalDeadCount);
            var totalDeadBiomassGram = cages.Sum(x => x.TotalDeadBiomassGram);
            var totalFeedGram = cages.Sum(x => x.TotalFeedGram);
            var cageBiomassGram = cages.Sum(x => x.CurrentBiomassGram);
            var initialBiomassGram = cages.Sum(x => x.InitialBiomassGram);

            return new DashboardProjectSummaryDto
            {
                ProjectId = report.Project.Id,
                ProjectCode = string.IsNullOrWhiteSpace(report.Project.ProjectCode) ? "-" : report.Project.ProjectCode,
                ProjectName = string.IsNullOrWhiteSpace(report.Project.ProjectName) ? "-" : report.Project.ProjectName,
                MeasurementAverageGram = WeightedAverageGram(cageBiomassGram, cageFish),
                CageFish = cageFish,
                TotalShipmentCount = totalShipmentCount,
                TotalShipmentBiomassGram = RoundGram(totalShipmentBiomassGram),
                WarehouseFish = report.WarehouseSummary.WarehouseFishCount,
                TotalSystemFish = report.WarehouseSummary.TotalSystemFishCount,
                TotalDeadCount = totalDeadCount,
                TotalDeadBiomassGram = RoundGram(totalDeadBiomassGram),
                ActiveCageCount = cages.Count,
                Fcr = ComputeFcr(totalFeedGram, cageBiomassGram, initialBiomassGram, totalDeadBiomassGram, totalShipmentBiomassGram),
                CageBiomassGram = RoundGram(cageBiomassGram),
                WarehouseBiomassGram = report.WarehouseSummary.WarehouseBiomassGram,
                TotalSystemBiomassGram = report.WarehouseSummary.TotalSystemBiomassGram,
                Cages = cages
            };
        }

        private static DashboardCageSummaryDto MapCageSummary(CageProjectReport cage)
        {
            var totalShipmentCount = cage.DailyRows.Sum(x => x.ShipmentFishCount);
            var totalShipmentBiomassGram = cage.DailyRows.Sum(x => x.ShipmentBiomassGram);
            var totalDeadBiomassGram = cage.DailyRows.Sum(x => x.DeadBiomassGram);

            return new DashboardCageSummaryDto
            {
                ProjectCageId = cage.ProjectCageId,
                CageLabel = cage.CageLabel,
                MeasurementAverageGram = cage.CurrentAverageGram,
                InitialFishCount = cage.InitialFishCount,
                InitialBiomassGram = RoundGram(cage.InitialBiomassGram),
                CurrentFishCount = cage.CurrentFishCount,
                TotalShipmentCount = totalShipmentCount,
                TotalShipmentBiomassGram = RoundGram(totalShipmentBiomassGram),
                TotalDeadCount = cage.TotalDeadCount,
                TotalDeadBiomassGram = RoundGram(totalDeadBiomassGram),
                TotalFeedGram = RoundGram(cage.TotalFeedGram),
                CurrentBiomassGram = RoundGram(cage.CurrentBiomassGram),
                Fcr = ComputeFcr(cage.TotalFeedGram, cage.CurrentBiomassGram, cage.InitialBiomassGram, totalDeadBiomassGram, totalShipmentBiomassGram)
            };
        }

        private static DashboardProjectDetailCageDto MapProjectDetailCage(CageProjectReport cage)
        {
            return new DashboardProjectDetailCageDto
            {
                ProjectCageId = cage.ProjectCageId,
                CageLabel = cage.CageLabel,
                InitialFishCount = cage.InitialFishCount,
                InitialAverageGram = RoundGram(cage.InitialAverageGram),
                InitialBiomassGram = RoundGram(cage.InitialBiomassGram),
                CurrentFishCount = cage.CurrentFishCount,
                CurrentAverageGram = RoundGram(cage.CurrentAverageGram),
                CurrentBiomassGram = RoundGram(cage.CurrentBiomassGram),
                TotalDeadCount = cage.TotalDeadCount,
                TotalFeedGram = RoundGram(cage.TotalFeedGram),
                TotalCountDelta = cage.TotalCountDelta,
                TotalBiomassDelta = RoundGram(cage.TotalBiomassDelta),
                MissingFeedingDays = cage.MissingFeedingDays,
                DailyRows = cage.DailyRows.Select(row => new DashboardCageDailyRowDto
                {
                    Date = row.Date,
                    FeedGram = RoundGram(row.FeedGram),
                    FeedStockCount = row.FeedStockCount,
                    FeedDetails = row.FeedDetails,
                    DeadCount = row.DeadCount,
                    DeadBiomassGram = RoundGram(row.DeadBiomassGram),
                    CountDelta = row.CountDelta,
                    BiomassDelta = RoundGram(row.BiomassDelta),
                    Weather = row.Weather,
                    NetOperationCount = row.NetOperationCount,
                    NetOperationDetails = row.NetOperationDetails,
                    TransferCount = row.TransferCount,
                    TransferDetails = row.TransferDetails,
                    WeighingCount = row.WeighingCount,
                    WeighingDetails = row.WeighingDetails,
                    StockConvertCount = row.StockConvertCount,
                    StockConvertDetails = row.StockConvertDetails,
                    ShipmentCount = row.ShipmentCount,
                    ShipmentDetails = row.ShipmentDetails,
                    ShipmentFishCount = row.ShipmentFishCount,
                    ShipmentBiomassGram = RoundGram(row.ShipmentBiomassGram),
                    Fed = row.Fed
                }).ToList()
            };
        }

        private static bool HasDailyEntry(CageDailyRow row)
        {
            return row.FeedGram > 0
                || row.FeedStockCount > 0
                || row.FeedDetails.Count > 0
                || row.DeadCount > 0
                || row.CountDelta != 0
                || row.BiomassDelta != 0
                || !string.IsNullOrWhiteSpace(row.Weather)
                || row.Weather != "-"
                || row.NetOperationCount > 0
                || row.TransferCount > 0
                || row.WeighingCount > 0
                || row.StockConvertCount > 0
                || row.ShipmentCount > 0
                || row.ShipmentFishCount > 0
                || row.ShipmentBiomassGram > 0;
        }

        private static bool IsActiveProjectCage(DateTime? releasedDate)
        {
            return !releasedDate.HasValue || releasedDate.Value.Year <= LegacyOpenEndedYearThreshold;
        }

        private static string ToDateOnly(DateTime value) => value.ToString("yyyy-MM-dd");

        private static List<string> EnumerateDates(DateTime startDate, DateTime endDate)
        {
            var result = new List<string>();
            var cursor = startDate.Date;
            var end = endDate.Date;
            while (cursor <= end)
            {
                result.Add(ToDateOnly(cursor));
                cursor = cursor.AddDays(1);
            }
            return result;
        }

        private static decimal WeightedAverageGram(decimal totalBiomassGram, int fishCount)
        {
            if (fishCount <= 0) return 0m;
            return RoundGram(totalBiomassGram / fishCount);
        }

        private static decimal? ComputeFcr(decimal totalFeedGram, decimal currentBiomassGram, decimal initialBiomassGram, decimal totalDeadBiomassGram, decimal totalShipmentBiomassGram)
        {
            var producedBiomassKg = Math.Max(0m, (currentBiomassGram + totalDeadBiomassGram + totalShipmentBiomassGram - initialBiomassGram) / 1000m);
            if (producedBiomassKg <= 0) return null;
            return decimal.Round((totalFeedGram / 1000m) / producedBiomassKg, 3);
        }

        private static decimal CalculateIncrementedAverageGram(decimal fromAverageGram, decimal incrementGram)
        {
            return RoundGram(fromAverageGram + incrementGram);
        }

        private static decimal CalculateBiomassGram(int fishCount, decimal averageGram)
        {
            return RoundGram(fishCount * averageGram);
        }

        private static decimal RoundGram(decimal value) => decimal.Round(value, 3, MidpointRounding.AwayFromZero);

        private static string? FormatStockTransition(long? fromStockId, long? toStockId, Dictionary<long, string> stockLabelById)
        {
            if (!fromStockId.HasValue && !toStockId.HasValue) return null;
            var fromText = fromStockId.HasValue ? stockLabelById.GetValueOrDefault(fromStockId.Value, fromStockId.Value.ToString()) : "?";
            var toText = toStockId.HasValue ? stockLabelById.GetValueOrDefault(toStockId.Value, toStockId.Value.ToString()) : "?";
            return $"stock:{fromText} -> {toText}";
        }

        private static string JoinDetail(IEnumerable<string?> parts)
        {
            return string.Join(" | ", parts.Where(x => !string.IsNullOrWhiteSpace(x))!);
        }

        private static Dictionary<string, TValue> GetOrCreate<TValue>(Dictionary<long, Dictionary<string, TValue>> root, long cageId)
        {
            if (!root.TryGetValue(cageId, out var byDate))
            {
                byDate = new Dictionary<string, TValue>();
                root[cageId] = byDate;
            }
            return byDate;
        }

        private static void AppendDetail(Dictionary<long, Dictionary<string, List<string>>> target, long cageId, string date, string detail)
        {
            if (string.IsNullOrWhiteSpace(detail)) return;
            var byDate = GetOrCreate(target, cageId);
            if (!byDate.TryGetValue(date, out var list))
            {
                list = new List<string>();
                byDate[date] = list;
            }
            list.Add(detail);
        }

        private static void AddByDate(Dictionary<long, Dictionary<string, int>> target, long cageId, string date, int value)
        {
            var byDate = GetOrCreate(target, cageId);
            byDate[date] = byDate.GetValueOrDefault(date) + value;
        }

        private static void AddByDate(Dictionary<long, Dictionary<string, decimal>> target, long cageId, string date, decimal value)
        {
            var byDate = GetOrCreate(target, cageId);
            byDate[date] = byDate.GetValueOrDefault(date) + value;
        }

        private sealed class ProjectDashboardReport
        {
            public required Project Project { get; init; }
            public required List<CageProjectReport> Cages { get; init; }
            public required WarehouseSummary WarehouseSummary { get; init; }
        }

        private sealed class WarehouseSummary
        {
            public int ActiveWarehouseCount { get; init; }
            public int WarehouseFishCount { get; init; }
            public decimal WarehouseBiomassGram { get; init; }
            public int TotalSystemFishCount { get; init; }
            public decimal TotalSystemBiomassGram { get; init; }
        }

        private sealed class CageProjectReport
        {
            public long ProjectCageId { get; init; }
            public string CageLabel { get; init; } = string.Empty;
            public int InitialFishCount { get; init; }
            public decimal InitialAverageGram { get; init; }
            public decimal InitialBiomassGram { get; init; }
            public int CurrentFishCount { get; init; }
            public decimal CurrentAverageGram { get; init; }
            public decimal CurrentBiomassGram { get; init; }
            public int TotalDeadCount { get; init; }
            public decimal TotalFeedGram { get; init; }
            public int TotalCountDelta { get; init; }
            public decimal TotalBiomassDelta { get; init; }
            public List<string> MissingFeedingDays { get; init; } = new();
            public List<CageDailyRow> DailyRows { get; init; } = new();
        }

        private sealed class CageDailyRow
        {
            public string Date { get; init; } = string.Empty;
            public decimal FeedGram { get; init; }
            public int FeedStockCount { get; init; }
            public List<string> FeedDetails { get; init; } = new();
            public int DeadCount { get; init; }
            public decimal DeadBiomassGram { get; init; }
            public int CountDelta { get; init; }
            public decimal BiomassDelta { get; init; }
            public string Weather { get; init; } = "-";
            public int NetOperationCount { get; init; }
            public List<string> NetOperationDetails { get; init; } = new();
            public int TransferCount { get; init; }
            public List<string> TransferDetails { get; init; } = new();
            public int WeighingCount { get; init; }
            public List<string> WeighingDetails { get; init; } = new();
            public int StockConvertCount { get; init; }
            public List<string> StockConvertDetails { get; init; } = new();
            public int ShipmentCount { get; init; }
            public List<string> ShipmentDetails { get; init; } = new();
            public int ShipmentFishCount { get; init; }
            public decimal ShipmentBiomassGram { get; init; }
            public bool Fed { get; init; }
        }
    }
}
