namespace aqua_api.Modules.Budget.Domain.Entities
{
    public class BudgetWaterTemperature : BaseEntity
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal WaterTemperatureCelsius { get; set; }
        public string? Description { get; set; }
    }
}
