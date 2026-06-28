using aqua_api.Modules.Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations
{
    public class BudgetWaterTemperatureConfiguration : BaseEntityConfiguration<BudgetWaterTemperature>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BudgetWaterTemperature> builder)
        {
            builder.ToTable("RII_BUDGET_WATER_TEMPERATURE");

            builder.Property(x => x.Year).IsRequired();
            builder.Property(x => x.Month).IsRequired();
            builder.Property(x => x.WaterTemperatureCelsius).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => new { x.Year, x.Month })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BUDGET_WATER_TEMPERATURE_YEAR_MONTH_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
