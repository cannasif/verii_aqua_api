using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/ProjectCageDailyKpi")]
    [Authorize]
    public class ProjectCageDailyKpiController : ControllerBase
    {
        private readonly IProjectCageDailyKpiService _service;

        public ProjectCageDailyKpiController(IProjectCageDailyKpiService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>>> GetLatest([FromQuery] long? projectId, [FromQuery] DateTime? snapshotDate)
        {
            var result = await _service.GetLatestAsync(projectId, snapshotDate);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("snapshot")]
        public async Task<ActionResult<ApiResponse<List<ProjectCageDailyKpiSnapshotDto>>>> CreateSnapshot([FromBody] CreateProjectCageDailyKpiSnapshotRequest request)
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = long.TryParse(rawUserId, out var parsedUserId) ? parsedUserId : 0;
            var result = await _service.CreateSnapshotAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
