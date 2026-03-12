using System.ComponentModel.DataAnnotations;

namespace aqua_api.DTOs.MailDto
{
    public class BulkSendMailDto
    {
        [Required(ErrorMessage = "Validation.ToEmailAddressesRequired")]
        [MinLength(1, ErrorMessage = "Validation.AtLeastOneRecipientRequired")]
        public List<string> To { get; set; } = new List<string>();

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
