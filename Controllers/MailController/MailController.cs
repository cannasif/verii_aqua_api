using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using aqua_api.DTOs;
using aqua_api.DTOs.MailDto;
using aqua_api.Interfaces;
using aqua_api.UnitOfWork;
using Hangfire;
using Infrastructure.BackgroundJobs.Interfaces;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly ILogger<MailController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;
        private readonly ISmtpSettingsService _smtpSettingsService;

        public MailController(
            IMailService mailService,
            ILogger<MailController> logger,
            IUnitOfWork unitOfWork,
            ILocalizationService localizationService,
            ISmtpSettingsService smtpSettingsService)
        {
            _mailService = mailService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
            _smtpSettingsService = smtpSettingsService;
        }

        /// <summary>
        /// Send email immediately (synchronous)
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendMailDto dto)
        {
            try
            {
                var result = await _mailService.SendEmailAsync(
                    dto.To,
                    dto.Subject,
                    dto.Body,
                    dto.FromEmail,
                    dto.FromName,
                    dto.IsHtml,
                    dto.Cc,
                    dto.Bcc,
                    dto.Attachments
                );

                if (result)
                {
                    return Ok(new { message = _localizationService.GetLocalizedString("General.EmailSentSuccessfully"), success = true });
                }

                return BadRequest(new { message = _localizationService.GetLocalizedString("General.EmailSendFailed"), success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                return StatusCode(500, new { message = _localizationService.GetLocalizedString("General.EmailSendFailed"), error = ex.Message });
            }
        }

        [HttpPost("send-test")]
        public async Task<ActionResult<ApiResponse<bool>>> SendTest([FromBody] SendTestMailDto dto)
        {
            try
            {
                SmtpSettingsRuntimeDto smtp;
                try
                {
                    smtp = await _smtpSettingsService.GetRuntimeAsync();
                }
                catch (InvalidOperationException)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsMissing"),
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsMissing"),
                            StatusCodes.Status400BadRequest));
                }
                catch (CryptographicException)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsCannotDecryptPassword"),
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsCannotDecryptPassword"),
                            StatusCodes.Status400BadRequest));
                }
                catch (Exception ex) when (ex.Message.Contains("key ring", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsCannotDecryptPassword"),
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsCannotDecryptPassword"),
                            StatusCodes.Status400BadRequest));
                }

                if (string.IsNullOrWhiteSpace(smtp.Host) ||
                    string.IsNullOrWhiteSpace(smtp.Username) ||
                    string.IsNullOrWhiteSpace(smtp.Password) ||
                    string.IsNullOrWhiteSpace(smtp.FromEmail))
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsIncomplete"),
                            _localizationService.GetLocalizedString("MailController.SmtpSettingsIncomplete"),
                            StatusCodes.Status400BadRequest));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    var unauth = ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("General.Unauthorized"),
                        _localizationService.GetLocalizedString("General.Unauthorized"),
                        StatusCodes.Status401Unauthorized);
                    return StatusCode(unauth.StatusCode, unauth);
                }

                var to = dto.To;
                if (string.IsNullOrWhiteSpace(to))
                {
                    var user = await _unitOfWork.Users.Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

                    if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    {
                        var notFound = ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("UserService.UserNotFound"),
                            _localizationService.GetLocalizedString("UserService.UserNotFound"),
                            StatusCodes.Status404NotFound);
                        return StatusCode(notFound.StatusCode, notFound);
                    }

                    to = user.Email;
                }

                var subject = _localizationService.GetLocalizedString("MailController.SmtpTestMailSubject");
                var body = _localizationService.GetLocalizedString("MailController.SmtpTestMailBody", DateTime.UtcNow.ToString("O"));

                var ok = await _mailService.SendEmailAsync(to, subject, body, false, null, null, null);
                if (!ok)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        ApiResponse<bool>.ErrorResult(
                            _localizationService.GetLocalizedString("MailController.TestMailSendFailed"),
                            _localizationService.GetLocalizedString("MailController.TestMailSendFailed"),
                            StatusCodes.Status400BadRequest));
                }

                return StatusCode(
                    StatusCodes.Status200OK,
                    ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("General.OperationSuccessful")));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("General.InternalServerError"),
                        ex.Message,
                        StatusCodes.Status500InternalServerError));
            }
        }

        /// <summary>
        /// Send email via Hangfire background job (asynchronous)
        /// </summary>
        [HttpPost("send-async")]
        public IActionResult SendEmailAsync([FromBody] SendMailDto dto)
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<IMailJob>(job =>
                    job.SendEmailWithAttachmentsAsync(
                        dto.To,
                        dto.Subject,
                        dto.Body,
                        dto.FromEmail,
                        dto.FromName,
                        dto.IsHtml,
                        dto.Cc,
                        dto.Bcc,
                        dto.Attachments
                    )
                );

                return Ok(new
                {
                    message = _localizationService.GetLocalizedString("General.EmailQueuedSuccessfully"),
                    success = true,
                    jobId = jobId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing email");
                return StatusCode(500, new { message = _localizationService.GetLocalizedString("General.EmailQueueFailed"), error = ex.Message });
            }
        }

        /// <summary>
        /// Send email to multiple recipients via Hangfire background job
        /// </summary>
        [HttpPost("send-bulk")]
        public IActionResult SendBulkEmail([FromBody] BulkSendMailDto dto)
        {
            try
            {
                var jobIds = new List<string>();

                foreach (var recipient in dto.To)
                {
                    var jobId = BackgroundJob.Enqueue<IMailJob>(job =>
                        job.SendEmailWithAttachmentsAsync(
                            recipient,
                            dto.Subject,
                            dto.Body,
                            dto.FromEmail,
                            dto.FromName,
                            dto.IsHtml,
                            dto.Cc,
                            dto.Bcc,
                            dto.Attachments
                        )
                    );
                    jobIds.Add(jobId);
                }

                return Ok(new
                {
                    message = _localizationService.GetLocalizedString("General.BulkEmailsQueuedSuccessfully", dto.To.Count),
                    success = true,
                    jobIds = jobIds,
                    count = jobIds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing bulk emails");
                return StatusCode(500, new { message = _localizationService.GetLocalizedString("General.BulkEmailQueueFailed"), error = ex.Message });
            }
        }

        /// <summary>
        /// Schedule email to be sent at a specific time
        /// </summary>
        [HttpPost("schedule")]
        public IActionResult ScheduleEmail([FromBody] SendMailDto dto, [FromQuery] DateTime scheduleAt)
        {
            try
            {
                if (scheduleAt <= DateTime.UtcNow)
                {
                    return BadRequest(new { message = _localizationService.GetLocalizedString("General.ScheduleTimeMustBeFuture"), success = false });
                }

                var delay = scheduleAt - DateTime.UtcNow;
                var jobId = BackgroundJob.Schedule<IMailJob>(job =>
                    job.SendEmailWithAttachmentsAsync(
                        dto.To,
                        dto.Subject,
                        dto.Body,
                        dto.FromEmail,
                        dto.FromName,
                        dto.IsHtml,
                        dto.Cc,
                        dto.Bcc,
                        dto.Attachments
                    ),
                    delay
                );

                return Ok(new
                {
                    message = _localizationService.GetLocalizedString("General.EmailScheduledSuccessfully"),
                    success = true,
                    jobId = jobId,
                    scheduledAt = scheduleAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling email");
                return StatusCode(500, new { message = _localizationService.GetLocalizedString("General.EmailScheduleFailed"), error = ex.Message });
            }
        }
    }
}
