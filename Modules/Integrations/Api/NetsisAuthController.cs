using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Integrations.Api
{
    [ApiController]
    [Route("api/netsis-auth")]
    [Authorize]
    public sealed class NetsisAuthController : ControllerBase
    {
        private readonly INetsisAuthTokenService _tokenService;
        private readonly ILogger<NetsisAuthController> _logger;

        public NetsisAuthController(
            INetsisAuthTokenService tokenService,
            ILogger<NetsisAuthController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpGet("login")]
        public async Task<ActionResult<ApiResponse<NetsisTokenResultDto>>> Login(
            [FromQuery] string? branchCode,
            [FromQuery] bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    var badRequest = ApiResponse<NetsisTokenResultDto>.ErrorResult(
                        "Netsis login için şube kodu zorunludur.",
                        "branchCode query parametresi gönderilmelidir.",
                        StatusCodes.Status400BadRequest);

                    return BadRequest(badRequest);
                }

                HttpContext.Items["BranchCode"] = branchCode.Trim();
                var token = await _tokenService.NetsisGetTokenAsync(forceRefresh, cancellationToken).ConfigureAwait(false);
                var message = token.Source == "memory"
                    ? "Netsis token memory cache üzerinden getirildi."
                    : "Netsis login başarılı.";

                return Ok(ApiResponse<NetsisTokenResultDto>.SuccessResult(token, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Netsis login check failed.");
                var message = string.IsNullOrWhiteSpace(ex.Message)
                    ? "Netsis bağlantısı test edilemedi."
                    : ex.Message;

                var response = ApiResponse<NetsisTokenResultDto>.ErrorResult(
                    message,
                    message,
                    StatusCodes.Status400BadRequest);

                return StatusCode(response.StatusCode, response);
            }
        }
    }
}
