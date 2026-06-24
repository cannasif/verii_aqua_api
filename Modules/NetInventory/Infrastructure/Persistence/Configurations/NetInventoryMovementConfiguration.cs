using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.NetInventory.Infrastructure.Persistence.Configurations;

public class NetInventoryMovementConfiguration : BaseEntityConfiguration<NetInventoryMovement>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NetInventoryMovement> builder)
    {
        builder.ToTable("RII_NetInventoryMovement", table =>
        {
            table.HasCheckConstraint("CK_RII_NetInventoryMovement_NetType", "[NetType] IN (1,2)");
            table.HasCheckConstraint("CK_RII_NetInventoryMovement_MovementType", "[MovementType] IN (1,2,3,4,5)");
            table.HasCheckConstraint("CK_RII_NetInventoryMovement_Quantity", "[Quantity] > 0");
        });

        builder.Property(x => x.MovementNo).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NetType).HasConversion<int>().IsRequired();
        builder.Property(x => x.MovementType).HasConversion<int>().IsRequired();
        builder.Property(x => x.MovementDate).IsRequired().HasColumnType("datetime2(3)");
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.MovementNo)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_RII_NetInventoryMovement_MovementNo_Active");

        builder.HasIndex(x => new { x.ProjectId, x.MovementDate })
            .HasDatabaseName("IX_RII_NetInventoryMovement_Project_Date");

        builder.HasIndex(x => new { x.TargetProjectCageId, x.MovementDate })
            .HasDatabaseName("IX_RII_NetInventoryMovement_TargetCage_Date");

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
