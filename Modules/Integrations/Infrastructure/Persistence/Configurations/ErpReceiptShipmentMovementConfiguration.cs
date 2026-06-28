using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Integrations.Infrastructure.Persistence.Configurations
{
    public class ErpReceiptShipmentMovementConfiguration : BaseEntityConfiguration<ErpReceiptShipmentMovement>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ErpReceiptShipmentMovement> builder)
        {
            builder.ToTable("RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");

            builder.Property(x => x.SourceSystem)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.SourceMovementKey)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.MovementDate)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(x => x.DocumentNo)
                .HasMaxLength(15)
                .IsRequired(false);

            builder.Property(x => x.ErpProjectCode)
                .HasMaxLength(15)
                .IsRequired(false);

            builder.Property(x => x.ErpStockCode)
                .HasMaxLength(35)
                .IsRequired();

            builder.Property(x => x.ErpStockName)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.Quantity)
                .HasColumnType("decimal(28,8)")
                .IsRequired();

            builder.Property(x => x.MovementKind)
                .HasMaxLength(1)
                .IsRequired();

            builder.Property(x => x.InOutCode)
                .HasMaxLength(1)
                .IsRequired();

            builder.Property(x => x.StockGroupCode)
                .HasMaxLength(8)
                .IsRequired(false);

            builder.Property(x => x.OperationType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.LastSyncedAt)
                .HasPrecision(3)
                .IsRequired();

            builder.Property(x => x.MatchedAt)
                .HasPrecision(3)
                .IsRequired(false);

            builder.Property(x => x.ProcessedAt)
                .HasPrecision(3)
                .IsRequired(false);

            builder.Property(x => x.MatchError)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.ProcessError)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(x => x.IsMatched)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.ProcessingAttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Cage)
                .WithMany()
                .HasForeignKey(x => x.CageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Stock)
                .WithMany()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.GoodsReceipt)
                .WithMany()
                .HasForeignKey(x => x.GoodsReceiptId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.GoodsReceiptLine)
                .WithMany()
                .HasForeignKey(x => x.GoodsReceiptLineId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Shipment)
                .WithMany()
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ShipmentLine)
                .WithMany()
                .HasForeignKey(x => x.ShipmentLineId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.BatchMovement)
                .WithMany()
                .HasForeignKey(x => x.BatchMovementId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.SourceMovementKey)
                .IsUnique()
                .HasDatabaseName("UX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_SOURCE_MOVEMENT_KEY");

            builder.HasIndex(x => new { x.ErpProjectCode, x.ErpWarehouseCode, x.MovementDate })
                .HasDatabaseName("IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_PROJECT_WAREHOUSE_DATE");

            builder.HasIndex(x => new { x.IsMatched, x.IsProcessed })
                .HasDatabaseName("IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_PROCESS_STATE");

            builder.HasIndex(x => new { x.GoodsReceiptId, x.GoodsReceiptLineId })
                .HasDatabaseName("IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_GOODS_RECEIPT");

            builder.HasIndex(x => new { x.ShipmentId, x.ShipmentLineId })
                .HasDatabaseName("IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_SHIPMENT");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
