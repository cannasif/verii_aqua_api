using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class UserAuthorityConfiguration : BaseEntityConfiguration<UserAuthority>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserAuthority> builder)
        {
            // Table name
            builder.ToTable("RII_USER_AUTHORITY");

            // Specific properties
            builder.Property(e => e.Title)
                .HasMaxLength(30)
                .IsRequired();

            // Indexes
            builder.HasIndex(e => e.Title)
                .IsUnique()
                .HasDatabaseName("IX_UserAuthority_Title");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_UserAuthority_IsDeleted");

            // Soft delete filter
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Seed deterministic roles
            builder.HasData(
                new UserAuthority { Id = 1, Title = "User", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new UserAuthority { Id = 2, Title = "Manager", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new UserAuthority { Id = 3, Title = "Admin", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
            );
        }
    }
}
