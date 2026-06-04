namespace aqua_api.Modules.CurrentDirection.Application.Services
{
    public interface ICurrentDirectionMatchService
    {
        Task<ApiResponse<PagedResponse<CurrentDirectionMatchDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CurrentDirectionMatchDto>> GetByIdAsync(long id);
        Task<ApiResponse<CurrentDirectionMatchDto>> CreateAsync(CreateCurrentDirectionMatchDto dto);
        Task<ApiResponse<CurrentDirectionMatchDto>> UpdateAsync(long id, UpdateCurrentDirectionMatchDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
