
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IFeedingLineService
    {
        Task<ApiResponse<FeedingLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<FeedingLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<FeedingLineDto>> CreateAsync(CreateFeedingLineDto dto);
        Task<ApiResponse<FeedingLineDto>> UpdateAsync(long id, UpdateFeedingLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
