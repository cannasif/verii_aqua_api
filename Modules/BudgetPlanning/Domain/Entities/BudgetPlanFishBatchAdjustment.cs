using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanFishBatchAdjustment : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public BudgetPlanFishBatchAdjustmentType AdjustmentType { get; set; }
    public int? EffectiveYear { get; set; }
    public int? EffectiveMonth { get; set; }
    public int LiveCount { get; set; }
    public decimal AverageGram { get; set; }
    public decimal BiomassKg { get; set; }
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
}
