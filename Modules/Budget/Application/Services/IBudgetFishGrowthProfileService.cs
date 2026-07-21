namespace aqua_api.Modules.Budget.Application.Services
{
    public interface IBudgetFishGrowthProfileService
    {
        Task<ApiResponse<PagedResponse<BudgetFishGrowthProfileSummaryDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BudgetFishGrowthProfileDto>> GetByIdAsync(long id);
        Task<ApiResponse<BudgetFishGrowthProfileDto>> GetByStockAndStartMonthAsync(long stockId, int startMonth);
        Task<ApiResponse<BudgetFishGrowthProfileDto>> UpsertAsync(UpsertBudgetFishGrowthProfileDto dto);
        Task<ApiResponse<ImportBudgetFishGrowthProfilesResultDto>> ImportAsync(ImportBudgetFishGrowthProfilesDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
