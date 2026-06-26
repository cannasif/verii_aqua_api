namespace aqua_api.Modules.Budget.Domain.Entities
{
    public class BudgetFishGrowthProfileLine : BaseEntity
    {
        public long BudgetFishGrowthProfileId { get; set; }
        public int GrowthMonthNo { get; set; }
        public int CalendarMonth { get; set; }
        public decimal MonthlyGrowthGram { get; set; }
        public decimal TotalGram { get; set; }

        public BudgetFishGrowthProfile BudgetFishGrowthProfile { get; set; } = null!;
    }
}
