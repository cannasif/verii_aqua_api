using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FishHealthEventConfiguration : BaseEntityConfiguration<FishHealthEvent>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FishHealthEvent> builder)
        {
            builder.ToTable("RII_FishHealthEvent");
            builder.Property(x => x.EventDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.Severity).HasMaxLength(40).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.AffectedRatioPct).HasPrecision(18, 3);
            builder.Property(x => x.VeterinarianName).HasMaxLength(150);
            builder.Property(x => x.Observation).HasMaxLength(2000);
            builder.Property(x => x.RecommendedAction).HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.FishHealthEvents)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.FishHealthEvents)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.FishHealthEvents)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
