namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface ICageWarehouseTransferService
    {
        Task<ApiResponse<CageWarehouseTransferDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<CageWarehouseTransferDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CageWarehouseTransferDto>> CreateAsync(CreateCageWarehouseTransferDto dto);
        Task<ApiResponse<CageWarehouseTransferDto>> UpdateAsync(long id, UpdateCageWarehouseTransferDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long cageWarehouseTransferId, long userId);
    }
}
