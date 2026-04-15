namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class WarehouseCageTransferLine : BaseEntity
    {
        public long WarehouseCageTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public WarehouseCageTransfer? WarehouseCageTransfer { get; set; }
        public FishBatch? FishBatch { get; set; }
        public aqua_api.Modules.Warehouse.Domain.Entities.Warehouse? FromWarehouse { get; set; }
        public ProjectCage? ToProjectCage { get; set; }
    }
}
