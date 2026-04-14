using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class UserSessionConfiguration : BaseEntityConfiguration<UserSession>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("RII_USER_SESSION");

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.Property(e => e.Token)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.RevokedAt)
                .IsRequired(false);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(e => e.DeviceInfo)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UserSession_UserId");

            builder.HasIndex(e => e.SessionId)
                .IsUnique()
                .HasDatabaseName("IX_UserSession_SessionId");

            builder.HasIndex(e => e.RevokedAt)
                .HasDatabaseName("IX_UserSession_RevokedAt");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_UserSession_IsDeleted");

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
