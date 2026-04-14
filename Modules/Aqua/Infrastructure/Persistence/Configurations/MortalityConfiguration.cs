using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class MortalityConfiguration : BaseEntityConfiguration<Mortality>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Mortality> builder)
        {
            builder.ToTable("RII_Mortality", table =>
            {
                table.HasCheckConstraint("CK_RII_Mortality_Status", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.MortalityDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Mortalities)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ProjectId, x.MortalityDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_Mortality_ProjectDate_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
