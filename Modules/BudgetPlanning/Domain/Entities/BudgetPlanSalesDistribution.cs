using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanSalesDistribution : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanMonthlyProjectionId { get; set; }
    public long BudgetPlanSalesLineId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public long? CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetMarketType MarketType { get; set; }
    public decimal SalesTon { get; set; }
    public decimal SalesKg { get; set; }
    public int SalesCount { get; set; }
    public decimal UnitGram { get; set; }
    public string CurrencyCode { get; set; } = "EUR";
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceEuro { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountEuro { get; set; }
    public decimal? AmountTry { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanMonthlyProjection BudgetPlanMonthlyProjection { get; set; } = null!;
    public BudgetPlanSalesLine BudgetPlanSalesLine { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
    public BudgetCalibrationDefinition? CalibrationDefinition { get; set; }
}
