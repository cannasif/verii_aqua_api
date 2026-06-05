namespace aqua_api.Modules.Cages.Application.Services
{
    public interface ICageWarehouseMappingService
    {
        Task<ApiResponse<CageWarehouseMappingDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<CageWarehouseMappingDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CageWarehouseMappingDto>> CreateAsync(CreateCageWarehouseMappingDto dto);
        Task<ApiResponse<CageWarehouseMappingDto>> UpdateAsync(long id, UpdateCageWarehouseMappingDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
