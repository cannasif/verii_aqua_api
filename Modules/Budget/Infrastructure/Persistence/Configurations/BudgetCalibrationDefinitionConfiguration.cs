using aqua_api.Modules.Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations
{
    public class BudgetCalibrationDefinitionConfiguration : BaseEntityConfiguration<BudgetCalibrationDefinition>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BudgetCalibrationDefinition> builder)
        {
            builder.ToTable("RII_BUDGET_CALIBRATION_DEFINITION");

            builder.Property(x => x.CalibrationCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CalibrationInfo).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.CalibrationCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BUDGET_CALIBRATION_DEFINITION_CODE_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
