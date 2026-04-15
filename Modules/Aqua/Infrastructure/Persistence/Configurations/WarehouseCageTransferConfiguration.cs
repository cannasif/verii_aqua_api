using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WarehouseCageTransferConfiguration : BaseEntityConfiguration<WarehouseCageTransfer>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WarehouseCageTransfer> builder)
        {
            builder.ToTable("RII_WarehouseCageTransfer");
            builder.Property(x => x.TransferNo).HasMaxLength(40).IsRequired();
            builder.Property(x => x.TransferDate).HasPrecision(3);
            builder.Property(x => x.Status).HasConversion<byte>();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Lines)
                .WithOne(x => x.WarehouseCageTransfer)
                .HasForeignKey(x => x.WarehouseCageTransferId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.TransferNo)
                .HasDatabaseName("IX_RII_WarehouseCageTransfer_TransferNo");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
