using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.FishGrowths.Infrastructure.Persistence.Configurations;

public class FishGrowthConfiguration : BaseEntityConfiguration<FishGrowth>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FishGrowth> builder)
    {
        builder.ToTable("RII_FISH_GROWTH", table =>
        {
            table.HasCheckConstraint("CK_RII_FISH_GROWTH_MONTH", "[GrowthMonth] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_FISH_GROWTH_VALUES", "[FishCount] > 0 AND [PreviousAverageGram] > 0 AND [GrowthGram] > 0 AND [NewAverageGram] > [PreviousAverageGram] AND [NewBiomassGram] > [PreviousBiomassGram]");
        });

        builder.Property(x => x.GrowthDate).HasPrecision(3).IsRequired();
        builder.Property(x => x.PreviousAverageGram).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.GrowthGram).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.NewAverageGram).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.PreviousBiomassGram).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.NewBiomassGram).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ProjectCage).WithMany().HasForeignKey(x => x.ProjectCageId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.FishBatch).WithMany().HasForeignKey(x => x.FishBatchId).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ProjectCageId, x.FishBatchId, x.GrowthYear, x.GrowthMonth })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_FISH_GROWTH_CAGE_BATCH_PERIOD_ACTIVE");

        builder.HasIndex(x => new { x.ProjectId, x.GrowthDate })
            .HasDatabaseName("IX_RII_FISH_GROWTH_PROJECT_DATE");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
