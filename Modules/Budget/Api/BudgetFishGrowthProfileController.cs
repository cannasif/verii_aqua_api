using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api
{
    [ApiController]
    [Route("api/BudgetFishGrowthProfile")]
    [Route("api/budget/FishGrowthProfile")]
    [Authorize]
    public class BudgetFishGrowthProfileController : ControllerBase
    {
        private readonly IBudgetFishGrowthProfileService _service;

        public BudgetFishGrowthProfileController(IBudgetFishGrowthProfileService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetFishGrowthProfileDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<BudgetFishGrowthProfileSummaryDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-stock/{stockId:long}/start-month/{startMonth:int}")]
        public async Task<ActionResult<ApiResponse<BudgetFishGrowthProfileDto>>> GetByStockAndStartMonth(long stockId, int startMonth)
        {
            var result = await _service.GetByStockAndStartMonthAsync(stockId, startMonth);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("upsert")]
        public async Task<ActionResult<ApiResponse<BudgetFishGrowthProfileDto>>> Upsert([FromBody] UpsertBudgetFishGrowthProfileDto dto)
        {
            var result = await _service.UpsertAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
        {
            var result = await _service.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
