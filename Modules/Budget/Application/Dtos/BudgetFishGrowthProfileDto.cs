namespace aqua_api.Modules.Budget.Application.Dtos
{
    public class BudgetFishGrowthProfileDto
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public int StartMonth { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<BudgetFishGrowthProfileLineDto> Lines { get; set; } = new();
    }

    public class BudgetFishGrowthProfileSummaryDto
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public int StartMonth { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LineCount { get; set; }
        public decimal FinalTotalGram { get; set; }
    }

    public class BudgetFishGrowthProfileLineDto
    {
        public long Id { get; set; }
        public long BudgetFishGrowthProfileId { get; set; }
        public int GrowthMonthNo { get; set; }
        public int CalendarMonth { get; set; }
        public decimal MonthlyGrowthGram { get; set; }
        public decimal TotalGram { get; set; }
    }

    public class UpsertBudgetFishGrowthProfileDto
    {
        public long StockId { get; set; }
        public int StartMonth { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<UpsertBudgetFishGrowthProfileLineDto> Lines { get; set; } = new();
    }

    public class UpsertBudgetFishGrowthProfileLineDto
    {
        public int GrowthMonthNo { get; set; }
        public decimal MonthlyGrowthGram { get; set; }
    }

    public class ImportBudgetFishGrowthProfilesDto
    {
        public List<ImportBudgetFishGrowthProfileRowDto> Rows { get; set; } = new();
    }

    public class ImportBudgetFishGrowthProfileRowDto
    {
        public string StockCode { get; set; } = string.Empty;
        public int StartMonth { get; set; }
        public int GrowthMonthNo { get; set; }
        public decimal MonthlyGrowthGram { get; set; }
    }

    public class ImportBudgetFishGrowthProfilesResultDto
    {
        public int ProfileCount { get; set; }
        public int RowCount { get; set; }
    }
}
