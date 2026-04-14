using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class CageConfiguration : BaseEntityConfiguration<Cage>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Cage> builder)
        {
            builder.ToTable("RII_Cage");
            builder.Property(x => x.CageCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CageName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.CapacityGram).HasPrecision(18, 3);

            builder.HasIndex(x => x.CageCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_RII_Cage_CageCode_Active");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
