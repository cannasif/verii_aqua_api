
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IFishBatchService
    {
        Task<ApiResponse<FishBatchDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<FishBatchDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<FishBatchDto>> CreateAsync(CreateFishBatchDto dto);
        Task<ApiResponse<FishBatchDto>> UpdateAsync(long id, UpdateFishBatchDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
