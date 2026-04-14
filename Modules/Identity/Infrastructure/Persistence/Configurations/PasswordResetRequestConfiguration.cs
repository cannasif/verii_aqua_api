using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class PasswordResetRequestConfiguration : BaseEntityConfiguration<PasswordResetRequest>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PasswordResetRequest> builder)
        {
            builder.ToTable("RII_PASSWORD_RESET_REQUEST");

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.UsedAt)
                .IsRequired(false);
        }
    }
}
