using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.GoodsReceipts.Infrastructure.Persistence.Configurations
{
    public class GoodsReceiptConfiguration : BaseEntityConfiguration<GoodsReceipt>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<GoodsReceipt> builder)
        {
            builder.ToTable("RII_GOODS_RECEIPT", table =>
            {
                table.HasCheckConstraint("CK_RII_GOODS_RECEIPT_STATUS", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.ReceiptNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ReceiptDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.WarehouseId).IsRequired(false);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.GoodsReceipts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.ReceiptNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_GOODS_RECEIPT_RECEIPT_NO_ACTIVE");

            builder.HasIndex(x => x.ProjectId)
                .HasDatabaseName("UX_RII_GOODS_RECEIPT_PROJECT_ACTIVE");

            builder.HasIndex(x => x.WarehouseId)
                .HasDatabaseName("IX_RII_GOODS_RECEIPT_WAREHOUSE_ID");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
