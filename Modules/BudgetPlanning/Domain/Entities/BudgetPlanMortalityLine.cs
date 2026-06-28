namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanMortalityLine : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanMonthlyProjectionId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal MortalityRatePercent { get; set; }
    public int MortalityCount { get; set; }
    public decimal MortalityKg { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanMonthlyProjection BudgetPlanMonthlyProjection { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
}
