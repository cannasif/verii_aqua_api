using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BatchBalances.Infrastructure.Persistence.Configurations
{
    public class BatchWarehouseBalanceConfiguration : BaseEntityConfiguration<BatchWarehouseBalance>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BatchWarehouseBalance> builder)
        {
            builder.ToTable("RII_BATCH_WAREHOUSE_BALANCE", table =>
            {
                table.HasCheckConstraint("CK_RII_BATCH_WAREHOUSE_BALANCE_NON_NEGATIVE", "[LiveCount] >= 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0");
            });

            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);
            builder.Property(x => x.AsOfDate).HasPrecision(3);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.BatchWarehouseBalances)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ProjectId, x.FishBatchId, x.WarehouseId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BATCH_WAREHOUSE_BALANCE_PROJECT_BATCH_WAREHOUSE_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
