using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using aqua_api.Models;

namespace aqua_api.Data.Configurations
{
    public class StockConfiguration : BaseEntityConfiguration<Stock>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Stock> builder)
        {
            // Table name
            builder.ToTable("RII_STOCK");

            // ERP Stock Code
            builder.Property(e => e.ErpStockCode)
                .HasMaxLength(50)
                .IsRequired();

            // Stock Name
            builder.Property(e => e.StockName)
                .HasMaxLength(250)
                .IsRequired();

            // Unit
            builder.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsRequired(false);

            // UreticiKodu
            builder.Property(e => e.UreticiKodu)
                .HasMaxLength(50)
                .IsRequired(false);

            // GrupKodu
            builder.Property(e => e.GrupKodu)
                .HasMaxLength(50)
                .IsRequired(false);

            // GrupAdi
            builder.Property(e => e.GrupAdi)
                .HasMaxLength(250)
                .IsRequired(false);

            // Kod1
            builder.Property(e => e.Kod1)
                .HasMaxLength(50)
                .IsRequired(false);

            // Kod1Adi
            builder.Property(e => e.Kod1Adi)
                .HasMaxLength(250)
                .IsRequired(false);

            // Kod2
            builder.Property(e => e.Kod2)
                .HasMaxLength(50)
                .IsRequired(false);

            // Kod2Adi
            builder.Property(e => e.Kod2Adi)
                .HasMaxLength(250)
                .IsRequired(false);

            // Kod3
            builder.Property(e => e.Kod3)
                .HasMaxLength(50)
                .IsRequired(false);

            // Kod3Adi
            builder.Property(e => e.Kod3Adi)
                .HasMaxLength(250)
                .IsRequired(false);

            // Kod4
            builder.Property(e => e.Kod4)
                .HasMaxLength(50)
                .IsRequired(false);

            // Kod4Adi
            builder.Property(e => e.Kod4Adi)
                .HasMaxLength(250)
                .IsRequired(false);

            // Kod5
            builder.Property(e => e.Kod5)
                .HasMaxLength(50)
                .IsRequired(false);

            // Kod5Adi
            builder.Property(e => e.Kod5Adi)
                .HasMaxLength(250)
                .IsRequired(false);

            // BranchCode
            builder.Property(e => e.BranchCode)
                .IsRequired()
                .HasDefaultValue(0);

            // Navigation Properties
            builder.HasOne(e => e.StockDetail)
                .WithOne(d => d.Stock)
                .HasForeignKey<StockDetail>(d => d.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.StockImages)
                .WithOne(i => i.Stock)
                .HasForeignKey(i => i.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.ParentRelations)
                .WithOne(r => r.Stock)
                .HasForeignKey(r => r.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes (composite for sync lookup by ErpStockCode + BranchCode)
            builder.HasIndex(e => e.ErpStockCode)
                .HasDatabaseName("IX_Stock_ErpStockCode");
            builder.HasIndex(e => new { e.ErpStockCode, e.BranchCode })
                .HasDatabaseName("IX_Stock_ErpStockCode_BranchCode");

            builder.HasIndex(e => e.StockName)
                .HasDatabaseName("IX_Stock_StockName");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Stock_IsDeleted");

            // Global Query Filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
