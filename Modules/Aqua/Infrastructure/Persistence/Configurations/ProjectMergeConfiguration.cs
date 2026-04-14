using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ProjectMergeConfiguration : BaseEntityConfiguration<ProjectMerge>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProjectMerge> builder)
        {
            builder.ToTable("RII_ProjectMerge");
            builder.Property(x => x.TargetProjectCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TargetProjectName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.MergeDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.SourceProjectStateAfterMerge).HasConversion<byte>().IsRequired();

            builder.HasOne(x => x.TargetProject)
                .WithMany()
                .HasForeignKey(x => x.TargetProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
