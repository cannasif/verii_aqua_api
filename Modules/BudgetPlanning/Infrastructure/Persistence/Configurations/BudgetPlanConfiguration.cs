using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.BudgetPlanning.Infrastructure.Persistence.Configurations;

public class BudgetPlanConfiguration : BaseEntityConfiguration<BudgetPlan>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BudgetPlan> builder)
    {
        builder.ToTable("RII_BUDGET_Plan", table =>
        {
            table.HasCheckConstraint("CK_RII_BUDGET_Plan_Status", "[Status] IN (0,1,2,3,4,5,6,7)");
            table.HasCheckConstraint("CK_RII_BUDGET_Plan_StartMonth", "[StartMonth] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RII_BUDGET_Plan_EndMonth", "[EndMonth] BETWEEN 1 AND 12");
        });

        builder.Property(x => x.BudgetNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BudgetCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BudgetName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.BudgetNo)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_Plan_BudgetNo_Active");

        builder.HasIndex(x => x.BudgetCode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_BUDGET_Plan_BudgetCode_Active");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
