
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IShipmentLineService
    {
        Task<ApiResponse<ShipmentLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<ShipmentLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<ShipmentLineDto>> CreateAsync(CreateShipmentLineDto dto);
        Task<ApiResponse<ShipmentLineDto>> UpdateAsync(long id, UpdateShipmentLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
