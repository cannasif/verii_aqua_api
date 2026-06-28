using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanFeedingLineConfiguration : BaseEntityConfiguration<BudgetPlanFeedingLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanFeedingLine> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_FEEDING_LINE", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FEEDING_LINE_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FEEDING_LINE_NON_NEGATIVE", "[FeedAmountRate] >= 0 AND [FeedKg] >= 0");
        });

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.FeedingLines)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanMonthlyProjection)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanMonthlyProjectionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.FeedStock)
            .WithMany()
            .HasForeignKey(x => x.FeedStockId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
