using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.GoodsReceipts.Infrastructure.Persistence.Configurations
{
    public class GoodsReceiptFishDistributionConfiguration : BaseEntityConfiguration<GoodsReceiptFishDistribution>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<GoodsReceiptFishDistribution> builder)
        {
            builder.ToTable("RII_GOODS_RECEIPT_FISH_DISTRIBUTION", table =>
            {
                table.HasCheckConstraint("CK_RII_GOODS_RECEIPT_FISH_DISTRIBUTION_COUNT", "[FishCount] > 0");
            });

            builder.HasOne(x => x.GoodsReceiptLine)
                .WithMany(x => x.FishDistributions)
                .HasForeignKey(x => x.GoodsReceiptLineId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.GoodsReceiptFishDistributions)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.GoodsReceiptLineId, x.ProjectCageId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_GOODS_RECEIPT_FISH_DISTRIBUTION_LINE_CAGE_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
