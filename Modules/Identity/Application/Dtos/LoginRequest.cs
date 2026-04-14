using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Identity.Application.Dtos
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty; // Email veya Username olarak kullanılacak

        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>true: token localStorage; false: token sessionStorage (frontend)</summary>
        public bool RememberMe { get; set; } = false;
    }
}
