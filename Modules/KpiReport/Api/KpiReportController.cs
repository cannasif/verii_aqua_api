using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.KpiReport.Api;

[ApiController]
[Route("api/kpi-report")]
[Authorize]
public class KpiReportController : ControllerBase
{
    private readonly IKpiReportService _service;

    public KpiReportController(IKpiReportService service)
    {
        _service = service;
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<List<KpiReportProjectOptionDto>>>> GetProjects()
    {
        var result = await _service.GetProjectsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("devir-fcr")]
    public async Task<ActionResult<ApiResponse<DevirFcrReportDto>>> GetDevirFcrReport([FromBody] DevirFcrReportRequestDto request)
    {
        var result = await _service.GetDevirFcrReportAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("project-feed-fish-summary")]
    public async Task<ActionResult<ApiResponse<ProjectFeedFishSummaryReportDto>>> GetProjectFeedFishSummary([FromBody] ProjectFeedFishSummaryRequestDto? request)
    {
        var result = await _service.GetProjectFeedFishSummaryAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("raw-kpi/{projectId:long}")]
    public async Task<ActionResult<ApiResponse<RawKpiReportDto>>> GetRawKpiReport(long projectId)
    {
        var result = await _service.GetRawKpiReportAsync(projectId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("project-detail/{projectId:long}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailReportDto>>> GetProjectDetailReport(long projectId)
    {
        var result = await _service.GetProjectDetailReportAsync(projectId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("business-kpi/{projectId:long}")]
    public async Task<ActionResult<ApiResponse<BusinessKpiReportDto>>> GetBusinessKpiReport(long projectId)
    {
        var result = await _service.GetBusinessKpiReportAsync(projectId);
        return StatusCode(result.StatusCode, result);
    }
}
