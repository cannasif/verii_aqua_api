using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aqua_api.Modules.Shipments.Api
{
    [ApiController]
    [Route("api/aqua/ShipmentLine")]
    [Authorize]
    public class ShipmentLineController : ControllerBase
    {
        private readonly IShipmentLineService _service;

        public ShipmentLineController(IShipmentLineService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<ShipmentLineDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ShipmentLineDto>>>> GetAll([FromBody] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ShipmentLineDto>>> Create([FromBody] CreateShipmentLineDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("auto-header")]
        public async Task<ActionResult<ApiResponse<ShipmentLineDto>>> CreateWithAutoHeader([FromBody] CreateShipmentLineWithAutoHeaderDto dto)
        {
            var result = await _service.CreateWithAutoHeaderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("auto-header-and-post")]
        public async Task<ActionResult<ApiResponse<ShipmentLineDto>>> CreateWithAutoHeaderAndPost([FromBody] CreateShipmentLineWithAutoHeaderDto dto)
        {
            var result = await _service.CreateWithAutoHeaderAndPostAsync(dto, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<ShipmentLineDto>>> Update(long id, [FromBody] UpdateShipmentLineDto dto)
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

        private long GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(raw, out var userId) ? userId : 1L;
        }
    }
}
