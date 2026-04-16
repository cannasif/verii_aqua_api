using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class OpeningImportJobConfiguration : BaseEntityConfiguration<OpeningImportJob>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<OpeningImportJob> builder)
        {
            builder.ToTable("RII_OpeningImportJob");

            builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            builder.Property(x => x.SourceSystem).HasMaxLength(100);
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.MappingsJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.SummaryJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.PreviewedAt).HasPrecision(3);
            builder.Property(x => x.AppliedAt).HasPrecision(3);

            builder.HasMany(x => x.Rows)
                .WithOne(x => x.OpeningImportJob)
                .HasForeignKey(x => x.OpeningImportJobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
