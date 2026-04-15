using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Warehouse.Infrastructure.Persistence.Configurations
{
    public class WarehouseConfiguration : BaseEntityConfiguration<WarehouseEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WarehouseEntity> builder)
        {
            builder.ToTable("RII_Warehouse");

            builder.Property(x => x.ErpWarehouseCode)
                .IsRequired();

            builder.Property(x => x.WarehouseName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CustomerCode)
                .HasMaxLength(25);

            builder.Property(x => x.BranchCode)
                .IsRequired();

            builder.Property(x => x.IsLocked)
                .IsRequired();

            builder.Property(x => x.AllowNegativeBalance)
                .IsRequired();

            builder.Property(x => x.LastSyncedAt)
                .IsRequired(false);

            builder.HasIndex(x => new { x.ErpWarehouseCode, x.BranchCode })
                .IsUnique()
                .HasDatabaseName("UX_RII_Warehouse_ErpWarehouseCode_BranchCode");

            builder.HasIndex(x => x.WarehouseName)
                .HasDatabaseName("IX_RII_Warehouse_WarehouseName");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
