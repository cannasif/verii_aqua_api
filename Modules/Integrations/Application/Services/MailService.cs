using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace aqua_api.Modules.Integrations.Application.Services
{
    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly ISmtpSettingsService _smtpSettingsService;

        public MailService(ISmtpSettingsService smtpSettingsService, ILogger<MailService> logger)
        {
            _smtpSettingsService = smtpSettingsService;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true,
            string? cc = null,
            string? bcc = null,
            List<string>? attachments = null)
        {
            return await SendEmailAsync(to, subject, body, null, null, isHtml, cc, bcc, attachments);
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            string? fromEmail = null,
            string? fromName = null,
            bool isHtml = true,
            string? cc = null,
            string? bcc = null,
            List<string>? attachments = null)
        {
            try
            {
                var smtp = await _smtpSettingsService.GetRuntimeAsync();

                if (string.IsNullOrWhiteSpace(smtp.Host))
                {
                    _logger.LogError("SMTP Host is not configured (DB).");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(smtp.Username) || string.IsNullOrWhiteSpace(smtp.Password))
                {
                    _logger.LogError("SMTP Username or Password is not configured (DB).");
                    return false;
                }

                using var client = new SmtpClient(smtp.Host, smtp.Port)
                {
                    EnableSsl = smtp.EnableSsl,
                    Credentials = new NetworkCredential(smtp.Username, smtp.Password),
                    Timeout = smtp.Timeout * 1000
                };

                using var message = new MailMessage();

                var resolvedFromEmail = !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : smtp.FromEmail;
                var resolvedFromName = !string.IsNullOrWhiteSpace(fromName) ? fromName : smtp.FromName;

                if (string.IsNullOrWhiteSpace(resolvedFromEmail))
                {
                    _logger.LogError("FromEmail is not configured (DB).");
                    return false;
                }

                message.From = new MailAddress(resolvedFromEmail, resolvedFromName);

                message.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                    message.CC.Add(cc);

                if (!string.IsNullOrWhiteSpace(bcc))
                    message.Bcc.Add(bcc);

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                if (attachments != null && attachments.Any())
                {
                    foreach (var attachmentPath in attachments)
                    {
                        if (File.Exists(attachmentPath))
                            message.Attachments.Add(new Attachment(attachmentPath));
                        else
                            _logger.LogWarning($"Attachment file not found: {attachmentPath}");
                    }
                }

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to} with subject: {subject}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}. Error: {ex.Message}");
                return false;
            }
        }
    }
}
