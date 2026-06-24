namespace aqua_api.Modules.NetInventory.Application.Services;

public interface INetInventoryMovementService
{
    Task<ApiResponse<PagedResponse<NetInventoryMovementDto>>> GetAllAsync(PagedRequest request);
    Task<ApiResponse<NetInventoryMovementDto>> GetByIdAsync(long id);
    Task<ApiResponse<NetInventoryMovementDto>> CreateAsync(CreateNetInventoryMovementDto dto);
    Task<ApiResponse<NetInventoryMovementDto>> UpdateAsync(long id, UpdateNetInventoryMovementDto dto);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id);
}
