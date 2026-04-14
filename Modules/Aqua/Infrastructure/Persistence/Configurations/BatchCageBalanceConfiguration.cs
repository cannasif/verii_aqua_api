using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class BatchCageBalanceConfiguration : BaseEntityConfiguration<BatchCageBalance>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BatchCageBalance> builder)
        {
            builder.ToTable("RII_BatchCageBalance", table =>
            {
                table.HasCheckConstraint("CK_RII_BatchCageBalance_NonNegative", "[LiveCount] >= 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0");
            });
            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);
            builder.Property(x => x.AsOfDate).HasPrecision(3);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.BatchCageBalances)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.BatchCageBalances)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.FishBatchId, x.ProjectCageId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BatchCageBalance_BatchCage_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
