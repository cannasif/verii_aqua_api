using aqua_api.Modules.Aqua.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;

namespace aqua_api.Modules.KpiReport.Application.Services;

public interface IKpiReportService
{
    Task<ApiResponse<List<KpiReportProjectOptionDto>>> GetProjectsAsync();
    Task<ApiResponse<DevirFcrReportDto>> GetDevirFcrReportAsync(DevirFcrReportRequestDto request);
    Task<ApiResponse<RawKpiReportDto>> GetRawKpiReportAsync(long projectId);
    Task<ApiResponse<ProjectDetailReportDto>> GetProjectDetailReportAsync(long projectId);
    Task<ApiResponse<BusinessKpiReportDto>> GetBusinessKpiReportAsync(long projectId);
}
