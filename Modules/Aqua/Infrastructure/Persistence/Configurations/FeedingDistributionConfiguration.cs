using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FeedingDistributionConfiguration : BaseEntityConfiguration<FeedingDistribution>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FeedingDistribution> builder)
        {
            builder.ToTable("RII_FeedingDistribution", table =>
            {
                table.HasCheckConstraint("CK_RII_FeedingDistribution_FeedGram", "[FeedGram] > 0");
            });
            builder.Property(x => x.FeedGram).HasPrecision(18, 3);

            builder.HasOne(x => x.FeedingLine)
                .WithMany(x => x.Distributions)
                .HasForeignKey(x => x.FeedingLineId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.FeedingDistributions)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
