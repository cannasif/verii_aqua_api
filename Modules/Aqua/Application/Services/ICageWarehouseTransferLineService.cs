namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface ICageWarehouseTransferLineService
    {
        Task<ApiResponse<CageWarehouseTransferLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<CageWarehouseTransferLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CageWarehouseTransferLineDto>> CreateAsync(CreateCageWarehouseTransferLineDto dto);
        Task<ApiResponse<CageWarehouseTransferLineDto>> CreateWithAutoHeaderAsync(CreateCageWarehouseTransferLineWithAutoHeaderDto dto);
        Task<ApiResponse<CageWarehouseTransferLineDto>> UpdateAsync(long id, UpdateCageWarehouseTransferLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
