using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Identity.Api
{
    [ApiController]
    [Route("api/permission-definitions")]
    [Authorize]
    public class PermissionDefinitionController : ControllerBase
    {
        private readonly IPermissionDefinitionService _permissionDefinitionService;

        public PermissionDefinitionController(IPermissionDefinitionService permissionDefinitionService)
        {
            _permissionDefinitionService = permissionDefinitionService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PermissionDefinitionDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _permissionDefinitionService.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> GetById(long id)
        {
            var result = await _permissionDefinitionService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> Create([FromBody] CreatePermissionDefinitionDto dto)
        {
            var result = await _permissionDefinitionService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<PermissionDefinitionDto>>> Update(long id, [FromBody] UpdatePermissionDefinitionDto dto)
        {
            var result = await _permissionDefinitionService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }


        [HttpPost("sync")]
        public async Task<ActionResult<ApiResponse<PermissionDefinitionSyncResultDto>>> Sync([FromBody] SyncPermissionDefinitionsDto dto)
        {
            var result = await _permissionDefinitionService.SyncAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id)
        {
            var result = await _permissionDefinitionService.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
