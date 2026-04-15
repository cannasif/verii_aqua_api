using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WarehouseTransferLineConfiguration : BaseEntityConfiguration<WarehouseTransferLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WarehouseTransferLine> builder)
        {
            builder.ToTable("RII_WarehouseTransferLine", table =>
            {
                table.HasCheckConstraint("CK_RII_WarehouseTransferLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
                table.HasCheckConstraint("CK_RII_WarehouseTransferLine_FromToDiff", "[FromWarehouseId] <> [ToWarehouseId]");
            });

            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);

            builder.HasOne(x => x.WarehouseTransfer)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.WarehouseTransferId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
