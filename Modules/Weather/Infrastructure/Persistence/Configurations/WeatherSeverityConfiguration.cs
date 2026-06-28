using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Weather.Infrastructure.Persistence.Configurations
{
    public class WeatherSeverityConfiguration : BaseEntityConfiguration<WeatherSeverity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WeatherSeverity> builder)
        {
            builder.ToTable("RII_WEATHER_SEVERITY", table =>
            {
                table.HasCheckConstraint("CK_RII_WEATHER_SEVERITY_SCORE", "[Score] >= 0");
            });
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

            builder.HasIndex(x => x.Code)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_RII_WEATHER_SEVERITY_CODE_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
