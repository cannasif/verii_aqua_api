using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/CageWarehouseTransferLine")]
    [Authorize]
    public class CageWarehouseTransferLineController : ControllerBase
    {
        private readonly ICageWarehouseTransferLineService _service;

        public CageWarehouseTransferLineController(ICageWarehouseTransferLineService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferLineDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CageWarehouseTransferLineDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferLineDto>>> Create([FromBody] CreateCageWarehouseTransferLineDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("auto-header")]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferLineDto>>> CreateWithAutoHeader([FromBody] CreateCageWarehouseTransferLineWithAutoHeaderDto dto)
        {
            var result = await _service.CreateWithAutoHeaderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferLineDto>>> Update(long id, [FromBody] UpdateCageWarehouseTransferLineDto dto)
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
