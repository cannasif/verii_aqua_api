namespace aqua_api.Modules.Identity.Domain.Entities
{
    public class PasswordResetRequest : BaseEntity
    {
        public long UserId { get; set; }
        public User? User { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }
    }
}