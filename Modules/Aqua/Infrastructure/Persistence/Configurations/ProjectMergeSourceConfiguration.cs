using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ProjectMergeSourceConfiguration : BaseEntityConfiguration<ProjectMergeSource>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectMergeSource> builder)
        {
            builder.ToTable("RII_ProjectMergeSource");
            builder.Property(x => x.SourceProjectCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SourceProjectName).HasMaxLength(200).IsRequired();

            builder.HasOne(x => x.ProjectMerge)
                .WithMany(x => x.SourceProjects)
                .HasForeignKey(x => x.ProjectMergeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SourceProject)
                .WithMany()
                .HasForeignKey(x => x.SourceProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
