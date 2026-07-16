using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api;

[ApiController]
[Route("api/budget/FeedMortalityRate")]
[Authorize]
public class BudgetFeedMortalityRateController : ControllerBase
{
    private readonly IBudgetAdjustmentRateDefinitionService _service;

    public BudgetFeedMortalityRateController(IBudgetAdjustmentRateDefinitionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<BudgetFeedMortalityRateDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _service.GetFeedMortalityRatesAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetFeedMortalityRateDto>>> GetById(long id)
    {
        var result = await _service.GetFeedMortalityRateAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetFeedMortalityRateDto>>> Create([FromBody] CreateBudgetFeedMortalityRateDto dto)
    {
        var result = await _service.CreateFeedMortalityRateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetFeedMortalityRateDto>>> Update(long id, [FromBody] CreateBudgetFeedMortalityRateDto dto)
    {
        var result = await _service.UpdateFeedMortalityRateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteFeedMortalityRateAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
