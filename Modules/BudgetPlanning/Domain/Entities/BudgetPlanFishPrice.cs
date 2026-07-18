namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

public class BudgetPlanFishPrice : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long? FishStockId { get; set; }
    public long CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetFishPriceType PriceType { get; set; } = BudgetFishPriceType.Sales;
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public string CurrencyCode { get; set; } = "EUR";
    public decimal UnitPrice { get; set; }
    public decimal IncreaseRatePercent { get; set; }
    public int IncreasePeriodMonths { get; set; } = 1;
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public StockEntity? FishStock { get; set; }
    public BudgetCalibrationDefinition CalibrationDefinition { get; set; } = null!;
}
