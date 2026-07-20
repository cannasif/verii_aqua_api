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
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal OpeningBiomassKg { get; set; }
    public decimal ClosingBiomassKg { get; set; }
    public decimal GrowthBiomassKg { get; set; }
    public decimal ProducedBiomassKg { get; set; }
    public int SalesCount { get; set; }
    public decimal SalesTon { get; set; }
    public decimal DomesticSalesTon { get; set; }
    public decimal ForeignSalesTon { get; set; }
    public decimal SalesKg { get; set; }
    public int StockCount { get; set; }
    public decimal StockTon { get; set; }
    public decimal StockKg { get; set; }
    public decimal FeedKg { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public decimal UnitGram { get; set; }
    public decimal AveragePriceEuro { get; set; }
    public decimal AmountEuro { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? AveragePriceTry { get; set; }
    public decimal? AmountTry { get; set; }
    public decimal Fcr { get; set; }
}
