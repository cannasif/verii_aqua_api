using Hangfire;
using Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public class MailJob : IMailJob
    {
        private readonly IMailService _mailService;
        private readonly ILogger<MailJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILocalizationService _localizationService;

        public MailJob(IMailService mailService, ILogger<MailJob> logger, IConfiguration configuration, ILocalizationService localizationService)
        {
            _mailService = mailService;
            _logger = logger;
            _configuration = configuration;
            _localizationService = localizationService;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null)
        {
            try
            {
                _logger.LogInformation($"MailJob: Sending email to {to} with subject: {subject}");
                
                var result = await _mailService.SendEmailAsync(to, subject, body, isHtml, cc, bcc, attachments);
                
                if (result)
                {
                    _logger.LogInformation($"MailJob: Email sent successfully to {to}");
                }
                else
                {
                    _logger.LogWarning($"MailJob: Failed to send email to {to}");
                    throw new Exception(_localizationService.GetLocalizedString("MailJob.EmailSendFailedForRecipient", to));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MailJob: Error sending email to {to}");
                throw;
            }
        }

        public async Task SendEmailWithAttachmentsAsync(string to, string subject, string body, string? fromEmail = null, string? fromName = null, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null)
        {
            try
            {
                _logger.LogInformation($"MailJob: Sending email to {to} with subject: {subject}");
                
                var result = await _mailService.SendEmailAsync(to, subject, body, fromEmail, fromName, isHtml, cc, bcc, attachments);
                
                if (result)
                {
                    _logger.LogInformation($"MailJob: Email sent successfully to {to}");
                }
                else
                {
                    _logger.LogWarning($"MailJob: Failed to send email to {to}");
                    throw new Exception(_localizationService.GetLocalizedString("MailJob.EmailSendFailedForRecipient", to));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MailJob: Error sending email to {to}");
                throw;
            }
        }

        public async Task SendUserCreatedEmailAsync(string email, string username, string password, string? firstName, string? lastName, string baseUrl)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var emailSubject = _localizationService.GetLocalizedString("MailJob.UserCreatedSubject");
            var displayName = string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) 
                ? username 
                : $"{firstName} {lastName}".Trim();

            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.UserCreatedIntro")}</p>
                <div class=""info-box"">
                    <p><strong>{_localizationService.GetLocalizedString("MailJob.UserCreatedLoginEmailLabel")}</strong> {email}</p>
                    <p><strong>{_localizationService.GetLocalizedString("MailJob.UserCreatedPasswordLabel")}</strong> {password}</p>
                </div>
                <p>{_localizationService.GetLocalizedString("MailJob.UserCreatedPasswordChangeInfo")}</p>
                <div style=""text-align: center; margin-top: 30px;"">
                    <a href=""{effectiveBaseUrl}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.LoginButton")}</a>
                </div>";

            var emailBody = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.UserCreatedTitle"), content);
            await SendEmailAsync(email, emailSubject, emailBody, true);
        }

        public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink, string emailSubject)
        {
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", fullName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.ResetPasswordIntro")}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{resetLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ResetPasswordButton")}</a>
                </div>
                <p>{_localizationService.GetLocalizedString("MailJob.ResetPasswordLinkCopyInfo")}</p>
                <p style=""word-break: break-all; color: #fb923c; font-size: 14px;"">{resetLink}</p>
                <div style=""margin-top: 20px; padding-top: 20px; border-top: 1px solid rgba(255,255,255,0.1);"">
                    <p style=""font-size: 13px; color: #94a3b8; margin: 0;"">{_localizationService.GetLocalizedString("MailJob.ResetPasswordLinkValidFor30Minutes")}</p>
                    <p style=""font-size: 13px; color: #94a3b8; margin: 5px 0 0 0;"">{_localizationService.GetLocalizedString("MailJob.ResetPasswordIgnoreIfNotRequested")}</p>
                </div>";

            var emailBody = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.ResetPasswordTitle"), content);
            await SendEmailAsync(email, emailSubject, emailBody, true);
        }

        public async Task SendPasswordChangedEmailAsync(string email, string displayName, string baseUrl)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var emailSubject = _localizationService.GetLocalizedString("MailJob.PasswordChangedSubject");
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.PasswordChangedIntro")}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.PasswordChangedContinueSecurely")}</p>
                <div style=""text-align: center; margin-top: 30px;"">
                    <a href=""{effectiveBaseUrl}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.LoginButton")}</a>
                </div>";

            var emailBody = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.PasswordChangedTitle"), content);
            await SendEmailAsync(email, emailSubject, emailBody, true);
        }

        public async Task SendPasswordResetCompletedEmailAsync(string email, string displayName, string baseUrl)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var emailSubject = _localizationService.GetLocalizedString("MailJob.PasswordResetCompletedSubject");
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.PasswordResetCompletedIntro")}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.PasswordResetCompletedLoginInfo")}</p>
                <div style=""text-align: center; margin-top: 30px;"">
                    <a href=""{effectiveBaseUrl}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.LoginButton")}</a>
                </div>";

            var emailBody = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.PasswordResetCompletedTitle"), content);
            await SendEmailAsync(email, emailSubject, emailBody, true);
        }

        public async Task SendDemandApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string demandLink)
        {
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.DemandApprovalPendingIntro")}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{approvalLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ApproveRejectButton")}</a>
                    <a href=""{demandLink}"" class=""btn btn-secondary"" style=""margin-left: 10px;"">{_localizationService.GetLocalizedString("MailJob.GoToDemandButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.DemandApprovalPendingTitle"), content);
            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendDemandApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string demandPath,
            long demandId)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var effectiveApprovalPath = GetFrontendPath("ApprovalPendingPath", "approvals/pending");
            var effectiveDemandPath = GetFrontendPath("DemandDetailPath", "demands");
            var subject = _localizationService.GetLocalizedString("MailJob.ApprovalPendingSubject");
            var demandLink = $"{effectiveBaseUrl}/{effectiveDemandPath}/{demandId}";

            foreach (var (email, fullName, uid) in usersToNotify)
            {
                var displayName = string.IsNullOrWhiteSpace(fullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : fullName;
                var actionId = userIdToActionId.GetValueOrDefault(uid);
                var approvalLink = actionId != 0
                    ? $"{effectiveBaseUrl}/{effectiveApprovalPath}?actionId={actionId}"
                    : $"{effectiveBaseUrl}/{effectiveApprovalPath}";

                await SendDemandApprovalPendingEmailAsync(
                    email,
                    displayName,
                    subject,
                    approvalLink,
                    demandLink);
            }
        }

        public async Task SendOrderApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string orderLink)
        {
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.OrderApprovalPendingIntro")}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{approvalLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ApproveRejectButton")}</a>
                    <a href=""{orderLink}"" class=""btn btn-secondary"" style=""margin-left: 10px;"">{_localizationService.GetLocalizedString("MailJob.GoToOrderButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.OrderApprovalPendingTitle"), content);
            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendBulkOrderApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string orderPath,
            long orderId)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var effectiveApprovalPath = GetFrontendPath("ApprovalPendingPath", "approvals/pending");
            var effectiveOrderPath = GetFrontendPath("OrderDetailPath", "orders");
            var subject = _localizationService.GetLocalizedString("MailJob.ApprovalPendingSubject");
            var orderLink = $"{effectiveBaseUrl}/{effectiveOrderPath}/{orderId}";

            foreach (var (email, fullName, uid) in usersToNotify)
            {
                var displayName = string.IsNullOrWhiteSpace(fullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : fullName;
                var actionId = userIdToActionId.GetValueOrDefault(uid);
                var approvalLink = actionId != 0
                    ? $"{effectiveBaseUrl}/{effectiveApprovalPath}?actionId={actionId}"
                    : $"{effectiveBaseUrl}/{effectiveApprovalPath}";

                await SendOrderApprovalPendingEmailAsync(
                    email,
                    displayName,
                    subject,
                    approvalLink,
                    orderLink);
            }
        }

        public async Task SendQuotationApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string quotationLink)
        {
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.QuotationApprovalPendingIntro")}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{approvalLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ApproveRejectButton")}</a>
                    <a href=""{quotationLink}"" class=""btn btn-secondary"" style=""margin-left: 10px;"">{_localizationService.GetLocalizedString("MailJob.GoToQuotationButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.QuotationApprovalPendingTitle"), content);
            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendBulkQuotationApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string quotationPath,
            long quotationId)
        {
            var effectiveBaseUrl = GetFrontendBaseUrl();
            var effectiveApprovalPath = GetFrontendPath("ApprovalPendingPath", "approvals/pending");
            var effectiveQuotationPath = GetFrontendPath("QuotationDetailPath", "quotations");
            var subject = _localizationService.GetLocalizedString("MailJob.ApprovalPendingSubject");
            var quotationLink = $"{effectiveBaseUrl}/{effectiveQuotationPath}/{quotationId}";

            foreach (var (email, fullName, uid) in usersToNotify)
            {
                var displayName = string.IsNullOrWhiteSpace(fullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : fullName;
                var actionId = userIdToActionId.GetValueOrDefault(uid);
                var approvalLink = actionId != 0
                    ? $"{effectiveBaseUrl}/{effectiveApprovalPath}?actionId={actionId}"
                    : $"{effectiveBaseUrl}/{effectiveApprovalPath}";

                await SendQuotationApprovalPendingEmailAsync(
                    email,
                    displayName,
                    subject,
                    approvalLink,
                    quotationLink);
            }
        }

        public async Task SendQuotationApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string quotationNo,
            string quotationLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.QuotationApprovedSubject", quotationNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.QuotationApprovedIntro", quotationNo, approverFullName)}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{quotationLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewQuotationButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.QuotationApprovedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        public async Task SendQuotationRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string quotationNo,
            string rejectReason,
            string quotationLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.QuotationRejectedSubject", quotationNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.QuotationRejectedIntro", quotationNo, rejectorFullName)}</p>
                <div class=""info-box"">
                    <p><strong>{_localizationService.GetLocalizedString("MailJob.RejectionReasonLabel")}</strong> {rejectReason}</p>
                </div>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{quotationLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewQuotationButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.QuotationRejectedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        public async Task SendDemandApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string demandNo,
            string demandLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.DemandApprovedSubject", demandNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.DemandApprovedIntro", demandNo, approverFullName)}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{demandLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewDemandButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.DemandApprovedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        public async Task SendDemandRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string demandNo,
            string rejectReason,
            string demandLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.DemandRejectedSubject", demandNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.DemandRejectedIntro", demandNo, rejectorFullName)}</p>
                <div class=""info-box"">
                    <p><strong>{_localizationService.GetLocalizedString("MailJob.RejectionReasonLabel")}</strong> {rejectReason}</p>
                </div>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{demandLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewDemandButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.DemandRejectedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        public async Task SendOrderApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string orderNo,
            string orderLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.OrderApprovedSubject", orderNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.OrderApprovedIntro", orderNo, approverFullName)}</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{orderLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewOrderButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.OrderApprovedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        public async Task SendOrderRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string orderNo,
            string rejectReason,
            string orderLink)
        {
            var subject = _localizationService.GetLocalizedString("MailJob.OrderRejectedSubject", orderNo);
            var displayName = string.IsNullOrWhiteSpace(creatorFullName) ? _localizationService.GetLocalizedString("MailJob.DefaultDisplayName") : creatorFullName;
            
            var content = $@"
                <p>{_localizationService.GetLocalizedString("MailJob.DearUser", displayName)}</p>
                <p>{_localizationService.GetLocalizedString("MailJob.OrderRejectedIntro", orderNo, rejectorFullName)}</p>
                <div class=""info-box"">
                    <p><strong>{_localizationService.GetLocalizedString("MailJob.RejectionReasonLabel")}</strong> {rejectReason}</p>
                </div>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{orderLink}"" class=""btn"">{_localizationService.GetLocalizedString("MailJob.ViewOrderButton")}</a>
                </div>";

            var body = GetEmailTemplate(_localizationService.GetLocalizedString("MailJob.OrderRejectedTitle"), content);
            await SendEmailAsync(creatorEmail, subject, body, true);
        }

        private string GetFrontendBaseUrl()
        {
            return _configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        }

        private string GetFrontendPath(string key, string fallback)
        {
            return _configuration[$"FrontendSettings:{key}"]?.Trim('/') ?? fallback;
        }

        private string GetEmailTemplate(string title, string content)
        {
            var year = DateTime.Now.Year;
            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<link href=""https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap"" rel=""stylesheet"">
<style>
    body {{ font-family: 'Outfit', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0f0518; margin: 0; padding: 0; color: #ffffff; }}
    .wrapper {{ width: 100%; table-layout: fixed; background-color: #0f0518; padding-bottom: 40px; }}
    .container {{ max-width: 600px; margin: 0 auto; background-color: #140a1e; border-radius: 24px; border: 1px solid rgba(255,255,255,0.1); overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.4); }}
    .header {{ padding: 40px 40px 20px 40px; text-align: center; background: radial-gradient(circle at 50% -20%, rgba(236, 72, 153, 0.15), transparent 70%); }}
    .header h2 {{ margin: 0; font-size: 24px; font-weight: 700; color: #ffffff; text-transform: uppercase; letter-spacing: 1px; }}
    .content {{ padding: 20px 40px 40px 40px; color: #e2e8f0; line-height: 1.6; font-size: 16px; }}
    .footer {{ padding: 20px; text-align: center; color: #64748b; font-size: 12px; border-top: 1px solid rgba(255,255,255,0.05); background-color: #0c0516; }}
    .btn {{ display: inline-block; padding: 14px 32px; color: #ffffff !important; text-decoration: none; border-radius: 12px; font-weight: bold; text-transform: uppercase; letter-spacing: 1px; margin: 10px 5px; background: #f97316; background: linear-gradient(90deg, #db2777, #f97316, #eab308); box-shadow: 0 4px 15px rgba(249, 115, 22, 0.3); transition: all 0.3s ease; }}
    .btn:hover {{ opacity: 0.9; transform: translateY(-2px); box-shadow: 0 6px 20px rgba(249, 115, 22, 0.4); }}
    .btn-secondary {{ background: transparent; border: 1px solid rgba(255,255,255,0.2); color: #e2e8f0 !important; box-shadow: none; }}
    .btn-secondary:hover {{ background: rgba(255,255,255,0.05); border-color: rgba(255,255,255,0.4); }}
    .info-box {{ background-color: rgba(0,0,0,0.3); padding: 20px; border-radius: 12px; margin: 20px 0; border: 1px solid rgba(255,255,255,0.1); }}
    strong {{ color: #fb923c; }}
    a {{ color: #fb923c; text-decoration: none; }}
    p {{ margin-bottom: 15px; }}
</style>
</head>
<body>
    <div class=""wrapper"">
        <br>
        <div class=""container"">
            <div class=""header"">
                <h2>{title}</h2>
            </div>
            <div class=""content"">
                {content}
            </div>
            <div class=""footer"">
                <p>{_localizationService.GetLocalizedString("MailJob.AutomaticEmailFooter")}</p>
                <p>&copy; {year} {_localizationService.GetLocalizedString("MailJob.BrandName")}</p>
            </div>
        </div>
        <br>
    </div>
</body>
</html>";
        }
    }
}
