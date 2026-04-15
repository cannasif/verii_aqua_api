namespace aqua_api.Modules.Warehouse.Application.Services
{
    public interface IWarehouseService
    {
        Task<ApiResponse<PagedResponse<WarehouseDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WarehouseDto>> GetByIdAsync(long id);
    }
}
