using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Budget.Domain.Entities
{
    public class BudgetFishGrowthProfile : BaseEntity
    {
        public long StockId { get; set; }
        public int StartMonth { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public StockEntity Stock { get; set; } = null!;
        public ICollection<BudgetFishGrowthProfileLine> Lines { get; set; } = new List<BudgetFishGrowthProfileLine>();
    }
}
