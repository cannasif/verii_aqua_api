namespace aqua_api.Modules.Integrations.Application.Services
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null);
        Task<bool> SendEmailAsync(string to, string subject, string body, string? fromEmail = null, string? fromName = null, bool isHtml = true, string? cc = null, string? bcc = null, List<string>? attachments = null);
    }
}
