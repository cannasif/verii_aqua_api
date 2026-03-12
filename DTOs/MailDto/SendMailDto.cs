using System.ComponentModel.DataAnnotations;

namespace aqua_api.DTOs.MailDto
{
    public class SendMailDto
    {
        [Required(ErrorMessage = "Validation.ToEmailAddressRequired")]
        [EmailAddress(ErrorMessage = "Validation.InvalidEmailAddress")]
        public string To { get; set; } = string.Empty;

        [Required(ErrorMessage = "Validation.SubjectRequired")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Validation.BodyRequired")]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;

        [EmailAddress(ErrorMessage = "Validation.InvalidCcEmailAddress")]
        public string? Cc { get; set; }

        [EmailAddress(ErrorMessage = "Validation.InvalidBccEmailAddress")]
        public string? Bcc { get; set; }

        public string? FromEmail { get; set; }

        public string? FromName { get; set; }

        public List<string>? Attachments { get; set; }
    }
}
