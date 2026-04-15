using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/WarehouseTransferLine")]
    [Authorize]
    public class WarehouseTransferLineController : ControllerBase
    {
        private readonly IWarehouseTransferLineService _service;

        public WarehouseTransferLineController(IWarehouseTransferLineService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<WarehouseTransferLineDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<WarehouseTransferLineDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WarehouseTransferLineDto>>> Create([FromBody] CreateWarehouseTransferLineDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("auto-header")]
        public async Task<ActionResult<ApiResponse<WarehouseTransferLineDto>>> CreateWithAutoHeader([FromBody] CreateWarehouseTransferLineWithAutoHeaderDto dto)
        {
            var result = await _service.CreateWithAutoHeaderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<WarehouseTransferLineDto>>> Update(long id, [FromBody] UpdateWarehouseTransferLineDto dto)
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
