using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrentDirectionEntity = aqua_api.Modules.CurrentDirection.Domain.Entities.CurrentDirection;

namespace aqua_api.Modules.CurrentDirection.Infrastructure.Persistence.Configurations
{
    public class CurrentDirectionConfiguration : BaseEntityConfiguration<CurrentDirectionEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CurrentDirectionEntity> builder)
        {
            builder.ToTable("RII_CURRENT_DIRECTION");

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_CURRENT_DIRECTION_NAME_ACTIVE");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
