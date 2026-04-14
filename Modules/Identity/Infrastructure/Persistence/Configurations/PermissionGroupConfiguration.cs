using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class PermissionGroupConfiguration : BaseEntityConfiguration<PermissionGroup>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PermissionGroup> builder)
        {
            builder.ToTable("RII_PERMISSION_GROUPS");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IsSystemAdmin)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_PermissionGroups_Name");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_PermissionGroups_IsDeleted");

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasData(
                new PermissionGroup
                {
                    Id = 1,
                    Name = "System Admin",
                    Description = "Full system access",
                    IsSystemAdmin = true,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                }
            );
        }
    }
}
