namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class WarehouseTransferLine : BaseEntity
    {
        public long WarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public WarehouseTransfer? WarehouseTransfer { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
