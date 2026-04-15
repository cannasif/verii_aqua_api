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

            var balance = await _uow.Db.BatchCageBalances
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
            balance.AsOfDate = movementDate;

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

            var balance = await _uow.Db.BatchWarehouseBalances
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
            balance.AsOfDate = movementDate;

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
    }
}
