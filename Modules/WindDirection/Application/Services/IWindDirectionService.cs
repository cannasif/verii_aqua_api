namespace aqua_api.Modules.WindDirection.Application.Services
{
    public interface IWindDirectionService
    {
        Task<ApiResponse<WindDirectionDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WindDirectionDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WindDirectionDto>> CreateAsync(CreateWindDirectionDto dto);
        Task<ApiResponse<WindDirectionDto>> UpdateAsync(long id, UpdateWindDirectionDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
