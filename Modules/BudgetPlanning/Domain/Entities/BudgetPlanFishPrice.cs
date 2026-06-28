namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanFishPrice : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UnitPriceEuro { get; set; }
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetCalibrationDefinition CalibrationDefinition { get; set; } = null!;
}
