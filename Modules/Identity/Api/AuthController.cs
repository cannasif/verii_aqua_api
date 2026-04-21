using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using aqua_api.Shared.Host.WebApi.Hubs;

namespace aqua_api.Modules.Identity.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IHubContext<AuthHub> _hubContext;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IPermissionAccessService _permissionAccessService;

        public AuthController(
            IHubContext<AuthHub> hubContext,
            ILocalizationService localizationService,
            IAuthService authService,
            IUserService userService,
            IPermissionAccessService permissionAccessService)
        {
            _hubContext = hubContext;
            _localizationService = localizationService;
            _authService = authService;
            _userService = userService;
            _permissionAccessService = permissionAccessService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginWithSessionResponseDto>>> Login([FromBody] LoginRequest request)
        {
            var loginDto = new LoginDto
            {
                Username = request.Email,
                Password = request.Password,
                RememberMe = request.RememberMe
            };

            var loginResult = await _authService.LoginWithSessionAsync(loginDto);

            if (loginResult.Success && loginResult.Data != null)
            {
                await AuthHub.ForceLogoutUser(_hubContext, loginResult.Data.UserId.ToString());
                return StatusCode(loginResult.StatusCode, loginResult);
            }

            return StatusCode(loginResult.StatusCode, ApiResponse<LoginWithSessionResponseDto>.ErrorResult(
                loginResult.Message,
                loginResult.ExceptionMessage,
                loginResult.StatusCode));
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401, ApiResponse<UserDto>.ErrorResult(
                    _localizationService.GetLocalizedString("AuthService.UserIdNotFound"),
                    _localizationService.GetLocalizedString("General.Unauthorized"),
                    401));
            }

            var result = await _userService.GetUserProfileAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<ActionResult<ApiResponse<MyPermissionsDto>>> GetMyPermissions()
        {
            var result = await _permissionAccessService.GetMyPermissionsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("request-password-reset")]
        public async Task<ActionResult<ApiResponse<string>>> RequestPasswordReset([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.RequestPasswordResetAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authService.ChangePasswordAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<LoginWithSessionResponseDto>>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return StatusCode(result.StatusCode, result);
        }

    }
}
