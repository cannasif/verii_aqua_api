using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ShipmentConfiguration : BaseEntityConfiguration<Shipment>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("RII_Shipment", table =>
            {
                table.HasCheckConstraint("CK_RII_Shipment_Status", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.ShipmentNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ShipmentDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.TargetWarehouse).HasMaxLength(100);
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.ShipmentNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_Shipment_ShipmentNo_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
