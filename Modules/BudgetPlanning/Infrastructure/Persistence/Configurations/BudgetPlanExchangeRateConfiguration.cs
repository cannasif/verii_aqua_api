using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanExchangeRateConfiguration : BaseEntityConfiguration<BudgetPlanExchangeRate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanExchangeRate> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_EXCHANGE_RATE", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_EXCHANGE_RATE_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_EXCHANGE_RATE_RATE", "[ExchangeRate] >= 0");
        });

        builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.RateType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SourceReference).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.ExchangeRates)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BudgetPlanId, x.Year, x.Month, x.CurrencyCode, x.RateType })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_PLAN_EXCHANGE_RATE_PERIOD_CURRENCY");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
