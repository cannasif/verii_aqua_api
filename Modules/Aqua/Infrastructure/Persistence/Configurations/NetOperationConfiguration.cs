using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class NetOperationConfiguration : BaseEntityConfiguration<NetOperation>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<NetOperation> builder)
        {
            builder.ToTable("RII_NetOperation", table =>
            {
                table.HasCheckConstraint("CK_RII_NetOperation_Status", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.OperationNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.OperationDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.NetOperations)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.OperationType)
                .WithMany(x => x.NetOperations)
                .HasForeignKey(x => x.OperationTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.OperationNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_NetOperation_OperationNo_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
