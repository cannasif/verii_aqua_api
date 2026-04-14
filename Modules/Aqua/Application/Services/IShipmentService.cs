
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IShipmentService
    {
        Task<ApiResponse<ShipmentDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<ShipmentDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<ShipmentDto>> CreateAsync(CreateShipmentDto dto);
        Task<ApiResponse<ShipmentDto>> UpdateAsync(long id, UpdateShipmentDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long shipmentId, long userId);
    }
}
