namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IBatchWarehouseBalanceService
    {
        Task<ApiResponse<BatchWarehouseBalanceDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BatchWarehouseBalanceDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BatchWarehouseBalanceDto>> CreateAsync(CreateBatchWarehouseBalanceDto dto);
        Task<ApiResponse<BatchWarehouseBalanceDto>> UpdateAsync(long id, UpdateBatchWarehouseBalanceDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
