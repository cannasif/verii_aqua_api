namespace aqua_api.Modules.Budget.Application.Dtos
{
    public class BudgetWaterTemperatureDto
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal WaterTemperatureCelsius { get; set; }
        public string? Description { get; set; }
    }

    public class CreateBudgetWaterTemperatureDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal WaterTemperatureCelsius { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateBudgetWaterTemperatureDto : CreateBudgetWaterTemperatureDto
    {
    }
}
