using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Shipments.Infrastructure.Persistence.Configurations
{
    public class ShipmentConfiguration : BaseEntityConfiguration<Shipment>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("RII_SHIPMENT", table =>
            {
                table.HasCheckConstraint("CK_RII_SHIPMENT_STATUS", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.ShipmentNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ShipmentDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.TargetWarehouseId).IsRequired(false);
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.ConfigureErpPostableHeader();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.ShipmentNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_SHIPMENT_SHIPMENT_NO_ACTIVE");

            builder.HasIndex(x => x.TargetWarehouseId)
                .HasDatabaseName("IX_RII_SHIPMENT_TARGET_WAREHOUSE_ID");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
