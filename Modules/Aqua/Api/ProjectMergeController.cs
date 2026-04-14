using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/ProjectMerge")]
    [Authorize]
    public class ProjectMergeController : ControllerBase
    {
        private readonly IProjectMergeService _service;
        private readonly IUserService _userService;

        public ProjectMergeController(IProjectMergeService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<ProjectMergeDto>>> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProjectMergeDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProjectMergeDto>>> Create([FromBody] CreateProjectMergeDto dto)
        {
            var currentUserResponse = await _userService.GetCurrentUserIdAsync();
            if (!currentUserResponse.Success)
            {
                var unauth = ApiResponse<ProjectMergeDto>.ErrorResult(
                    currentUserResponse.Message,
                    currentUserResponse.Message,
                    StatusCodes.Status401Unauthorized);
                return StatusCode(unauth.StatusCode, unauth);
            }

            var result = await _service.CreateAsync(dto, currentUserResponse.Data);
            return StatusCode(result.StatusCode, result);
        }
    }
}
