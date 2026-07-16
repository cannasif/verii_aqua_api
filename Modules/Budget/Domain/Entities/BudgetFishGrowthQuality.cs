using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Budget.Domain.Entities;

public class BudgetFishGrowthQuality : BaseEntity
{
    public long FishStockId { get; set; }
    public int GrowthMonthNo { get; set; }
    public decimal QualityPercent { get; set; }
    public string? Description { get; set; }

    public StockEntity? FishStock { get; set; }
}
