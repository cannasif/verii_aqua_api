using Hangfire;
using Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Helpers;
using aqua_api.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundJobs
{
    [Queue("dead-letter")]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 0, LogEvents = true)]
    public class HangfireDeadLetterJob : IHangfireDeadLetterJob
    {
        private readonly IMailService _mailService;
        private readonly ILogger<HangfireDeadLetterJob> _logger;
        private readonly HangfireMonitoringOptions _options;

        public HangfireDeadLetterJob(
            IMailService mailService,
            ILogger<HangfireDeadLetterJob> logger,
            IOptions<HangfireMonitoringOptions> options)
        {
            _mailService = mailService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task ProcessAsync(HangfireDeadLetterPayload payload)
        {
            _logger.LogCritical(
                "Dead-letter Hangfire job captured. JobId: {JobId}, Job: {JobName}, Queue: {Queue}, RetryCount: {RetryCount}, Reason: {Reason}, Exception: {ExceptionType} - {ExceptionMessage}",
                payload.JobId,
                payload.JobName,
                payload.Queue,
                payload.RetryCount,
                payload.Reason,
                payload.ExceptionType,
                payload.ExceptionMessage);

            if (_options.AlertEmails == null || _options.AlertEmails.Count == 0)
            {
                return;
            }

            var subject = $"[AQUA][CRITICAL][Hangfire] Dead-letter: {payload.JobName}";
            var body = $@"
                <h3>Critical Aqua Hangfire Job Failed</h3>
                <p><b>JobId:</b> {payload.JobId}</p>
                <p><b>JobName:</b> {payload.JobName}</p>
                <p><b>Queue:</b> {payload.Queue}</p>
                <p><b>RetryCount:</b> {payload.RetryCount}</p>
                <p><b>Reason:</b> {payload.Reason}</p>
                <p><b>Exception:</b> {payload.ExceptionType}</p>
                <p><b>Message:</b> {payload.ExceptionMessage}</p>
                <p><b>OccurredAtUtc:</b> {payload.OccurredAtUtc:O}</p>";

            foreach (var email in _options.AlertEmails.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                try
                {
                    await _mailService.SendEmailAsync(email.Trim(), subject, body, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send dead-letter alert email to {Email}", email);
                }
            }
        }
    }
}
