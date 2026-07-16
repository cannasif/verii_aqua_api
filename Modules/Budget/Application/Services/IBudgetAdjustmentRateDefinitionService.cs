namespace aqua_api.Modules.Budget.Application.Services;

public interface IBudgetAdjustmentRateDefinitionService
{
    Task<ApiResponse<PagedResponse<BudgetFeedMortalityRateDto>>> GetFeedMortalityRatesAsync(PagedRequest request);
    Task<ApiResponse<BudgetFeedMortalityRateDto>> GetFeedMortalityRateAsync(long id);
    Task<ApiResponse<BudgetFeedMortalityRateDto>> CreateFeedMortalityRateAsync(CreateBudgetFeedMortalityRateDto dto);
    Task<ApiResponse<BudgetFeedMortalityRateDto>> UpdateFeedMortalityRateAsync(long id, CreateBudgetFeedMortalityRateDto dto);
    Task<ApiResponse<bool>> DeleteFeedMortalityRateAsync(long id);

    Task<ApiResponse<PagedResponse<BudgetFishGrowthQualityDto>>> GetFishGrowthQualitiesAsync(PagedRequest request);
    Task<ApiResponse<BudgetFishGrowthQualityDto>> GetFishGrowthQualityAsync(long id);
    Task<ApiResponse<BudgetFishGrowthQualityDto>> CreateFishGrowthQualityAsync(CreateBudgetFishGrowthQualityDto dto);
    Task<ApiResponse<BudgetFishGrowthQualityDto>> UpdateFishGrowthQualityAsync(long id, CreateBudgetFishGrowthQualityDto dto);
    Task<ApiResponse<bool>> DeleteFishGrowthQualityAsync(long id);
}
