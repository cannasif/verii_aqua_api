using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.StockConverts.Infrastructure.Persistence.Configurations
{
    public class StockConvertConfiguration : BaseEntityConfiguration<StockConvert>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockConvert> builder)
        {
            builder.ToTable("RII_STOCK_CONVERT", table =>
            {
                table.HasCheckConstraint("CK_RII_STOCK_CONVERT_STATUS", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.ConvertNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ConvertDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.StockConverts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.ConvertNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_STOCK_CONVERT_CONVERT_NO_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
