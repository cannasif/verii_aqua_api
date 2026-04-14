using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ProjectMergeCageConfiguration : BaseEntityConfiguration<ProjectMergeCage>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectMergeCage> builder)
        {
            builder.ToTable("RII_ProjectMergeCage");
            builder.Property(x => x.CageCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CageName).HasMaxLength(200).IsRequired();

            builder.HasOne(x => x.ProjectMerge)
                .WithMany(x => x.Cages)
                .HasForeignKey(x => x.ProjectMergeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SourceProject)
                .WithMany()
                .HasForeignKey(x => x.SourceProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Cage)
                .WithMany()
                .HasForeignKey(x => x.CageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
