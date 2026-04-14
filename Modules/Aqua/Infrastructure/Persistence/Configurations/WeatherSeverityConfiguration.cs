using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WeatherSeverityConfiguration : BaseEntityConfiguration<WeatherSeverity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WeatherSeverity> builder)
        {
            builder.ToTable("RII_WeatherSeverity", table =>
            {
                table.HasCheckConstraint("CK_RII_WeatherSeverity_Score", "[Score] >= 0");
            });
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

            builder.HasOne(x => x.WeatherType)
                .WithMany(x => x.WeatherSeverities)
                .HasForeignKey(x => x.WeatherTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RII_WeatherSeverity_RII_WeatherType_WeatherTypeId");

            builder.HasIndex(x => new { x.WeatherTypeId, x.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [WeatherTypeId] IS NOT NULL")
                .HasDatabaseName("UX_RII_WeatherSeverity_WeatherType_Code_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
