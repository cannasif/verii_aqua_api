using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanFishBatchConfiguration : BaseEntityConfiguration<BudgetPlanFishBatch>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanFishBatch> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN_FISH_BATCH", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FISH_BATCH_SOURCE_TYPE", "[SourceType] IN (0,1)");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FISH_BATCH_NON_NEGATIVE", "[InitialLiveCount] >= 0 AND [InitialAverageGram] >= 0 AND [InitialBiomassKg] >= 0");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_FISH_BATCH_GROWTH_START_MONTH", "[GrowthStartMonth] BETWEEN 1 AND 12");
        });

        builder.Property(x => x.SourceType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.BatchCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.InitialUnitCost).HasPrecision(18, 6);
        builder.Property(x => x.InitialSmmAmount).HasPrecision(18, 6);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.FishBatches)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanProject)
            .WithMany(x => x.FishBatches)
            .HasForeignKey(x => x.BudgetPlanProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceFishBatch)
            .WithMany()
            .HasForeignKey(x => x.SourceFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.FishStock)
            .WithMany()
            .HasForeignKey(x => x.FishStockId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.BatchCode })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_RII_BUDGET_PLAN_FISH_BATCH_BATCH_CODE_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
