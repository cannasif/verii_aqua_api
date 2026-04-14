using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Stock.Infrastructure.Persistence.Configurations
{
    public class StockRelationConfiguration : BaseEntityConfiguration<StockRelation>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockRelation> builder)
        {
            // Table name
            builder.ToTable("RII_STOCK_RELATION");

            // Stock relationship
            builder.Property(e => e.StockId)
                .IsRequired();

            builder.HasOne(e => e.Stock)
                .WithMany(s => s.ParentRelations)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Related Stock relationship
            builder.Property(e => e.RelatedStockId)
                .IsRequired();

            builder.HasOne(e => e.RelatedStock)
                .WithMany()
                .HasForeignKey(e => e.RelatedStockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quantity
            builder.Property(e => e.Quantity)
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            // Description
            builder.Property(e => e.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            // Is Mandatory
            builder.Property(e => e.IsMandatory)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(e => e.StockId)
                .HasDatabaseName("IX_StockRelation_StockId");

            builder.HasIndex(e => e.RelatedStockId)
                .HasDatabaseName("IX_StockRelation_RelatedStockId");

            builder.HasIndex(e => new { e.StockId, e.RelatedStockId })
                .HasDatabaseName("IX_StockRelation_StockId_RelatedStockId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_StockRelation_IsDeleted");

            // Global Query Filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
