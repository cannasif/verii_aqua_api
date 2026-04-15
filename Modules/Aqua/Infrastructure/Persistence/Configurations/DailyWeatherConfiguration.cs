using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class DailyWeatherConfiguration : BaseEntityConfiguration<DailyWeather>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DailyWeather> builder)
        {
            builder.ToTable("RII_DailyWeather");
            builder.Property(x => x.WeatherDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.TemperatureC).HasPrecision(18, 3);
            builder.Property(x => x.WindKnot).HasPrecision(18, 3);
            builder.Property(x => x.WaterTemperatureSurfaceC).HasPrecision(18, 3);
            builder.Property(x => x.WaterTemperatureDepthC).HasPrecision(18, 3);
            builder.Property(x => x.DissolvedOxygenMgL).HasPrecision(18, 3);
            builder.Property(x => x.SalinityPpt).HasPrecision(18, 3);
            builder.Property(x => x.Ph).HasPrecision(18, 3);
            builder.Property(x => x.CurrentSpeedKn).HasPrecision(18, 3);
            builder.Property(x => x.WaveHeightM).HasPrecision(18, 3);
            builder.Property(x => x.TurbidityNtu).HasPrecision(18, 3);
            builder.Property(x => x.AmmoniaMgL).HasPrecision(18, 3);
            builder.Property(x => x.NitriteMgL).HasPrecision(18, 3);
            builder.Property(x => x.AlgalBloomIndex).HasPrecision(18, 3);
            builder.Property(x => x.SensorHealthScore).HasPrecision(18, 3);
            builder.Property(x => x.SensorRecordedAt).HasPrecision(3);
            builder.Property(x => x.DataSource).HasMaxLength(50);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.DailyWeathers)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.WeatherType)
                .WithMany(x => x.DailyWeathers)
                .HasForeignKey(x => x.WeatherTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.WeatherSeverity)
                .WithMany(x => x.DailyWeathers)
                .HasForeignKey(x => x.WeatherSeverityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ProjectId, x.WeatherDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_DailyWeather_ProjectDate_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
