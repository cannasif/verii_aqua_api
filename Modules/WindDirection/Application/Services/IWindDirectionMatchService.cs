namespace aqua_api.Modules.WindDirection.Application.Services
{
    public interface IWindDirectionMatchService
    {
        Task<ApiResponse<WindDirectionMatchDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WindDirectionMatchDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WindDirectionMatchDto>> CreateAsync(CreateWindDirectionMatchDto dto);
        Task<ApiResponse<WindDirectionMatchDto>> UpdateAsync(long id, UpdateWindDirectionMatchDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
