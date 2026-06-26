namespace aqua_api.Modules.Budget.Application.Dtos
{
    public class BudgetFeedConsumptionRateDto
    {
        public long Id { get; set; }
        public long WaterTemperatureId { get; set; }
        public int? WaterTemperatureYear { get; set; }
        public int? WaterTemperatureMonth { get; set; }
        public decimal? WaterTemperatureCelsius { get; set; }
        public long CalibrationDefinitionId { get; set; }
        public string? CalibrationCode { get; set; }
        public string? CalibrationInfo { get; set; }
        public long FeedStockId { get; set; }
        public string? FeedStockCode { get; set; }
        public string? FeedStockName { get; set; }
        public decimal FeedAmount { get; set; }
        public string? Description { get; set; }
    }

    public class CreateBudgetFeedConsumptionRateDto
    {
        public long WaterTemperatureId { get; set; }
        public long CalibrationDefinitionId { get; set; }
        public long FeedStockId { get; set; }
        public decimal FeedAmount { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateBudgetFeedConsumptionRateDto : CreateBudgetFeedConsumptionRateDto
    {
    }
}
