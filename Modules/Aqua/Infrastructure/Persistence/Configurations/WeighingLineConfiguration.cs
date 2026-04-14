using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WeighingLineConfiguration : BaseEntityConfiguration<WeighingLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WeighingLine> builder)
        {
            builder.ToTable("RII_WeighingLine", table =>
            {
                table.HasCheckConstraint("CK_RII_WeighingLine_Positive", "[MeasuredCount] > 0 AND [MeasuredAverageGram] > 0 AND [MeasuredBiomassGram] > 0");
            });
            builder.Property(x => x.MeasuredAverageGram).HasPrecision(18, 3);
            builder.Property(x => x.MeasuredBiomassGram).HasPrecision(18, 3);

            builder.HasOne(x => x.Weighing)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.WeighingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.WeighingLines)
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
