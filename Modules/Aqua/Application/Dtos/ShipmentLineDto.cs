namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class ShipmentLineDto
    {
        public long Id { get; set; }
        public long ShipmentId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateShipmentLineDto
    {
        public long ShipmentId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateShipmentLineDto : CreateShipmentLineDto
    {
    }
}
