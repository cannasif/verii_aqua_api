using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api;

[ApiController]
[Route("api/budget/FishGrowthQuality")]
[Authorize]
public class BudgetFishGrowthQualityController : ControllerBase
{
    private readonly IBudgetAdjustmentRateDefinitionService _service;

    public BudgetFishGrowthQualityController(IBudgetAdjustmentRateDefinitionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<BudgetFishGrowthQualityDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _service.GetFishGrowthQualitiesAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetFishGrowthQualityDto>>> GetById(long id)
    {
        var result = await _service.GetFishGrowthQualityAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetFishGrowthQualityDto>>> Create([FromBody] CreateBudgetFishGrowthQualityDto dto)
    {
        var result = await _service.CreateFishGrowthQualityAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetFishGrowthQualityDto>>> Update(long id, [FromBody] CreateBudgetFishGrowthQualityDto dto)
    {
        var result = await _service.UpdateFishGrowthQualityAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteFishGrowthQualityAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
