using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Feedings.Infrastructure.Persistence.Configurations
{
    public class FeedingConfiguration : BaseEntityConfiguration<Feeding>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Feeding> builder)
        {
            builder.ToTable("RII_FEEDING", table =>
            {
                table.HasCheckConstraint("CK_RII_FEEDING_SLOT", "[FeedingSlot] IN (0,1)");
                table.HasCheckConstraint("CK_RII_FEEDING_STATUS", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.FeedingNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FeedingDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.FeedingSlot).HasConversion<byte>().IsRequired();
            builder.Property(x => x.SourceType).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.ConfigureErpPostableHeader();

            builder.Property<DateTime>("FeedingDateOnly")
                .HasColumnType("date")
                .HasComputedColumnSql("CAST([FeedingDate] AS date)", true);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Feedings)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.FeedingNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_FEEDING_FEEDING_NO_ACTIVE");

            builder.HasIndex("ProjectId", "FeedingDateOnly", "FeedingSlot")
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_FEEDING_PROJECT_DATE_SLOT_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
