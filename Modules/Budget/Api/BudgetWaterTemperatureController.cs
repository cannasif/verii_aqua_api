using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api
{
    [ApiController]
    [Route("api/BudgetWaterTemperature")]
    [Route("api/budget/WaterTemperature")]
    [Authorize]
    public class BudgetWaterTemperatureController : ControllerBase
    {
        private readonly IBudgetWaterTemperatureService _service;

        public BudgetWaterTemperatureController(IBudgetWaterTemperatureService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetWaterTemperatureDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<BudgetWaterTemperatureDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BudgetWaterTemperatureDto>>> Create([FromBody] CreateBudgetWaterTemperatureDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetWaterTemperatureDto>>> Update(long id, [FromBody] UpdateBudgetWaterTemperatureDto dto)
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
