using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class ComplianceCorrectiveActionConfiguration : BaseEntityConfiguration<ComplianceCorrectiveAction>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ComplianceCorrectiveAction> builder)
        {
            builder.ToTable("RII_ComplianceCorrectiveAction");
            builder.Property(x => x.ActionCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.OwnerName).HasMaxLength(150);
            builder.Property(x => x.DueDate).HasPrecision(3);
            builder.Property(x => x.ClosedDate).HasPrecision(3);
            builder.Property(x => x.ClosureNote).HasMaxLength(1000);

            builder.HasOne(x => x.ComplianceAudit)
                .WithMany(x => x.CorrectiveActions)
                .HasForeignKey(x => x.ComplianceAuditId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
