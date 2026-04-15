namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWarehouseTransferLineService
    {
        Task<ApiResponse<WarehouseTransferLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WarehouseTransferLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WarehouseTransferLineDto>> CreateAsync(CreateWarehouseTransferLineDto dto);
        Task<ApiResponse<WarehouseTransferLineDto>> CreateWithAutoHeaderAsync(CreateWarehouseTransferLineWithAutoHeaderDto dto);
        Task<ApiResponse<WarehouseTransferLineDto>> UpdateAsync(long id, UpdateWarehouseTransferLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
