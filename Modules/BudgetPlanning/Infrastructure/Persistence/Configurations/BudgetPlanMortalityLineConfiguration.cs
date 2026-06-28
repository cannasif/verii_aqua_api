using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanMortalityLineConfiguration : BaseEntityConfiguration<BudgetPlanMortalityLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanMortalityLine> builder)
    {
        builder.ToTable("RII_BUDGET_PlanMortalityLine", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PlanMortalityLine_Month", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PlanMortalityLine_NonNegative", "[MortalityRatePercent] >= 0 AND [MortalityCount] >= 0 AND [MortalityKg] >= 0");
        });

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.MortalityLines)
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

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
