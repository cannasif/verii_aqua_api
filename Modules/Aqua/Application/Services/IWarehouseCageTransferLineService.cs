namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWarehouseCageTransferLineService
    {
        Task<ApiResponse<WarehouseCageTransferLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WarehouseCageTransferLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WarehouseCageTransferLineDto>> CreateAsync(CreateWarehouseCageTransferLineDto dto);
        Task<ApiResponse<WarehouseCageTransferLineDto>> CreateWithAutoHeaderAsync(CreateWarehouseCageTransferLineWithAutoHeaderDto dto);
        Task<ApiResponse<WarehouseCageTransferLineDto>> UpdateAsync(long id, UpdateWarehouseCageTransferLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
