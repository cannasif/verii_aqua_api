using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WindDirectionMatchEntity = aqua_api.Modules.WindDirection.Domain.Entities.WindDirectionMatch;

namespace aqua_api.Modules.WindDirection.Infrastructure.Persistence.Configurations
{
    public class WindDirectionMatchConfiguration : BaseEntityConfiguration<WindDirectionMatchEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WindDirectionMatchEntity> builder)
        {
            builder.ToTable("RII_WIND_DIRECTION_MATCHES");

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

            builder.HasOne(x => x.WindDirection)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.WindDirectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ProjectId, x.ProjectCageId, x.RecordDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_WIND_DIRECTION_MATCHES_PROJECT_CAGE_DATE_ACTIVE");

            builder.HasIndex(x => x.RecordDate)
                .HasDatabaseName("IX_RII_WIND_DIRECTION_MATCHES_RECORD_DATE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
