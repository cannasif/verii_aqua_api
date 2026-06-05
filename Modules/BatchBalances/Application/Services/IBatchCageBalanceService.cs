
namespace aqua_api.Modules.BatchBalances.Application.Services
{
    public interface IBatchCageBalanceService
    {
        Task<ApiResponse<BatchCageBalanceDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BatchCageBalanceDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BatchCageBalanceDto>> CreateAsync(CreateBatchCageBalanceDto dto);
        Task<ApiResponse<BatchCageBalanceDto>> UpdateAsync(long id, UpdateBatchCageBalanceDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
