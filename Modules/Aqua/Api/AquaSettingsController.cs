using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/AquaSettings")]
    [Authorize]
    public class AquaSettingsController : ControllerBase
    {
        private readonly IAquaSettingsService _service;
        private readonly IUserService _userService;

        public AquaSettingsController(IAquaSettingsService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<AquaSettingsDto>>> Get()
        {
            var result = await _service.GetAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<AquaSettingsDto>>> Update([FromBody] UpdateAquaSettingsDto dto)
        {
            var currentUserResponse = await _userService.GetCurrentUserIdAsync();
            if (!currentUserResponse.Success)
            {
                var unauth = ApiResponse<AquaSettingsDto>.ErrorResult(
                    currentUserResponse.Message,
                    currentUserResponse.Message,
                    StatusCodes.Status401Unauthorized);

                return StatusCode(unauth.StatusCode, unauth);
            }

            var result = await _service.UpdateAsync(dto, currentUserResponse.Data);
            return StatusCode(result.StatusCode, result);
        }
    }
}
