using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WelfareAssessmentConfiguration : BaseEntityConfiguration<WelfareAssessment>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WelfareAssessment> builder)
        {
            builder.ToTable("RII_WelfareAssessment");
            builder.Property(x => x.AssessmentDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.WelfareScore).HasPrecision(18, 3).IsRequired();
            builder.Property(x => x.StockingDensityKgM3).HasPrecision(18, 3);
            builder.Property(x => x.AppetiteScore).HasPrecision(18, 3);
            builder.Property(x => x.BehaviorScore).HasPrecision(18, 3);
            builder.Property(x => x.GillScore).HasPrecision(18, 3);
            builder.Property(x => x.SkinScore).HasPrecision(18, 3);
            builder.Property(x => x.FinScore).HasPrecision(18, 3);
            builder.Property(x => x.AssessedBy).HasMaxLength(150);
            builder.Property(x => x.Observation).HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.WelfareAssessments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.WelfareAssessments)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.WelfareAssessments)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
