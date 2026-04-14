namespace aqua_api.Modules.Integrations.Domain.Entities
{
    public class SmtpSetting : BaseEntity
    {
        // EntityBase: Id, CreatedAt, UpdatedAt, vs. (sende ne varsa)

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string Username { get; set; } = string.Empty;

        // Şifreyi DB’de şifreli saklayacağız
        public string PasswordEncrypted { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "V3RII CRM SYSTEM";

        public int Timeout { get; set; } = 30;
    }
}