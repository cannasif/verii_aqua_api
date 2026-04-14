using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class AquaSettingConfiguration : BaseEntityConfiguration<AquaSetting>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<AquaSetting> builder)
        {
            builder.ToTable("RII_AquaSetting", table =>
            {
                table.HasCheckConstraint(
                    "CK_RII_AquaSetting_PartialTransferOccupiedCageMode",
                    "[PartialTransferOccupiedCageMode] IN (0,1,2)");
            });

            builder.Property(x => x.PartialTransferOccupiedCageMode)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.RequireFullTransfer)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.AllowProjectMerge)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
