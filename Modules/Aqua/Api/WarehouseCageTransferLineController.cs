using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/WarehouseCageTransferLine")]
    [Authorize]
    public class WarehouseCageTransferLineController : ControllerBase
    {
        private readonly IWarehouseCageTransferLineService _service;

        public WarehouseCageTransferLineController(IWarehouseCageTransferLineService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<WarehouseCageTransferLineDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<WarehouseCageTransferLineDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WarehouseCageTransferLineDto>>> Create([FromBody] CreateWarehouseCageTransferLineDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("auto-header")]
        public async Task<ActionResult<ApiResponse<WarehouseCageTransferLineDto>>> CreateWithAutoHeader([FromBody] CreateWarehouseCageTransferLineWithAutoHeaderDto dto)
        {
            var result = await _service.CreateWithAutoHeaderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<WarehouseCageTransferLineDto>>> Update(long id, [FromBody] UpdateWarehouseCageTransferLineDto dto)
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
