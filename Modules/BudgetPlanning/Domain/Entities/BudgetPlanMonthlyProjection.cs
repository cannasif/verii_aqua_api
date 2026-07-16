namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanMonthlyProjection : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int MonthIndex { get; set; }
    public int OpeningLiveCount { get; set; }
    public decimal OpeningAverageGram { get; set; }
    public decimal OpeningBiomassKg { get; set; }
    public decimal RawMonthlyGrowthGram { get; set; }
    public decimal GrowthQualityPercent { get; set; } = 100m;
    public decimal MonthlyGrowthGram { get; set; }
    public decimal ClosingAverageGram { get; set; }
    public decimal SalesTon { get; set; }
    public int SalesCount { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public decimal FeedKg { get; set; }
    public decimal FeedMortalityReductionPercent { get; set; }
    public decimal FeedMortalityReductionKg { get; set; }
    public int ClosingLiveCount { get; set; }
    public decimal ClosingBiomassKg { get; set; }
    public long? CalibrationDefinitionId { get; set; }
    public long? WaterTemperatureId { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
    public BudgetCalibrationDefinition? CalibrationDefinition { get; set; }
    public BudgetWaterTemperature? WaterTemperature { get; set; }
}
