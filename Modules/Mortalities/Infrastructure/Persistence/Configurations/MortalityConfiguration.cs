using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Mortalities.Infrastructure.Persistence.Configurations
{
    public class MortalityConfiguration : BaseEntityConfiguration<Mortality>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Mortality> builder)
        {
            builder.ToTable("RII_MORTALITY", table =>
            {
                table.HasCheckConstraint("CK_RII_MORTALITY_STATUS", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.MortalityNo).HasMaxLength(50);
            builder.Property(x => x.MortalityDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.ConfigureErpPostableHeader();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Mortalities)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ProjectId, x.MortalityDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_MORTALITY_PROJECT_DATE_ACTIVE");

            builder.HasIndex(x => x.MortalityNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [MortalityNo] IS NOT NULL")
                .HasDatabaseName("UX_RII_MORTALITY_MORTALITY_NO_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
