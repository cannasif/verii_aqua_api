using aqua_api.Modules.Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations
{
    public class BudgetFishGrowthProfileLineConfiguration : BaseEntityConfiguration<BudgetFishGrowthProfileLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BudgetFishGrowthProfileLine> builder)
        {
            builder.ToTable("RII_BUDGET_FISH_GROWTH_PROFILE_LINE");

            builder.Property(x => x.BudgetFishGrowthProfileId).IsRequired();
            builder.Property(x => x.GrowthMonthNo).IsRequired();
            builder.Property(x => x.CalendarMonth).IsRequired();
            // Netsis growth parameters arrive with up to eight decimal places.
            // Keeping that precision prevents cumulative gram/biomass drift
            // across long budget horizons.
            builder.Property(x => x.MonthlyGrowthGram).HasColumnType("decimal(18,8)").IsRequired();
            builder.Property(x => x.TotalGram).HasColumnType("decimal(18,8)").IsRequired();

            builder.HasIndex(x => new { x.BudgetFishGrowthProfileId, x.GrowthMonthNo })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_PROFILE_MONTH_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
