using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.NetInventory.Infrastructure.Persistence.Configurations;

public class NetInventoryMovementConfiguration : BaseEntityConfiguration<NetInventoryMovement>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NetInventoryMovement> builder)
    {
        builder.ToTable("RII_NET_INVENTORY_MOVEMENT", table =>
        {
            table.HasCheckConstraint("CK_RII_NET_INVENTORY_MOVEMENT_NET_TYPE", "[NetType] IN (1,2)");
            table.HasCheckConstraint("CK_RII_NET_INVENTORY_MOVEMENT_MOVEMENT_TYPE", "[MovementType] IN (1,2,3,4,5)");
            table.HasCheckConstraint("CK_RII_NET_INVENTORY_MOVEMENT_QUANTITY", "[Quantity] > 0");
        });

        builder.Property(x => x.MovementNo).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NetType).HasConversion<int>().IsRequired();
        builder.Property(x => x.MovementType).HasConversion<int>().IsRequired();
        builder.Property(x => x.MovementDate).IsRequired().HasColumnType("datetime2(3)");
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.MovementNo)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_NET_INVENTORY_MOVEMENT_MOVEMENT_NO_ACTIVE");

        builder.HasIndex(x => new { x.ProjectId, x.MovementDate })
            .HasDatabaseName("IX_RII_NET_INVENTORY_MOVEMENT_PROJECT_DATE");

        builder.HasIndex(x => new { x.TargetProjectCageId, x.MovementDate })
            .HasDatabaseName("IX_RII_NET_INVENTORY_MOVEMENT_TARGET_CAGE_DATE");

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.TargetWarehouse)
            .WithMany()
            .HasForeignKey(x => x.TargetWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SourceProjectCage)
            .WithMany()
            .HasForeignKey(x => x.SourceProjectCageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.TargetProjectCage)
            .WithMany()
            .HasForeignKey(x => x.TargetProjectCageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
