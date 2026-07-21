namespace aqua_api.Modules.FishGrowths.Application.Services;

public interface IFishGrowthService
{
    Task<ApiResponse<PagedResponse<FishGrowthDto>>> GetAllAsync(PagedRequest request);
    Task<ApiResponse<FishGrowthDto>> CreateAsync(CreateFishGrowthDto dto, long userId);
}
