namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

public class BudgetPlanFeedingLine : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanMonthlyProjectionId { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public long? FeedStockId { get; set; }
    public decimal FeedAmountRate { get; set; }
    public decimal FeedKg { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanMonthlyProjection BudgetPlanMonthlyProjection { get; set; } = null!;
    public BudgetPlanFishBatch BudgetPlanFishBatch { get; set; } = null!;
    public StockEntity? FeedStock { get; set; }
}
