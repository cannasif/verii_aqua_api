
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IFeedingDistributionService
    {
        Task<ApiResponse<FeedingDistributionDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<FeedingDistributionDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<FeedingDistributionDto>> CreateAsync(CreateFeedingDistributionDto dto);
        Task<ApiResponse<FeedingDistributionDto>> UpdateAsync(long id, UpdateFeedingDistributionDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
