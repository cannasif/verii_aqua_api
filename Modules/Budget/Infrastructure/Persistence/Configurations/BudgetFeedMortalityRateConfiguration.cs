using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations;

public class BudgetFeedMortalityRateConfiguration : BaseEntityConfiguration<BudgetFeedMortalityRate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetFeedMortalityRate> builder)
    {
        builder.ToTable("RII_BUDGET_FEED_MORTALITY_RATE", table =>
            table.HasCheckConstraint("CK_RII_BUDGET_FEED_MORTALITY_RATE_PERCENT", "[ReductionRatePercent] >= 0 AND [ReductionRatePercent] <= 100"));
        builder.Property(x => x.ReductionRatePercent).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasOne(x => x.WaterTemperature).WithMany().HasForeignKey(x => x.WaterTemperatureId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CalibrationDefinition).WithMany().HasForeignKey(x => x.CalibrationDefinitionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FeedStock).WithMany().HasForeignKey(x => x.FeedStockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.WaterTemperatureId, x.CalibrationDefinitionId, x.FeedStockId })
            .IsUnique().HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_FEED_MORTALITY_RATE_COMBINATION_ACTIVE");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
