using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FishLabSampleConfiguration : BaseEntityConfiguration<FishLabSample>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FishLabSample> builder)
        {
            builder.ToTable("RII_FishLabSample");
            builder.Property(x => x.SampleDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.SampleCode).HasMaxLength(80).IsRequired();
            builder.Property(x => x.SampleType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.LaboratoryName).HasMaxLength(150);
            builder.Property(x => x.RequestedBy).HasMaxLength(150);
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.FishLabSamples)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.FishLabSamples)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.FishLabSamples)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishHealthEvent)
                .WithMany()
                .HasForeignKey(x => x.FishHealthEventId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
