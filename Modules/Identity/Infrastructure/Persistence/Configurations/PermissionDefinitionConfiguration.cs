using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class PermissionDefinitionConfiguration : BaseEntityConfiguration<PermissionDefinition>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PermissionDefinition> builder)
        {
            builder.ToTable("RII_PERMISSION_DEFINITIONS");

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasDatabaseName("IX_PermissionDefinitions_Code");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_PermissionDefinitions_IsDeleted");

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasData(
                new PermissionDefinition { Id = 1, Code = "dashboard.view", Name = "Dashboard View", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false, IsActive = true },
                new PermissionDefinition { Id = 2, Code = "customers.view", Name = "Customers View", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false, IsActive = true },
                new PermissionDefinition { Id = 3, Code = "salesmen360.view", Name = "Salesmen 360 View", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false, IsActive = true },
                new PermissionDefinition { Id = 4, Code = "customer360.view", Name = "Customer 360 View", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false, IsActive = true },
                new PermissionDefinition { Id = 5, Code = "powerbi.view", Name = "Power BI View", CreatedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false, IsActive = true }
            );
        }
    }
}
