namespace aqua_api.Modules.BudgetKpi.Application.Services;

public interface IBudgetKpiService
{
    Task<ApiResponse<BudgetKpiReportDto>> GetReportAsync(long budgetPlanId);
}
