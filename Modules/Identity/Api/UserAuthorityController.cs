using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Identity.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAuthorityController : ControllerBase
    {
        private readonly IUserAuthorityService _userAuthorityService;
        private readonly ILocalizationService _localizationService;

        public UserAuthorityController(IUserAuthorityService userAuthorityService, ILocalizationService localizationService)
        {
            _userAuthorityService = userAuthorityService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserAuthorityDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _userAuthorityService.GetAllAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> GetById(long id)
        {
            var result = await _userAuthorityService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> Create([FromBody] CreateUserAuthorityDto createDto)
        {

            var result = await _userAuthorityService.CreateAsync(createDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<UserAuthorityDto>>> Update(long id, [FromBody] UpdateUserAuthorityDto updateDto)
        {

            var result = await _userAuthorityService.UpdateAsync(id, updateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(long id)
        {
            var result = await _userAuthorityService.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}/exists")]
        public async Task<ActionResult<ApiResponse<bool>>> Exists(long id)
        {
            var result = await _userAuthorityService.ExistsAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}