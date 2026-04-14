using System;
namespace aqua_api.Modules.Identity.Domain.Entities
{
    public class UserDetail : BaseEntity
    {
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Kullanıcı resmi URL
        public string? ProfilePictureUrl { get; set; }

        // Boy (cm cinsinden)
        public decimal? Height { get; set; }

        // Kilo (kg cinsinden)
        public decimal? Weight { get; set; }

        // Açıklama
        public string? Description { get; set; }

        // Cinsiyet
        public Gender? Gender { get; set; }
    }
}