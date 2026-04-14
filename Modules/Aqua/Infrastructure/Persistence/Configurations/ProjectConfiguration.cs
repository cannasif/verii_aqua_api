using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{

    public class ProjectConfiguration : BaseEntityConfiguration<Project>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("RII_Project", table =>
            {
                table.HasCheckConstraint("CK_RII_Project_Status", "[Status] IN (0,1,2)");
            });
            builder.Property(x => x.ProjectCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProjectName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => x.ProjectCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_Project_ProjectCode_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
