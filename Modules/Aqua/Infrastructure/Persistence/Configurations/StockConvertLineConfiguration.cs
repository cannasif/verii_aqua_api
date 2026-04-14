using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class StockConvertLineConfiguration : BaseEntityConfiguration<StockConvertLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockConvertLine> builder)
        {
            builder.ToTable("RII_StockConvertLine", table =>
            {
                table.HasCheckConstraint("CK_RII_StockConvertLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [NewAverageGram] >= 0 AND [BiomassGram] > 0");
            });
            builder.Property(x => x.AverageGram).HasPrecision(18, 3);
            builder.Property(x => x.NewAverageGram).HasPrecision(18, 3);
            builder.Property(x => x.BiomassGram).HasPrecision(18, 3);

            builder.HasOne(x => x.StockConvert)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.StockConvertId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FromFishBatch)
                .WithMany(x => x.StockConvertFromLines)
                .HasForeignKey(x => x.FromFishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ToFishBatch)
                .WithMany(x => x.StockConvertToLines)
                .HasForeignKey(x => x.ToFishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FromProjectCage)
                .WithMany()
                .HasForeignKey(x => x.FromProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ToProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ToProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            // Legacy rows may not have increment gram; allow zero for historical compatibility.
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
