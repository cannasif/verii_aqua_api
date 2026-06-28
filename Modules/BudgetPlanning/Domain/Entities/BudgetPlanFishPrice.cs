namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

public class BudgetPlanFishPrice : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long? FishStockId { get; set; }
    public long CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UnitPriceEuro { get; set; }
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public StockEntity? FishStock { get; set; }
    public BudgetCalibrationDefinition CalibrationDefinition { get; set; } = null!;
}
