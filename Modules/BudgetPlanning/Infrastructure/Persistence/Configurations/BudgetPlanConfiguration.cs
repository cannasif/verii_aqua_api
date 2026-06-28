using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanConfiguration : BaseEntityConfiguration<BudgetPlan>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlan> builder)
    {
        builder.ToTable("RII_BUDGET_PLAN", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_STATUS", "[Status] IN (0,1,2,3,4,5,6,7)");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_START_MONTH", "[StartMonth] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_PLAN_END_MONTH", "[EndMonth] BETWEEN 1 AND 12");
        });

        builder.Property(x => x.BudgetNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BudgetCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BudgetName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.BudgetNo)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_PLAN_BUDGET_NO_ACTIVE");

        builder.HasIndex(x => x.BudgetCode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_PLAN_BUDGET_CODE_ACTIVE");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
