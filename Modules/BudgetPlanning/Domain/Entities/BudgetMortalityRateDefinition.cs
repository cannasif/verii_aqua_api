namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

public class BudgetMortalityRateDefinition : BaseEntity
{
    public long? FishStockId { get; set; }
    public long? CalibrationDefinitionId { get; set; }
    public int? GrowthMonthNo { get; set; }
    public decimal MortalityRatePercent { get; set; }
    public string? Description { get; set; }

    public StockEntity? FishStock { get; set; }
    public BudgetCalibrationDefinition? CalibrationDefinition { get; set; }
}
