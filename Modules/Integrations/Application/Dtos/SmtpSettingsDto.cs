using System;
using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Integrations.Application.Dtos
{
    // GET /api/settings/smtp response
    public class SmtpSettingsDto
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        public int Timeout { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    // PUT /api/settings/smtp request
    public class UpdateSmtpSettingsDto
    {
        [Required]
        [MaxLength(200)]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        [MaxLength(200)]
        public string Username { get; set; } = string.Empty;

        // Password sadece update ile gelir, GET ile asla dönmez
        // boş gelirse backend eski password'u koruyacak
        [MaxLength(500)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(200)]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FromName { get; set; } = string.Empty;

        [Range(1, 300)]
        public int Timeout { get; set; } = 30;
    }

    // internal usage (mail sender)
    // front'a dönmeyecek
    public class SmtpSettingsRuntimeDto
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        public int Timeout { get; set; }
    }
}
