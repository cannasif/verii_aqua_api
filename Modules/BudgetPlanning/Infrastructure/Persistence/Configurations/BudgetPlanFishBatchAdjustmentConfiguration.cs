using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanFishBatchAdjustmentConfiguration : BaseEntityConfiguration<BudgetPlanFishBatchAdjustment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanFishBatchAdjustment> builder)
    {
        builder.ToTable("RII_BUDGET_PlanFishBatchAdjustment", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PlanFishBatchAdjustment_Type", "[AdjustmentType] IN (0,1,2,3)");
            table.HasCheckConstraint("CK_RII_BUDGET_PlanFishBatchAdjustment_Positive", "[LiveCount] > 0 AND [AverageGram] >= 0 AND [BiomassKg] >= 0");
        });

        builder.Property(x => x.AdjustmentType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.FishBatchAdjustments)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BudgetPlanFishBatch)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.BudgetPlanFishBatchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.BudgetPlanFishBatchId, x.Id })
            .HasDatabaseName("IX_RII_BUDGET_PlanFishBatchAdjustment_Batch");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
