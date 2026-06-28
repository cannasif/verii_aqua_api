using aqua_api.Modules.Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Budget.Infrastructure.Persistence.Configurations
{
    public class BudgetFishGrowthProfileConfiguration : BaseEntityConfiguration<BudgetFishGrowthProfile>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BudgetFishGrowthProfile> builder)
        {
            builder.ToTable("RII_BUDGET_FISH_GROWTH_PROFILE");

            builder.Property(x => x.StockId).IsRequired();
            builder.Property(x => x.StartMonth).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasOne(x => x.Stock)
                .WithMany()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Lines)
                .WithOne(x => x.BudgetFishGrowthProfile)
                .HasForeignKey(x => x.BudgetFishGrowthProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StockId, x.StartMonth })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_BUDGET_FISH_GROWTH_PROFILE_STOCK_START_MONTH_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
