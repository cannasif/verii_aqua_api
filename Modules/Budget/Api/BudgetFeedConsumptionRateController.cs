using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api
{
    [ApiController]
    [Route("api/BudgetFeedConsumptionRate")]
    [Route("api/budget/FeedConsumptionRate")]
    [Authorize]
    public class BudgetFeedConsumptionRateController : ControllerBase
    {
        private readonly IBudgetFeedConsumptionRateService _service;

        public BudgetFeedConsumptionRateController(IBudgetFeedConsumptionRateService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetFeedConsumptionRateDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<BudgetFeedConsumptionRateDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("feed-stocks")]
        public async Task<ActionResult<ApiResponse<PagedResponse<StockGetDto>>>> GetFeedStocks([FromQuery] PagedRequest request)
        {
            var result = await _service.GetFeedStocksAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BudgetFeedConsumptionRateDto>>> Create([FromBody] CreateBudgetFeedConsumptionRateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetFeedConsumptionRateDto>>> Update(long id, [FromBody] UpdateBudgetFeedConsumptionRateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
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
