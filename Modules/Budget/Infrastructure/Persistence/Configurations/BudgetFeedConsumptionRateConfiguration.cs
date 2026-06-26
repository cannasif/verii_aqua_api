using aqua_api.Modules.Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations
{
    public class BudgetFeedConsumptionRateConfiguration : BaseEntityConfiguration<BudgetFeedConsumptionRate>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BudgetFeedConsumptionRate> builder)
        {
            builder.ToTable("RII_BUDGET_FEED_CONSUMPTION_RATE");

            builder.Property(x => x.WaterTemperatureId).IsRequired();
            builder.Property(x => x.CalibrationDefinitionId).IsRequired();
            builder.Property(x => x.FeedStockId).IsRequired();
            builder.Property(x => x.FeedAmount).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasOne(x => x.WaterTemperature)
                .WithMany()
                .HasForeignKey(x => x.WaterTemperatureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CalibrationDefinition)
                .WithMany()
                .HasForeignKey(x => x.CalibrationDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FeedStock)
                .WithMany()
                .HasForeignKey(x => x.FeedStockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WaterTemperatureId, x.CalibrationDefinitionId, x.FeedStockId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BUDGET_FEED_CONSUMPTION_RATE_Combination_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
