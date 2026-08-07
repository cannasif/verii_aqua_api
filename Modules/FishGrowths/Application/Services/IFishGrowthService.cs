namespace aqua_api.Modules.FishGrowths.Application.Services;

public interface IFishGrowthService
{
    Task<ApiResponse<PagedResponse<FishGrowthDto>>> GetAllAsync(PagedRequest request);
    Task<ApiResponse<FishGrowthDto?>> GetMonthlyAsync(long projectCageId, long fishBatchId, int year, int month);
    Task<ApiResponse<FishGrowthDto>> CreateAsync(CreateFishGrowthDto dto, long userId);
    Task<ApiResponse<FishGrowthDto>> UpdateAsync(long id, UpdateFishGrowthDto dto, long userId);
    Task<ApiResponse<bool>> DeleteAsync(long id, long userId);
}
