using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Cages.Infrastructure.Persistence.Configurations
{
    public class CageWarehouseMappingConfiguration : BaseEntityConfiguration<CageWarehouseMapping>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CageWarehouseMapping> builder)
        {
            builder.ToTable("RII_CageWarehouseMapping");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.Note)
                .HasMaxLength(500);

            builder.HasOne(x => x.Cage)
                .WithMany(x => x.WarehouseMappings)
                .HasForeignKey(x => x.CageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.CageId)
                .HasFilter("[IsDeleted] = 0 AND [IsActive] = 1")
                .IsUnique()
                .HasDatabaseName("UX_RII_CageWarehouseMapping_Cage_Active");

            builder.HasIndex(x => x.WarehouseId)
                .HasDatabaseName("IX_RII_CageWarehouseMapping_WarehouseId");
        }
    }
}
