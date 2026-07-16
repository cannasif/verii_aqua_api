using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations;

public class BudgetFishGrowthQualityConfiguration : BaseEntityConfiguration<BudgetFishGrowthQuality>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetFishGrowthQuality> builder)
    {
        builder.ToTable("RII_BUDGET_FISH_GROWTH_QUALITY", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_FISH_GROWTH_QUALITY_MONTH", "[GrowthMonthNo] >= 1 AND [GrowthMonthNo] <= 120");
            table.HasCheckConstraint("CK_RII_BUDGET_FISH_GROWTH_QUALITY_PERCENT", "[QualityPercent] >= 0 AND [QualityPercent] <= 100");
        });
        builder.Property(x => x.QualityPercent).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasOne(x => x.FishStock).WithMany().HasForeignKey(x => x.FishStockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.FishStockId, x.GrowthMonthNo })
            .IsUnique().HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_FISH_GROWTH_QUALITY_STOCK_MONTH_ACTIVE");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
