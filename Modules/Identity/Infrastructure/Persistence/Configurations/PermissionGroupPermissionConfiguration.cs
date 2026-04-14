using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class PermissionGroupPermissionConfiguration : BaseEntityConfiguration<PermissionGroupPermission>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PermissionGroupPermission> builder)
        {
            builder.ToTable("RII_PERMISSION_GROUP_PERMISSIONS");

            builder.HasIndex(x => new { x.PermissionGroupId, x.PermissionDefinitionId })
                .IsUnique()
                .HasDatabaseName("IX_PermissionGroupPermission_GroupId_DefinitionId");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_PermissionGroupPermission_IsDeleted");

            builder.HasOne(x => x.PermissionGroup)
                .WithMany(x => x.GroupPermissions)
                .HasForeignKey(x => x.PermissionGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.PermissionDefinition)
                .WithMany(x => x.GroupPermissions)
                .HasForeignKey(x => x.PermissionDefinitionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
