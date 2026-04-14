
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IProjectCageService
    {
        Task<ApiResponse<ProjectCageDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<ProjectCageDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<ProjectCageDto>> CreateAsync(CreateProjectCageDto dto);
        Task<ApiResponse<ProjectCageDto>> UpdateAsync(long id, UpdateProjectCageDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
