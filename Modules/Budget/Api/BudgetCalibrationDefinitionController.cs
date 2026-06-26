using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Budget.Api
{
    [ApiController]
    [Route("api/BudgetCalibrationDefinition")]
    [Route("api/budget/CalibrationDefinition")]
    [Authorize]
    public class BudgetCalibrationDefinitionController : ControllerBase
    {
        private readonly IBudgetCalibrationDefinitionService _service;

        public BudgetCalibrationDefinitionController(IBudgetCalibrationDefinitionService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetCalibrationDefinitionDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<BudgetCalibrationDefinitionDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BudgetCalibrationDefinitionDto>>> Create([FromBody] CreateBudgetCalibrationDefinitionDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<BudgetCalibrationDefinitionDto>>> Update(long id, [FromBody] UpdateBudgetCalibrationDefinitionDto dto)
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
