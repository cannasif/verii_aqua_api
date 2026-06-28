using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlan : BaseEntity
{
    public string BudgetNo { get; set; } = string.Empty;
    public string BudgetCode { get; set; } = string.Empty;
    public string BudgetName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int StartMonth { get; set; }
    public int EndYear { get; set; }
    public int EndMonth { get; set; }
    public BudgetPlanStatus Status { get; set; } = BudgetPlanStatus.Draft;
    public string? Description { get; set; }
    public DateTime? CalculatedAt { get; set; }

    public ICollection<BudgetPlanProject> Projects { get; set; } = new List<BudgetPlanProject>();
    public ICollection<BudgetPlanFishBatch> FishBatches { get; set; } = new List<BudgetPlanFishBatch>();
    public ICollection<BudgetPlanFishBatchAdjustment> FishBatchAdjustments { get; set; } = new List<BudgetPlanFishBatchAdjustment>();
    public ICollection<BudgetPlanMonthlyProjection> MonthlyProjections { get; set; } = new List<BudgetPlanMonthlyProjection>();
    public ICollection<BudgetPlanSalesLine> SalesLines { get; set; } = new List<BudgetPlanSalesLine>();
    public ICollection<BudgetPlanFeedingLine> FeedingLines { get; set; } = new List<BudgetPlanFeedingLine>();
    public ICollection<BudgetPlanMortalityLine> MortalityLines { get; set; } = new List<BudgetPlanMortalityLine>();
    public ICollection<BudgetPlanExchangeRate> ExchangeRates { get; set; } = new List<BudgetPlanExchangeRate>();
    public ICollection<BudgetPlanFishPrice> FishPrices { get; set; } = new List<BudgetPlanFishPrice>();
}
