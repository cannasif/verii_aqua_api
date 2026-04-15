using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WarehouseTransferConfiguration : BaseEntityConfiguration<WarehouseTransfer>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WarehouseTransfer> builder)
        {
            builder.ToTable("RII_WarehouseTransfer");

            builder.Property(x => x.TransferNo).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
