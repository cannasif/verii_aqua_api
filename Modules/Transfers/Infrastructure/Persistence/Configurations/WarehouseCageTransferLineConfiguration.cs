using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Transfers.Infrastructure.Persistence.Configurations
{
    public class WarehouseCageTransferLineConfiguration : BaseEntityConfiguration<WarehouseCageTransferLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WarehouseCageTransferLine> builder)
        {
            builder.ToTable("RII_WAREHOUSE_CAGE_TRANSFER_LINE", table =>
            {
                table.HasCheckConstraint("CK_RII_WAREHOUSE_CAGE_TRANSFER_LINE_POSITIVE", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
            });

            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);

            builder.HasOne(x => x.WarehouseCageTransfer)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.WarehouseCageTransferId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FromWarehouse)
                .WithMany()
                .HasForeignKey(x => x.FromWarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ToProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ToProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
