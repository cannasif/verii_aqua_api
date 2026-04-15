using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FishLabResultConfiguration : BaseEntityConfiguration<FishLabResult>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FishLabResult> builder)
        {
            builder.ToTable("RII_FishLabResult");
            builder.Property(x => x.ResultDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.ResultType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.PathogenName).HasMaxLength(120);
            builder.Property(x => x.ResultValue).HasMaxLength(120);
            builder.Property(x => x.Unit).HasMaxLength(30);
            builder.Property(x => x.Interpretation).HasMaxLength(1000);
            builder.Property(x => x.RecommendedAction).HasMaxLength(1000);

            builder.HasOne(x => x.FishLabSample)
                .WithMany(x => x.Results)
                .HasForeignKey(x => x.FishLabSampleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
