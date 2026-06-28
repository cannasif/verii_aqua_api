using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Shipments.Infrastructure.Persistence.Configurations
{
    public class ShipmentLineConfiguration : BaseEntityConfiguration<ShipmentLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ShipmentLine> builder)
        {
            builder.ToTable("RII_SHIPMENT_LINE", table =>
            {
                table.HasCheckConstraint("CK_RII_SHIPMENT_LINE_POSITIVE", "[FishCount] > 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0");
            });
            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);
            builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 6);
            builder.Property(x => x.LocalUnitPrice).HasPrecision(18, 6);
            builder.Property(x => x.LineAmount).HasPrecision(18, 6);
            builder.Property(x => x.LocalLineAmount).HasPrecision(18, 6);
            builder.Property(x => x.ErpSourceMovementKey).HasMaxLength(300);

            builder.HasOne(x => x.Shipment)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.ShipmentLines)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FromProjectCage)
                .WithMany()
                .HasForeignKey(x => x.FromProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.ErpSourceMovementKey)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [ErpSourceMovementKey] IS NOT NULL")
                .HasDatabaseName("UX_RII_SHIPMENT_LINE_ERP_SOURCE_MOVEMENT_KEY_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
