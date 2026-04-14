using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace aqua_api.Modules.Integrations.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SmtpSettingsController : ControllerBase
    {
        private readonly ISmtpSettingsService _smtpSettingsService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserService _userService;

        public SmtpSettingsController(
            ISmtpSettingsService smtpSettingsService,
            ILocalizationService localizationService,
            IUserService userService)
        {
            _smtpSettingsService = smtpSettingsService;
            _localizationService = localizationService;
            _userService = userService;
        }

        // GET api/SmtpSettings
        [HttpGet]
        public async Task<ActionResult<ApiResponse<SmtpSettingsDto>>> Get()
        {
            var res = await _smtpSettingsService.GetAsync();
            return StatusCode(res.StatusCode, res);
        }

        // PUT api/SmtpSettings
        [HttpPut]
        public async Task<ActionResult<ApiResponse<SmtpSettingsDto>>> Update([FromBody] UpdateSmtpSettingsDto dto)
        {

            var currentUserResponse = await _userService.GetCurrentUserIdAsync();
            if (!currentUserResponse.Success)
            {
                var unauth = ApiResponse<SmtpSettingsDto>.ErrorResult(
                    currentUserResponse.Message,
                    currentUserResponse.Message,
                    StatusCodes.Status401Unauthorized);

                return StatusCode(unauth.StatusCode, unauth);
            }

            long userId = currentUserResponse.Data;

            var res = await _smtpSettingsService.UpdateAsync(dto, userId);
            return StatusCode(res.StatusCode, res);
        }
    }
}
