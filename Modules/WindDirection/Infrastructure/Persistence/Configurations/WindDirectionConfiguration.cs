using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WindDirectionEntity = aqua_api.Modules.WindDirection.Domain.Entities.WindDirection;

namespace aqua_api.Modules.WindDirection.Infrastructure.Persistence.Configurations
{
    public class WindDirectionConfiguration : BaseEntityConfiguration<WindDirectionEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WindDirectionEntity> builder)
        {
            builder.ToTable("RII_WIND_DIRECTION");

            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_WIND_DIRECTION_NAME_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
