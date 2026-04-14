using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Configurations
{
    public class NetOperationLineConfiguration : BaseEntityConfiguration<NetOperationLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<NetOperationLine> builder)
        {
            builder.ToTable("RII_NetOperationLine");
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.NetOperation)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.NetOperationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProjectCage)
                .WithMany()
                .HasForeignKey(x => x.ProjectCageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FishBatch)
                .WithMany()
                .HasForeignKey(x => x.FishBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
