using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class CageWarehouseTransferConfiguration : BaseEntityConfiguration<CageWarehouseTransfer>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CageWarehouseTransfer> builder)
        {
            builder.ToTable("RII_CageWarehouseTransfer");
            builder.Property(x => x.TransferNo).HasMaxLength(40).IsRequired();
            builder.Property(x => x.TransferDate).HasPrecision(3);
            builder.Property(x => x.Status).HasConversion<byte>();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Lines)
                .WithOne(x => x.CageWarehouseTransfer)
                .HasForeignKey(x => x.CageWarehouseTransferId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.TransferNo)
                .HasDatabaseName("IX_RII_CageWarehouseTransfer_TransferNo");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
