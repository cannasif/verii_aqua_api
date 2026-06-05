namespace aqua_api.Modules.AquaReports.Application.Services
{
    public interface IDashboardProjectReportService
    {
        Task<ApiResponse<DashboardProjectsResponseDto>> GetProjectSummariesAsync(IEnumerable<long> projectIds);
        Task<ApiResponse<DashboardProjectDetailDto>> GetProjectDetailAsync(long projectId);
    }
}
