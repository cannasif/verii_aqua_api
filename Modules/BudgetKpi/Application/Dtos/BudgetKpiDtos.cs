namespace aqua_api.Modules.BudgetKpi.Application.Dtos;

public class BudgetKpiReportDto
{
    public BudgetKpiSummaryDto Summary { get; set; } = new();
    public List<BudgetKpiMonthlyDto> MonthlyRows { get; set; } = new();
}

public class BudgetKpiMonthlyDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal OpeningBiomassKg { get; set; }
    public decimal ClosingBiomassKg { get; set; }
    public decimal GrowthBiomassKg { get; set; }
    public decimal SalesKg { get; set; }
    public decimal FeedKg { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public decimal Fcr { get; set; }
}
