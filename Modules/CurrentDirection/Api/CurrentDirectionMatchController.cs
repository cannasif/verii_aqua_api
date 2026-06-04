using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.CurrentDirection.Api
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CurrentDirectionMatchController : ControllerBase
    {
        private readonly ICurrentDirectionMatchService _service;

        public CurrentDirectionMatchController(ICurrentDirectionMatchService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCurrentDirectionMatchDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateCurrentDirectionMatchDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
