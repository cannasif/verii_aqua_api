using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanSalesLineConfiguration : BaseEntityConfiguration<BudgetPlanSalesLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanSalesLine> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_SALES_LINE", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_LINE_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_LINE_NON_NEGATIVE", "[SalesTon] >= 0");
        });

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.SalesLines)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.BudgetPlanFishBatchId, x.Year, x.Month })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
