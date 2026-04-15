
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IBalanceLedgerManager
    {
        Task ApplyDelta(
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
            long? actorUserId = null);

        Task ApplyWarehouseDelta(
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
            long? actorUserId = null);
    }
}
