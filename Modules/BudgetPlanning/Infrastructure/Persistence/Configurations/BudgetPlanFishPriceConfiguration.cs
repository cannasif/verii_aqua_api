using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanFishPriceConfiguration : BaseEntityConfiguration<BudgetPlanFishPrice>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanFishPrice> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_FISH_PRICE", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FISH_PRICE_MONTH", "[Month] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE", "[UnitPriceEuro] >= 0");
        });

        builder.Property(x => x.UnitPriceEuro).HasPrecision(18, 6);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.FishPrices)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CalibrationDefinition)
            .WithMany()
            .HasForeignKey(x => x.CalibrationDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.CalibrationDefinitionId, x.Year, x.Month })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
