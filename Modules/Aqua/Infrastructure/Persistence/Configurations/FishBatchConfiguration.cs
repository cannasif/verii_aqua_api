using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FishBatchConfiguration : BaseEntityConfiguration<FishBatch>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FishBatch> builder)
        {
            builder.ToTable("RII_FishBatch", table =>
            {
                table.HasCheckConstraint("CK_RII_FishBatch_CurrentAverageGram", "[CurrentAverageGram] > 0");
            });
            builder.Property(x => x.BatchCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CurrentAverageGram).HasPrecision(18, 3).IsRequired();
            builder.Property(x => x.StartDate).HasPrecision(3).IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.FishBatches)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishStock)
                .WithMany()
                .HasForeignKey(x => x.FishStockId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SourceGoodsReceiptLine)
                .WithMany()
                .HasForeignKey(x => x.SourceGoodsReceiptLineId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ProjectId, x.BatchCode })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_FishBatch_Project_BatchCode_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
