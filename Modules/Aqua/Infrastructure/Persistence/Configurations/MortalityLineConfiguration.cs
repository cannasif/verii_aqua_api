using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class MortalityLineConfiguration : BaseEntityConfiguration<MortalityLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<MortalityLine> builder)
        {
            builder.ToTable("RII_MortalityLine", table =>
            {
                table.HasCheckConstraint("CK_RII_MortalityLine_DeadCount", "[DeadCount] > 0");
            });

            builder.HasOne(x => x.Mortality)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.MortalityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.MortalityLines)
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
