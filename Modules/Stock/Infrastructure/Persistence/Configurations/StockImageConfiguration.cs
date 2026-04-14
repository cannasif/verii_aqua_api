using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Stock.Infrastructure.Persistence.Configurations
{
    public class StockImageConfiguration : BaseEntityConfiguration<StockImage>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockImage> builder)
        {
            // Table name
            builder.ToTable("RII_STOCK_IMAGE");

            // Stock relationship
            builder.Property(e => e.StockId)
                .IsRequired();

            builder.HasOne(e => e.Stock)
                .WithMany(s => s.StockImages)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            // File Path
            builder.Property(e => e.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            // Alt Text
            builder.Property(e => e.AltText)
                .HasMaxLength(200)
                .IsRequired(false);

            // Sort Order
            builder.Property(e => e.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Is Primary
            builder.Property(e => e.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(e => e.StockId)
                .HasDatabaseName("IX_StockImage_StockId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_StockImage_IsDeleted");

            // Global Query Filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
