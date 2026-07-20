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
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_LINE_MARKET_TYPE", "[MarketType] IN (0, 1)");
        });

        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SourceUnitPrice).HasColumnType("decimal(18,6)");

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.SalesLines)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.BudgetPlanFishBatchId, x.Year, x.Month, x.MarketType })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique()
            .HasDatabaseName("IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
