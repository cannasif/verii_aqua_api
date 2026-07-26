namespace aqua_api.Modules.Integrations.Application.Dtos;

public class ErpBudgetExchangeRateDto
{
    public int CurrencyTypeId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? YearMonth { get; set; }
    public decimal ExchangeRate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime? RecordDate { get; set; }
}
