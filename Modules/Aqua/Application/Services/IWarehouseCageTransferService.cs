namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWarehouseCageTransferService
    {
        Task<ApiResponse<WarehouseCageTransferDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WarehouseCageTransferDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WarehouseCageTransferDto>> CreateAsync(CreateWarehouseCageTransferDto dto);
        Task<ApiResponse<WarehouseCageTransferDto>> UpdateAsync(long id, UpdateWarehouseCageTransferDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long warehouseCageTransferId, long userId);
    }
}
