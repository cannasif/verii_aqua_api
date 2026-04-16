using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class OpeningImportRowConfiguration : BaseEntityConfiguration<OpeningImportRow>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<OpeningImportRow> builder)
        {
            builder.ToTable("RII_OpeningImportRow");

            builder.Property(x => x.SheetName).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
            builder.Property(x => x.RawDataJson).HasColumnType("nvarchar(max)").IsRequired();
            builder.Property(x => x.NormalizedDataJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.MessagesJson).HasColumnType("nvarchar(max)");

            builder.HasIndex(x => new { x.OpeningImportJobId, x.SheetName, x.RowNumber })
                .HasDatabaseName("IX_RII_OpeningImportRow_JobSheetRow");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
