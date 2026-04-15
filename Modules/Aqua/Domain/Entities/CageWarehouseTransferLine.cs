namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class CageWarehouseTransferLine : BaseEntity
    {
        public long CageWarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public CageWarehouseTransfer? CageWarehouseTransfer { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? FromProjectCage { get; set; }
        public aqua_api.Modules.Warehouse.Domain.Entities.Warehouse? ToWarehouse { get; set; }
    }
}
