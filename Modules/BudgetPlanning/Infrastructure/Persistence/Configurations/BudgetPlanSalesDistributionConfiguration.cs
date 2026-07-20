using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanSalesDistributionConfiguration : BaseEntityConfiguration<BudgetPlanSalesDistribution>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanSalesDistribution> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_SALES_DISTRIBUTION", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_MARKET_TYPE", "[MarketType] IN (0, 1)");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_NON_NEGATIVE", "[SalesTon] >= 0 AND [SalesKg] >= 0 AND [SalesCount] >= 0");
        });

        builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SalesTon).HasColumnType("decimal(18,6)");
        builder.Property(x => x.SalesKg).HasColumnType("decimal(18,6)");
        builder.Property(x => x.UnitGram).HasColumnType("decimal(18,6)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,6)");
        builder.Property(x => x.UnitPriceEuro).HasColumnType("decimal(18,6)");
        builder.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
        builder.Property(x => x.Amount).HasColumnType("decimal(18,6)");
        builder.Property(x => x.AmountEuro).HasColumnType("decimal(18,6)");
        builder.Property(x => x.AmountTry).HasColumnType("decimal(18,6)");

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.SalesDistributions)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanMonthlyProjection)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanMonthlyProjectionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.BudgetPlanSalesLine)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanSalesLineId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany()
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CalibrationDefinition)
            .WithMany()
            .HasForeignKey(x => x.CalibrationDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new
            {
                x.BudgetPlanId,
                x.BudgetPlanFishBatchId,
                x.Year,
                x.Month,
                x.MarketType,
                x.CalibrationDefinitionId
            })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_DIMENSION_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
