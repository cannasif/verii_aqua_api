namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWarehouseTransferService
    {
        Task<ApiResponse<WarehouseTransferDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WarehouseTransferDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WarehouseTransferDto>> CreateAsync(CreateWarehouseTransferDto dto);
        Task<ApiResponse<WarehouseTransferDto>> UpdateAsync(long id, UpdateWarehouseTransferDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long warehouseTransferId, long userId);
    }
}
