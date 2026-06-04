using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class SeaWaterTemperatureConfiguration : BaseEntityConfiguration<SeaWaterTemperature>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SeaWaterTemperature> builder)
        {
            builder.ToTable("RII_SEA_WATER_TEMPERATURE");

            builder.Property(x => x.RecordDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.WaterTemperatureCelsius).HasColumnType("decimal(18,3)");
            builder.Property(x => x.WeatherDescription).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ProjectId, x.ProjectCageId, x.RecordDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_SEA_WATER_TEMPERATURE_ProjectCageDate_Active");

            builder.HasIndex(x => x.RecordDate)
                .HasDatabaseName("IX_RII_SEA_WATER_TEMPERATURE_RecordDate");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
