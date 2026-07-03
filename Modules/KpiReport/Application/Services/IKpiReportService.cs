using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;

namespace aqua_api.Modules.KpiReport.Application.Services;

public interface IKpiReportService
{
    Task<ApiResponse<List<KpiReportProjectOptionDto>>> GetProjectsAsync();
    Task<ApiResponse<ProjectFeedFishSummaryReportDto>> GetProjectFeedFishSummaryAsync(ProjectFeedFishSummaryRequestDto? request);
    Task<ApiResponse<DailyFeedingReportDto>> GetDailyFeedingReportAsync(DailyFeedingReportRequestDto? request);
    Task<ApiResponse<MonthlyOperationalReportDto>> GetMonthlyFeedingReportAsync(MonthlyOperationalReportRequestDto? request);
    Task<ApiResponse<MonthlyOperationalReportDto>> GetMonthlyMortalityReportAsync(MonthlyOperationalReportRequestDto? request);
    Task<ApiResponse<MonthlyOperationalReportDto>> GetMonthlyShipmentReportAsync(MonthlyOperationalReportRequestDto? request);
    Task<ApiResponse<MortalityTrackingReportDto>> GetMortalityTrackingReportAsync(MonthlyOperationalReportRequestDto? request);
    Task<ApiResponse<DevirFcrReportDto>> GetDevirFcrReportAsync(DevirFcrReportRequestDto request);
    Task<ApiResponse<RawKpiReportDto>> GetRawKpiReportAsync(long projectId);
    Task<ApiResponse<ProjectDetailReportDto>> GetProjectDetailReportAsync(long projectId);
    Task<ApiResponse<BusinessKpiReportDto>> GetBusinessKpiReportAsync(long projectId);
}
