using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace aqua_api.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class UserPermissionGroupConfiguration : BaseEntityConfiguration<UserPermissionGroup>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserPermissionGroup> builder)
        {
            builder.ToTable("RII_USER_PERMISSION_GROUPS");

            builder.HasIndex(x => new { x.UserId, x.PermissionGroupId })
                .IsUnique()
                .HasDatabaseName("IX_UserPermissionGroup_UserId_GroupId");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName("IX_UserPermissionGroup_IsDeleted");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.PermissionGroup)
                .WithMany(x => x.UserGroups)
                .HasForeignKey(x => x.PermissionGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);

        }
    }
}
