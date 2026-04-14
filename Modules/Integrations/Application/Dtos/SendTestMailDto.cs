using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class SendTestMailDto
    {
        // Optional. If empty, email is sent to the current logged-in user's email.
        [EmailAddress(ErrorMessage = "Validation.InvalidEmailAddress")]
        public string? To { get; set; }
    }
}
