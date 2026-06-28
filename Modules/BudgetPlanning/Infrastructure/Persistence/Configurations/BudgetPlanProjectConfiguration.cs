using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanProjectConfiguration : BaseEntityConfiguration<BudgetPlanProject>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlanProject> builder)
    {
        builder.ToTable("RII_BUDGET_PlanProject", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PlanProject_SourceType", "[SourceType] IN (0,1)");
        });

        builder.Property(x => x.SourceType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.ProjectCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProjectName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");

        builder.HasOne(x => x.BudgetPlan)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.BudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourceProject)
            .WithMany()
            .HasForeignKey(x => x.SourceProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BudgetPlanId, x.ProjectCode })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_RII_BUDGET_PlanProject_ProjectCode_Active");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
