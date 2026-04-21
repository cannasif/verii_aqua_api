namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IDashboardProjectReportService
    {
        Task<ApiResponse<DashboardProjectsResponseDto>> GetProjectSummariesAsync(IEnumerable<long> projectIds);
        Task<ApiResponse<DashboardProjectDetailDto>> GetProjectDetailAsync(long projectId);
    }
}
