using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ShipmentLineConfiguration : BaseEntityConfiguration<ShipmentLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ShipmentLine> builder)
        {
            builder.ToTable("RII_ShipmentLine", table =>
            {
                table.HasCheckConstraint("CK_RII_ShipmentLine_Positive", "[FishCount] > 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0");
            });
            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);

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

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
