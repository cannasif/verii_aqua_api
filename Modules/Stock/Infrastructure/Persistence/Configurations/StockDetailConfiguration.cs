using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Stock.Infrastructure.Persistence.Configurations
{
    public class StockDetailConfiguration : BaseEntityConfiguration<StockDetail>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockDetail> builder)
        {
            // Table name
            builder.ToTable("RII_STOCK_DETAIL");

            // Stock relationship
            builder.Property(e => e.StockId)
                .IsRequired();

            builder.HasOne(e => e.Stock)
                .WithOne(s => s.StockDetail)
                .HasForeignKey<StockDetail>(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            // HTML Description
            builder.Property(e => e.HtmlDescription)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Technical Specs JSON
            builder.Property(e => e.TechnicalSpecsJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Indexes
            builder.HasIndex(e => e.StockId)
                .IsUnique()
                .HasDatabaseName("IX_StockDetail_StockId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_StockDetail_IsDeleted");

            // Global Query Filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
