namespace aqua_api.Modules.Budget.Application.Services
{
    public interface IBudgetFeedConsumptionRateService
    {
        Task<ApiResponse<BudgetFeedConsumptionRateDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BudgetFeedConsumptionRateDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<PagedResponse<StockGetDto>>> GetFeedStocksAsync(PagedRequest request);
        Task<ApiResponse<BudgetFeedConsumptionRateDto>> CreateAsync(CreateBudgetFeedConsumptionRateDto dto);
        Task<ApiResponse<BudgetFeedConsumptionRateDto>> UpdateAsync(long id, UpdateBudgetFeedConsumptionRateDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
