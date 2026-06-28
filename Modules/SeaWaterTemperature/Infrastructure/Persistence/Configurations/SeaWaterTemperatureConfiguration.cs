using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeaWaterTemperatureEntity = aqua_api.Modules.SeaWaterTemperature.Domain.Entities.SeaWaterTemperature;

namespace aqua_api.Modules.SeaWaterTemperature.Infrastructure.Persistence.Configurations
{
    public class SeaWaterTemperatureConfiguration : BaseEntityConfiguration<SeaWaterTemperatureEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SeaWaterTemperatureEntity> builder)
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
                .HasDatabaseName("UX_RII_SEA_WATER_TEMPERATURE_PROJECT_CAGE_DATE_ACTIVE");

            builder.HasIndex(x => x.RecordDate)
                .HasDatabaseName("IX_RII_SEA_WATER_TEMPERATURE_RECORD_DATE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
