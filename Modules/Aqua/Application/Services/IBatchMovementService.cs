
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IBatchMovementService
    {
        Task<ApiResponse<BatchMovementDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BatchMovementDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BatchMovementDto>> CreateAsync(CreateBatchMovementDto dto);
        Task<ApiResponse<BatchMovementDto>> UpdateAsync(long id, UpdateBatchMovementDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
