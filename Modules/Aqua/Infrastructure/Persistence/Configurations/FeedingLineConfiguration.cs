using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FeedingLineConfiguration : BaseEntityConfiguration<FeedingLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FeedingLine> builder)
        {
            builder.ToTable("RII_FeedingLine", table =>
            {
                table.HasCheckConstraint("CK_RII_FeedingLine_Positive", "[QtyUnit] > 0 AND [GramPerUnit] > 0 AND [TotalGram] > 0");
            });
            builder.Property(x => x.QtyUnit).HasPrecision(18, 3);
            builder.Property(x => x.GramPerUnit).HasPrecision(18, 3);
            builder.Property(x => x.TotalGram).HasPrecision(18, 3);

            builder.HasOne(x => x.Feeding)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.FeedingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Stock)
                .WithMany()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
