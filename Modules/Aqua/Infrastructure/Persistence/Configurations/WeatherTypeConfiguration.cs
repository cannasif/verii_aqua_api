using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class WeatherTypeConfiguration : BaseEntityConfiguration<WeatherType>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WeatherType> builder)
        {
            builder.ToTable("RII_WeatherType");
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_WeatherType_Code_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
