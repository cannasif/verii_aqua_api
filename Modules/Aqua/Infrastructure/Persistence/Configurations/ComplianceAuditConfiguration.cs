using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ComplianceAuditConfiguration : BaseEntityConfiguration<ComplianceAudit>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ComplianceAudit> builder)
        {
            builder.ToTable("RII_ComplianceAudit");
            builder.Property(x => x.AuditDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.StandardCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ChecklistCode).HasMaxLength(50);
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.AuditorName).HasMaxLength(150);
            builder.Property(x => x.Summary).HasMaxLength(2000);
            builder.Property(x => x.NextAuditDate).HasPrecision(3);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ComplianceAudits)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.ComplianceAudits)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.ComplianceAudits)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
