
namespace aqua_api.Modules.Feedings.Application.Services
{
    public interface IFeedingService
    {
        Task<ApiResponse<FeedingDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<FeedingDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<FeedingDto>> CreateAsync(CreateFeedingDto dto);
        Task<ApiResponse<FeedingDto>> UpdateAsync(long id, UpdateFeedingDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long feedingId, long userId);
    }
}
