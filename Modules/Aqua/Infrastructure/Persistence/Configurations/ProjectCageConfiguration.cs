using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ProjectCageConfiguration : BaseEntityConfiguration<ProjectCage>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectCage> builder)
        {
            builder.ToTable("RII_ProjectCage", table =>
            {
                table.HasCheckConstraint("CK_RII_ProjectCage_AssignRelease", "[ReleasedDate] IS NULL OR [ReleasedDate] >= [AssignedDate]");
            });
            builder.Property(x => x.AssignedDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.ReleasedDate).HasPrecision(3);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ProjectCages)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Cage)
                .WithMany(x => x.ProjectCages)
                .HasForeignKey(x => x.CageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.CageId)
                .IsUnique()
                .HasFilter("[ReleasedDate] IS NULL AND [IsDeleted] = 0")
                .HasDatabaseName("UX_RII_ProjectCage_CageId_ActiveAssignment");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
