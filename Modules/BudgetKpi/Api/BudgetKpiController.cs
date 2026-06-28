using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.BudgetKpi.Api;

[ApiController]
[Route("api/budget/Kpi")]
[Authorize]
public class BudgetKpiController : ControllerBase
{
    private readonly IBudgetKpiService _service;

    public BudgetKpiController(IBudgetKpiService service)
    {
        _service = service;
    }

    [HttpGet("{budgetPlanId:long}/report")]
    public async Task<ActionResult<ApiResponse<BudgetKpiReportDto>>> GetReport(long budgetPlanId)
    {
        var result = await _service.GetReportAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }
}
