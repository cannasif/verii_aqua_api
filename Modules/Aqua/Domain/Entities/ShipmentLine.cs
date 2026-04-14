namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ShipmentLine : BaseEntity
    {
        public long ShipmentId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public Shipment? Shipment { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? FromProjectCage { get; set; }
    }
}
