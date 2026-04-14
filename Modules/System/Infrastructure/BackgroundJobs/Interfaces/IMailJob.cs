namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces
{
    public interface IMailJob
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null);
        Task SendEmailWithAttachmentsAsync(string to, string subject, string body, string? fromEmail = null, string? fromName = null, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null);
        Task SendUserCreatedEmailAsync(string email, string username, string password, string? firstName, string? lastName, string baseUrl);
        Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink, string emailSubject);
        Task SendPasswordResetCompletedEmailAsync(string email, string displayName, string baseUrl);
        Task SendPasswordChangedEmailAsync(string email, string displayName, string baseUrl);
        Task SendDemandApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string demandLink);
        Task SendDemandApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string demandPath,
            long demandId);

        Task SendOrderApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string orderLink);

        Task SendBulkOrderApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string orderPath,
            long orderId);

        Task SendQuotationApprovalPendingEmailAsync(string email, string displayName, string subject, string approvalLink, string quotationLink);

        Task SendBulkQuotationApprovalPendingEmailsAsync(
            List<(string Email, string FullName, long UserId)> usersToNotify,
            Dictionary<long, long> userIdToActionId,
            string baseUrl,
            string approvalPath,
            string quotationPath,
            long quotationId);

        Task SendQuotationApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string quotationNo,
            string quotationLink);

        Task SendQuotationRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string quotationNo,
            string rejectReason,
            string quotationLink);

        Task SendDemandApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string demandNo,
            string demandLink);

        Task SendDemandRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string demandNo,
            string rejectReason,
            string demandLink);

        Task SendOrderApprovedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string approverFullName,
            string orderNo,
            string orderLink);

        Task SendOrderRejectedEmailAsync(
            string creatorEmail,
            string creatorFullName,
            string rejectorFullName,
            string orderNo,
            string rejectReason,
            string orderLink);
    }
}
