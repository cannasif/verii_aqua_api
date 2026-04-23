using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/dashboard-project")]
    [Authorize]
    public class DashboardProjectReportController : ControllerBase
    {
        private readonly IDashboardProjectReportService _service;

        public DashboardProjectReportController(IDashboardProjectReportService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<DashboardProjectsResponseDto>>> GetSummary([FromQuery] List<long> projectIds)
        {
            var result = await _service.GetProjectSummariesAsync(projectIds);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("summary")]
        public async Task<ActionResult<ApiResponse<DashboardProjectsResponseDto>>> PostSummary([FromBody] DashboardProjectsRequestDto? request)
        {
            var result = await _service.GetProjectSummariesAsync(request?.ProjectIds ?? []);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("detail/{projectId:long}")]
        public async Task<ActionResult<ApiResponse<DashboardProjectDetailDto>>> GetDetail(long projectId)
        {
            var result = await _service.GetProjectDetailAsync(projectId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
