using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanFishBatch : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public long BudgetPlanProjectId { get; set; }
    public BudgetPlanSourceType SourceType { get; set; }
    public long? SourceFishBatchId { get; set; }
    public long FishStockId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public int InitialLiveCount { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal InitialBiomassKg { get; set; }
    public decimal? InitialUnitCost { get; set; }
    public decimal? InitialSmmAmount { get; set; }
    public int GrowthStartYear { get; set; }
    public int GrowthStartMonth { get; set; }
    public string? Note { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public BudgetPlanProject BudgetPlanProject { get; set; } = null!;
    public FishBatch? SourceFishBatch { get; set; }
    public StockEntity FishStock { get; set; } = null!;
    public ICollection<BudgetPlanFishBatchAdjustment> Adjustments { get; set; } = new List<BudgetPlanFishBatchAdjustment>();
}
