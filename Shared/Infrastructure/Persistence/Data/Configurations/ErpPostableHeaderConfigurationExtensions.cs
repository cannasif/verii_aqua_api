using aqua_api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Shared.Infrastructure.Persistence.Data.Configurations
{
    public static class ErpPostableHeaderConfigurationExtensions
    {
        public static void ConfigureErpPostableHeader<T>(this EntityTypeBuilder<T> builder)
            where T : class, IErpPostableHeader
        {
            builder.Property(x => x.IsERPIntegrated)
                .HasDefaultValue(false);

            builder.Property(x => x.ERPReferenceNumber)
                .HasMaxLength(50);

            builder.Property(x => x.ERPIntegrationDate)
                .IsRequired(false);

            builder.Property(x => x.ERPIntegrationStatus)
                .HasMaxLength(50);

            builder.Property(x => x.ERPErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.CountTriedBy)
                .HasDefaultValue(0);

            builder.HasIndex(x => x.IsERPIntegrated)
                .HasDatabaseName($"IX_{typeof(T).Name}_IsERPIntegrated");

            builder.HasIndex(x => x.ERPIntegrationStatus)
                .HasDatabaseName($"IX_{typeof(T).Name}_ERPIntegrationStatus");
        }
    }
}
