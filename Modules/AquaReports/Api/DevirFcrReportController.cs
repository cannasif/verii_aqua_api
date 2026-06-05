using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.AquaReports.Api
{
    [ApiController]
    [Route("api/aqua/reports/devir-fcr")]
    [Authorize]
    public class DevirFcrReportController : ControllerBase
    {
        private readonly IDevirFcrReportService _service;

        public DevirFcrReportController(IDevirFcrReportService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DevirFcrReportDto>>> GetReport([FromBody] DevirFcrReportRequestDto request)
        {
            var result = await _service.GetReportAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
