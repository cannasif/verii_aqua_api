using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class UserDetailConfiguration : BaseEntityConfiguration<UserDetail>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserDetail> builder)
        {
            // Table name
            builder.ToTable("RII_USER_DETAIL");

            // Properties configuration
            builder.Property(u => u.UserId)
                .IsRequired();

            builder.Property(u => u.ProfilePictureUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(u => u.Height)
                .IsRequired(false)
                .HasColumnType("decimal(5,2)");

            builder.Property(u => u.Weight)
                .IsRequired(false)
                .HasColumnType("decimal(5,2)");

            builder.Property(u => u.Description)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(u => u.Gender)
                .IsRequired(false)
                .HasConversion<byte>();

            // Relationships
            builder.HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(u => u.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserDetail_UserId");

            builder.HasIndex(u => u.IsDeleted)
                .HasDatabaseName("IX_UserDetail_IsDeleted");

            // Global query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
