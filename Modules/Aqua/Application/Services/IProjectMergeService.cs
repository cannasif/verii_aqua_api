namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IProjectMergeService
    {
        Task<ApiResponse<ProjectMergeDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<ProjectMergeDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<ProjectMergeDto>> CreateAsync(CreateProjectMergeDto dto, long userId);
    }
}
