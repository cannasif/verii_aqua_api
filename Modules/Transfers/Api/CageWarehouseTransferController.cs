using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aqua_api.Modules.Transfers.Api
{
    [ApiController]
    [Route("api/aqua/CageWarehouseTransfer")]
    [Authorize]
    public class CageWarehouseTransferController : ControllerBase
    {
        private readonly ICageWarehouseTransferService _service;

        public CageWarehouseTransferController(ICageWarehouseTransferService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CageWarehouseTransferDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferDto>>> Create([FromBody] CreateCageWarehouseTransferDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<CageWarehouseTransferDto>>> Update(long id, [FromBody] UpdateCageWarehouseTransferDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
        {
            var result = await _service.SoftDeleteAsync(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        private long? GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(raw, out var userId) ? userId : null;
        }
    }
}
