using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class BalanceLedgerManager : IBalanceLedgerManager
    {
        private readonly IUnitOfWork _uow;
        private readonly ILocalizationService _localizationService;

        public BalanceLedgerManager(IUnitOfWork uow, ILocalizationService localizationService)
        {
            _uow = uow;
            _localizationService = localizationService;
        }

        public async Task<BatchMassSnapshot> GetCageMassSnapshotAsync(
            long fishBatchId,
            long projectCageId,
            DateTime movementDate,
            BatchMovementType movementType)
        {
            var balance = _uow.Db.BatchCageBalances.Local.FirstOrDefault(x =>
                x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && !x.IsDeleted);
            balance ??= await _uow.Db.BatchCageBalances
                .FirstOrDefaultAsync(x =>
                    x.FishBatchId == fishBatchId
                    && x.ProjectCageId == projectCageId
                    && !x.IsDeleted);

            if (balance != null && movementDate.Date >= balance.AsOfDate.Date)
            {
                return CreateSnapshot(balance.LiveCount, balance.BiomassGram);
            }

            var movements = await _uow.Db.BatchMovements
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted
                    && x.FishBatchId == fishBatchId
                    && x.ProjectCageId == projectCageId
                    && x.MovementDate < movementDate.Date.AddDays(1))
                .ToListAsync();

            return CreateHistoricalSnapshot(
                movements,
                movementDate,
                movementType,
                x => x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId);
        }

        public async Task<BatchMassSnapshot> GetWarehouseMassSnapshotAsync(
            long fishBatchId,
            long warehouseId,
            DateTime movementDate,
            BatchMovementType movementType)
        {
            var balance = _uow.Db.BatchWarehouseBalances.Local.FirstOrDefault(x =>
                x.FishBatchId == fishBatchId
                && x.WarehouseId == warehouseId
                && !x.IsDeleted);
            balance ??= await _uow.Db.BatchWarehouseBalances
                .FirstOrDefaultAsync(x =>
                    x.FishBatchId == fishBatchId
                    && x.WarehouseId == warehouseId
                    && !x.IsDeleted);

            if (balance != null && movementDate.Date >= balance.AsOfDate.Date)
            {
                return CreateSnapshot(balance.LiveCount, balance.BiomassGram);
            }

            var movements = await _uow.Db.BatchMovements
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted
                    && x.FishBatchId == fishBatchId
                    && x.WarehouseId == warehouseId
                    && x.MovementDate < movementDate.Date.AddDays(1))
                .ToListAsync();

            return CreateHistoricalSnapshot(
                movements,
                movementDate,
                movementType,
                x => x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId);
        }

        public async Task ApplyDelta(
            long projectId,
            long fishBatchId,
            long projectCageId,
            int deltaCount,
            decimal? deltaBiomassGram,
            BatchMovementType movementType,
            DateTime movementDate,
            string description,
            string refTable,
            long refId,
            long? fromCageId,
            long? toCageId,
            long? fromStockId,
            long? toStockId,
            decimal? fromAvgGram,
            decimal? toAvgGram,
            long? actorUserId = null)
        {
            var fishBatch = await _uow.Db.FishBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fishBatchId && !x.IsDeleted);

            var resolvedFromStockId = fromStockId ?? fishBatch?.FishStockId;
            var resolvedToStockId = toStockId ?? resolvedFromStockId;

            var balance = _uow.Db.BatchCageBalances.Local.FirstOrDefault(x =>
                x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId && !x.IsDeleted);
            balance ??= await _uow.Db.BatchCageBalances
                .FirstOrDefaultAsync(x => x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId && !x.IsDeleted);

            if (balance == null)
            {
                balance = new BatchCageBalance
                {
                    FishBatchId = fishBatchId,
                    ProjectCageId = projectCageId,
                    LiveCount = 0,
                    AverageGram = 0,
                    BiomassGram = 0,
                    AsOfDate = movementDate,
                    IsDeleted = false
                };

                await _uow.Db.BatchCageBalances.AddAsync(balance);
            }

            var biomassDelta = deltaBiomassGram ?? 0m;
            var nextCount = balance.LiveCount + deltaCount;
            var nextBiomass = balance.BiomassGram + biomassDelta;

            if (nextCount < 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageCountCannotGoNegative"));
            }

            if (nextBiomass < 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageBiomassCannotGoNegative"));
            }

            balance.LiveCount = nextCount;
            balance.BiomassGram = nextBiomass;
            balance.AverageGram = nextCount > 0
                ? Math.Round(nextBiomass / nextCount, 3, MidpointRounding.AwayFromZero)
                : 0m;
            balance.AsOfDate = balance.AsOfDate > movementDate ? balance.AsOfDate : movementDate;

            var noteParts = new List<string>
            {
                description,
                $"projectId={projectId}",
                $"fromCage={fromCageId?.ToString() ?? "null"}",
                $"toCage={toCageId?.ToString() ?? "null"}",
                $"fromStock={resolvedFromStockId?.ToString() ?? "null"}",
                $"toStock={resolvedToStockId?.ToString() ?? "null"}",
                $"fromAvg={fromAvgGram?.ToString("0.###") ?? "null"}",
                $"toAvg={toAvgGram?.ToString("0.###") ?? "null"}"
            };

            await _uow.Db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                FromProjectCageId = fromCageId,
                ToProjectCageId = toCageId,
                FromStockId = resolvedFromStockId,
                ToStockId = resolvedToStockId,
                FromAverageGram = fromAvgGram,
                ToAverageGram = toAvgGram,
                MovementDate = movementDate,
                MovementType = movementType,
                SignedCount = deltaCount,
                SignedBiomassGram = biomassDelta,
                FeedGram = null,
                ActorUserId = actorUserId,
                ReferenceTable = refTable,
                ReferenceId = refId,
                Note = string.Join(" | ", noteParts),
                CreatedBy = actorUserId,
                IsDeleted = false
            });
        }

        public async Task ApplyWarehouseDelta(
            long projectId,
            long fishBatchId,
            long warehouseId,
            int deltaCount,
            decimal? deltaBiomassGram,
            BatchMovementType movementType,
            DateTime movementDate,
            string description,
            string refTable,
            long refId,
            long? fromWarehouseId,
            long? toWarehouseId,
            long? fromStockId,
            long? toStockId,
            decimal? fromAvgGram,
            decimal? toAvgGram,
            long? actorUserId = null)
        {
            var fishBatch = await _uow.Db.FishBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fishBatchId && !x.IsDeleted);

            var resolvedFromStockId = fromStockId ?? fishBatch?.FishStockId;
            var resolvedToStockId = toStockId ?? resolvedFromStockId;

            var balance = _uow.Db.BatchWarehouseBalances.Local.FirstOrDefault(x =>
                x.ProjectId == projectId && x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId && !x.IsDeleted);
            balance ??= await _uow.Db.BatchWarehouseBalances
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FishBatchId == fishBatchId && x.WarehouseId == warehouseId && !x.IsDeleted);

            if (balance == null)
            {
                balance = new BatchWarehouseBalance
                {
                    ProjectId = projectId,
                    FishBatchId = fishBatchId,
                    WarehouseId = warehouseId,
                    LiveCount = 0,
                    AverageGram = 0,
                    BiomassGram = 0,
                    AsOfDate = movementDate,
                    IsDeleted = false
                };

                await _uow.Db.BatchWarehouseBalances.AddAsync(balance);
            }

            var biomassDelta = deltaBiomassGram ?? 0m;
            var nextCount = balance.LiveCount + deltaCount;
            var nextBiomass = balance.BiomassGram + biomassDelta;

            if (nextCount < 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("BalanceLedgerManager.BatchWarehouseCountCannotGoNegative"));
            }

            if (nextBiomass < 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("BalanceLedgerManager.BatchWarehouseBiomassCannotGoNegative"));
            }

            balance.LiveCount = nextCount;
            balance.BiomassGram = nextBiomass;
            balance.AverageGram = nextCount > 0
                ? Math.Round(nextBiomass / nextCount, 3, MidpointRounding.AwayFromZero)
                : 0m;
            balance.AsOfDate = balance.AsOfDate > movementDate ? balance.AsOfDate : movementDate;

            var noteParts = new List<string>
            {
                description,
                $"projectId={projectId}",
                $"warehouse={warehouseId}",
                $"fromWarehouse={fromWarehouseId?.ToString() ?? "null"}",
                $"toWarehouse={toWarehouseId?.ToString() ?? "null"}",
                $"fromStock={resolvedFromStockId?.ToString() ?? "null"}",
                $"toStock={resolvedToStockId?.ToString() ?? "null"}",
                $"fromAvg={fromAvgGram?.ToString("0.###") ?? "null"}",
                $"toAvg={toAvgGram?.ToString("0.###") ?? "null"}"
            };

            await _uow.Db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = fishBatchId,
                WarehouseId = warehouseId,
                FromWarehouseId = fromWarehouseId,
                ToWarehouseId = toWarehouseId,
                FromStockId = resolvedFromStockId,
                ToStockId = resolvedToStockId,
                FromAverageGram = fromAvgGram,
                ToAverageGram = toAvgGram,
                MovementDate = movementDate,
                MovementType = movementType,
                SignedCount = deltaCount,
                SignedBiomassGram = biomassDelta,
                FeedGram = null,
                ActorUserId = actorUserId,
                ReferenceTable = refTable,
                ReferenceId = refId,
                Note = string.Join(" | ", noteParts),
                CreatedBy = actorUserId,
                IsDeleted = false
            });
        }

        private BatchMassSnapshot CreateHistoricalSnapshot(
            List<BatchMovement> persistedMovements,
            DateTime movementDate,
            BatchMovementType movementType,
            Func<BatchMovement, bool> locationPredicate)
        {
            var persistedById = persistedMovements
                .Where(x => x.Id > 0)
                .ToDictionary(x => x.Id);
            var pendingMovements = new List<BatchMovement>();

            foreach (var entry in _uow.Db.ChangeTracker.Entries<BatchMovement>())
            {
                var movement = entry.Entity;
                if (!locationPredicate(movement) || movement.MovementDate >= movementDate.Date.AddDays(1))
                {
                    continue;
                }

                if (movement.Id > 0)
                {
                    persistedById.Remove(movement.Id);
                }

                if (entry.State != EntityState.Deleted && !movement.IsDeleted)
                {
                    pendingMovements.Add(movement);
                }
            }

            var includedMovements = persistedById.Values
                .Concat(pendingMovements)
                .Where(x => IsIncludedAtOperation(x, movementDate, movementType))
                .ToList();
            var liveCount = includedMovements.Sum(x => (long)x.SignedCount);
            var biomassGram = includedMovements.Sum(x => x.SignedBiomassGram);

            if (liveCount < 0)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageCountCannotGoNegative"));
            }

            if (biomassGram < 0m)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageBiomassCannotGoNegative"));
            }

            return CreateSnapshot(checked((int)liveCount), biomassGram);
        }

        private static BatchMassSnapshot CreateSnapshot(int liveCount, decimal biomassGram)
        {
            var roundedBiomass = Math.Round(biomassGram, 3, MidpointRounding.AwayFromZero);
            var averageGram = liveCount > 0
                ? Math.Round(roundedBiomass / liveCount, 3, MidpointRounding.AwayFromZero)
                : 0m;
            return new BatchMassSnapshot(liveCount, roundedBiomass, averageGram);
        }

        private static bool IsIncludedAtOperation(
            BatchMovement movement,
            DateTime operationDate,
            BatchMovementType operationType)
        {
            if (movement.MovementDate.Date < operationDate.Date)
            {
                return true;
            }

            if (movement.MovementDate.Date > operationDate.Date)
            {
                return false;
            }

            if (movement.MovementDate.TimeOfDay < operationDate.TimeOfDay)
            {
                return true;
            }

            if (movement.MovementDate.TimeOfDay > operationDate.TimeOfDay)
            {
                return false;
            }

            return GetOperationPriority(movement.MovementType) <= GetOperationPriority(operationType);
        }

        private static int GetOperationPriority(BatchMovementType movementType) => movementType switch
        {
            BatchMovementType.Stocking or BatchMovementType.OpeningImport => 0,
            BatchMovementType.Adjustment => 10,
            BatchMovementType.FishGrowth => 20,
            BatchMovementType.Weighing => 30,
            BatchMovementType.Transfer or BatchMovementType.WarehouseTransfer or BatchMovementType.StockConvert => 40,
            BatchMovementType.Shipment => 50,
            BatchMovementType.Mortality => 60,
            BatchMovementType.Feeding => 70,
            _ => 40
        };
    }
}
