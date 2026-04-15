using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class FishTreatmentConfiguration : BaseEntityConfiguration<FishTreatment>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FishTreatment> builder)
        {
            builder.ToTable("RII_FishTreatment");
            builder.Property(x => x.TreatmentDate).HasPrecision(3).IsRequired();
            builder.Property(x => x.TreatmentType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.MedicationName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.ActiveIngredient).HasMaxLength(120);
            builder.Property(x => x.DoseValue).HasPrecision(18, 3);
            builder.Property(x => x.DoseUnit).HasMaxLength(30);
            builder.Property(x => x.WithdrawalEndDate).HasPrecision(3);
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.VeterinarianName).HasMaxLength(150);
            builder.Property(x => x.TreatmentReason).HasMaxLength(500);
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.FishTreatments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany(x => x.FishTreatments)
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany(x => x.FishTreatments)
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishHealthEvent)
                .WithMany()
                .HasForeignKey(x => x.FishHealthEventId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
