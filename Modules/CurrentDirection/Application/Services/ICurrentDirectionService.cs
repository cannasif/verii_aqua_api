namespace aqua_api.Modules.CurrentDirection.Application.Services
{
    public interface ICurrentDirectionService
    {
        Task<ApiResponse<PagedResponse<CurrentDirectionDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<CurrentDirectionDto>> GetByIdAsync(long id);
        Task<ApiResponse<CurrentDirectionDto>> CreateAsync(CreateCurrentDirectionDto dto);
        Task<ApiResponse<CurrentDirectionDto>> UpdateAsync(long id, UpdateCurrentDirectionDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
