using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanMonthlyProjectionConfiguration : BaseEntityConfiguration<BudgetPlanMonthlyProjection>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanMonthlyProjection> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_MONTHLY_PROJECTION", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_NON_NEGATIVE", "[OpeningLiveCount] >= 0 AND [ClosingLiveCount] >= 0 AND [OpeningBiomassKg] >= 0 AND [ClosingBiomassKg] >= 0");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_ADJUSTMENT_PERCENT", "[GrowthQualityPercent] >= 0 AND [GrowthQualityPercent] <= 100 AND [FeedMortalityReductionPercent] >= 0 AND [FeedMortalityReductionPercent] <= 100");
        });

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.MonthlyProjections)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CalibrationDefinition)
            .WithMany()
            .HasForeignKey(x => x.CalibrationDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.WaterTemperature)
            .WithMany()
            .HasForeignKey(x => x.WaterTemperatureId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.GrowthQualityPercent)
            .HasDefaultValue(100m);

        builder.HasIndex(x => new { x.BudgetPlanId, x.BudgetPlanFishBatchId, x.Year, x.Month })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_PERIOD_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
