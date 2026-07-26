namespace aqua_api.Modules.Integrations.Application.Services;

public interface IBudgetExchangeRateReadService
{
    Task<ApiResponse<List<ErpBudgetExchangeRateDto>>> GetBudgetExchangeRatesAsync(
        int startYear,
        int startMonth,
        int endYear,
        int endMonth);
}
