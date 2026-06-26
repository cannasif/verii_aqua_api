namespace aqua_api.Modules.Budget.Domain.Entities
{
    public class BudgetFeedConsumptionRate : BaseEntity
    {
        public long WaterTemperatureId { get; set; }
        public long CalibrationDefinitionId { get; set; }
        public long FeedStockId { get; set; }
        public decimal FeedAmount { get; set; }
        public string? Description { get; set; }

        public BudgetWaterTemperature? WaterTemperature { get; set; }
        public BudgetCalibrationDefinition? CalibrationDefinition { get; set; }
        public aqua_api.Modules.Stock.Domain.Entities.Stock? FeedStock { get; set; }
    }
}
