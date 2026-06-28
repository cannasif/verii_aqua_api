namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanExchangeRate : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string RateType { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? SourceReference { get; set; }
    public bool IsManualOverride { get; set; }
    public string? Description { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
}
