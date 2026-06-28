using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrentDirectionMatchEntity = aqua_api.Modules.CurrentDirection.Domain.Entities.CurrentDirectionMatch;

namespace aqua_api.Modules.CurrentDirection.Infrastructure.Persistence.Configurations
{
    public class CurrentDirectionMatchConfiguration : BaseEntityConfiguration<CurrentDirectionMatchEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CurrentDirectionMatchEntity> builder)
        {
            builder.ToTable("RRII_CURRENT_DIRECTION_MATCHES");

            builder.Property(x => x.RecordDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CurrentDirection)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.CurrentDirectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ProjectId, x.ProjectCageId, x.RecordDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RRII_CURRENT_DIRECTION_MATCHES_PROJECT_CAGE_DATE_ACTIVE");

            builder.HasIndex(x => x.RecordDate)
                .HasDatabaseName("IX_RRII_CURRENT_DIRECTION_MATCHES_RECORD_DATE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
