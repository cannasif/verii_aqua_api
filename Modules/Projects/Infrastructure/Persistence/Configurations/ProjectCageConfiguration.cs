using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Projects.Infrastructure.Persistence.Configurations
{
    public class ProjectCageConfiguration : BaseEntityConfiguration<ProjectCage>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectCage> builder)
        {
            builder.ToTable("RII_PROJECT_CAGE", table =>
            {
                table.HasCheckConstraint("CK_RII_PROJECT_CAGE_ASSIGN_RELEASE", "[ReleasedDate] IS NULL OR [ReleasedDate] >= [AssignedDate]");
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
                .HasDatabaseName("UX_RII_PROJECT_CAGE_CAGE_ID_ACTIVE_ASSIGNMENT");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
