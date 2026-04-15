using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class CageWarehouseTransferLineConfiguration : BaseEntityConfiguration<CageWarehouseTransferLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CageWarehouseTransferLine> builder)
        {
            builder.ToTable("RII_CageWarehouseTransferLine", table =>
            {
                table.HasCheckConstraint("CK_RII_CageWarehouseTransferLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
            });

            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);

            builder.HasOne(x => x.CageWarehouseTransfer)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.CageWarehouseTransferId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FromProjectCage)
                .WithMany()
                .HasForeignKey(x => x.FromProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ToWarehouse)
                .WithMany()
                .HasForeignKey(x => x.ToWarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
