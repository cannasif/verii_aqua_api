namespace aqua_api.Modules.Budget.Application.Dtos;

public class BudgetFeedMortalityRateDto
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
    public decimal ReductionRatePercent { get; set; }
    public string? Description { get; set; }
}

public class CreateBudgetFeedMortalityRateDto
{
    public long WaterTemperatureId { get; set; }
    public long CalibrationDefinitionId { get; set; }
    public long FeedStockId { get; set; }
    public decimal ReductionRatePercent { get; set; }
    public string? Description { get; set; }
}

public class BudgetFishGrowthQualityDto
{
    public long Id { get; set; }
    public long FishStockId { get; set; }
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public int GrowthMonthNo { get; set; }
    public decimal QualityPercent { get; set; }
    public string? Description { get; set; }
}

public class CreateBudgetFishGrowthQualityDto
{
    public long FishStockId { get; set; }
    public int GrowthMonthNo { get; set; }
    public decimal QualityPercent { get; set; }
    public string? Description { get; set; }
}
