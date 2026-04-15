using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class GoodsReceiptLineConfiguration : BaseEntityConfiguration<GoodsReceiptLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<GoodsReceiptLine> builder)
        {
            builder.ToTable("RII_GoodsReceiptLine", table =>
            {
                table.HasCheckConstraint("CK_RII_GoodsReceiptLine_ItemType", "[ItemType] IN (0,1)");
            });
            builder.Property(x => x.ItemType).HasConversion<byte>().IsRequired();
            builder.Property(x => x.QtyUnit).HasPrecision(18, 3);
            builder.Property(x => x.GramPerUnit).HasPrecision(18, 3);
            builder.Property(x => x.TotalGram).HasPrecision(18, 3);
            builder.Property(x => x.FishAverageGram).HasPrecision(18, 3);
            builder.Property(x => x.FishTotalGram).HasPrecision(18, 3);
            builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 6);
            builder.Property(x => x.LocalUnitPrice).HasPrecision(18, 6);
            builder.Property(x => x.LineAmount).HasPrecision(18, 6);
            builder.Property(x => x.LocalLineAmount).HasPrecision(18, 6);

            builder.HasOne(x => x.GoodsReceipt)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.GoodsReceiptId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Stock)
                .WithMany()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.GoodsReceiptLines)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
