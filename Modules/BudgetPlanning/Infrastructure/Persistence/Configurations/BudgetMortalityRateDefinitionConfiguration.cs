using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetMortalityRateDefinitionConfiguration : BaseEntityConfiguration<BudgetMortalityRateDefinition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetMortalityRateDefinition> builder)
    {
        builder.ToTable("RII_BUDGET_MORTALITY_RATE_DEFINITION", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RATE", "[MortalityRatePercent] >= 0 AND [MortalityRatePercent] <= 100");
        });

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.FishStock)
            .WithMany()
            .HasForeignKey(x => x.FishStockId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CalibrationDefinition)
            .WithMany()
            .HasForeignKey(x => x.CalibrationDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.FishStockId, x.CalibrationDefinitionId, x.GrowthMonthNo })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_KEY_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
