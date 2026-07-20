using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanSalesLine : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public decimal SalesTon { get; set; }
    public int? SalesCount { get; set; }
    public string CurrencyCode { get; set; } = "EUR";
    public decimal? SourceUnitPrice { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
}
