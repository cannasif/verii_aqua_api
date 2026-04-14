using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WeighingConfiguration : BaseEntityConfiguration<Weighing>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Weighing> builder)
        {
            builder.ToTable("RII_Weighing", table =>
            {
                table.HasCheckConstraint("CK_RII_Weighing_Status", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.WeighingNo).HasMaxLength(50).IsRequired();
            builder.Property(x => x.WeighingDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Weighings)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.WeighingNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_Weighing_WeighingNo_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
