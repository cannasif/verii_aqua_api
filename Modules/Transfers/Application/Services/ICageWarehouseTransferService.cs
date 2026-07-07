namespace aqua_api.Modules.Transfers.Application.Services
{
    public interface ICageWarehouseTransferService
    {
        Task<ApiResponse<CageWarehouseTransferDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<CageWarehouseTransferDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CageWarehouseTransferDto>> CreateAsync(CreateCageWarehouseTransferDto dto);
        Task<ApiResponse<CageWarehouseTransferDto>> UpdateAsync(long id, UpdateCageWarehouseTransferDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id, long? userId = null);
        Task<ApiResponse<bool>> Post(long cageWarehouseTransferId, long userId);
    }
}
