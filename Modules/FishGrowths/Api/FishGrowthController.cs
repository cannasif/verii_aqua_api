using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aqua_api.Modules.FishGrowths.Api;

[ApiController]
[Route("api/aqua/FishGrowth")]
[Authorize]
public class FishGrowthController : ControllerBase
{
    private readonly IFishGrowthService _service;

    public FishGrowthController(IFishGrowthService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<FishGrowthDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FishGrowthDto>>> Create([FromBody] CreateFishGrowthDto dto)
    {
        var result = await _service.CreateAsync(dto, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    private long GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out var userId) ? userId : 1L;
    }
}
