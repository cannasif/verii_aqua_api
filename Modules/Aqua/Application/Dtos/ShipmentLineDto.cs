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
        public string CurrencyCode { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? LocalUnitPrice { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? LocalLineAmount { get; set; }
    }

    public class CreateShipmentLineDto
    {
        public long ShipmentId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? LocalUnitPrice { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? LocalLineAmount { get; set; }
    }

    public class UpdateShipmentLineDto : CreateShipmentLineDto
    {
    }

    public class CreateShipmentLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime ShipmentDate { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? LocalUnitPrice { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? LocalLineAmount { get; set; }
    }
}
