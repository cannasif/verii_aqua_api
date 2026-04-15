using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ProjectCageDailyKpiSnapshotConfiguration : BaseEntityConfiguration<ProjectCageDailyKpiSnapshot>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectCageDailyKpiSnapshot> builder)
        {
            builder.ToTable("RII_ProjectCageDailyKpiSnapshot");
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.SnapshotDate).IsRequired();
            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassKg).HasPrecision(18, 3);
            builder.Property(x => x.FeedKgPeriod).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGainKgPeriod).HasPrecision(18, 3);
            builder.Property(x => x.SurvivalPct).HasPrecision(9, 2);
            builder.Property(x => x.MortalityPctPeriod).HasPrecision(9, 2);
            builder.Property(x => x.Fcr).HasPrecision(18, 4);
            builder.Property(x => x.Adg).HasPrecision(18, 4);
            builder.Property(x => x.Sgr).HasPrecision(18, 4);
            builder.Property(x => x.CapacityUsagePct).HasPrecision(9, 2);
            builder.Property(x => x.ForecastBiomassKg30Days).HasPrecision(18, 3);
            builder.Property(x => x.HarvestReadinessScore).HasPrecision(9, 2);
            builder.Property(x => x.DataQualityScore).HasPrecision(9, 2);
            builder.Property(x => x.FormulaNote).HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
