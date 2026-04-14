using System;

namespace aqua_api.Modules.Identity.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public long RoleId { get; set; } = 0;

        public UserAuthority? RoleNavigation { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;

        public DateTime? LastLoginDate { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}".Trim();

        // Navigation properties
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }
}