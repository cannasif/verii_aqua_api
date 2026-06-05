namespace aqua_api.Modules.ProjectKpis.Application.Services
{
    public interface IProjectCageDailyKpiService
    {
        Task<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>> GetLatestAsync(long? projectId, DateTime? snapshotDate);
        Task<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>> CreateSnapshotAsync(CreateProjectCageDailyKpiSnapshotRequest request, long userId);
    }
}
