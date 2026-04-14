using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Identity.Api
{
    [ApiController]
    [Route("api/permission-groups")]
    [Authorize]
    public class PermissionGroupController : ControllerBase
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public PermissionGroupController(IPermissionGroupService permissionGroupService)
        {
            _permissionGroupService = permissionGroupService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PermissionGroupDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _permissionGroupService.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> GetById(long id)
        {
            var result = await _permissionGroupService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> Create([FromBody] CreatePermissionGroupDto dto)
        {
            var result = await _permissionGroupService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> Update(long id, [FromBody] UpdatePermissionGroupDto dto)
        {
            var result = await _permissionGroupService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/permissions")]
        public async Task<ActionResult<ApiResponse<PermissionGroupDto>>> SetPermissions(long id, [FromBody] SetPermissionGroupPermissionsDto dto)
        {
            var result = await _permissionGroupService.SetPermissionsAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id)
        {
            var result = await _permissionGroupService.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
