using aqua_api.Modules.Identity.Domain.Entities;
using aqua_api.Modules.Warehouse.Domain.Entities;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Mappings;
using Xunit;

namespace aqua_api.Tests;

public sealed class AuditMetadataContractTests
{
    [Fact]
    public void WithAuditFrom_CopiesAuditIdsDatesAndLoadedUserNames()
    {
        var createdDate = new DateTime(2026, 8, 19, 10, 30, 0, DateTimeKind.Utc);
        var updatedDate = createdDate.AddHours(2);
        var entity = new Warehouse
        {
            CreatedDate = createdDate,
            UpdatedDate = updatedDate,
            CreatedBy = 11,
            UpdatedBy = 12,
            CreatedByUser = new User { Username = "creator", FirstName = "Ada", LastName = "Lovelace" },
            UpdatedByUser = new User { Username = "updater" },
        };

        var result = new TestAuditDto().WithAuditFrom(entity);

        Assert.Equal(createdDate, result.CreatedDate);
        Assert.Equal(updatedDate, result.UpdatedDate);
        Assert.Equal(11, result.CreatedBy);
        Assert.Equal(12, result.UpdatedBy);
        Assert.Equal("Ada Lovelace", result.CreatedByFullUser);
        Assert.Equal("updater", result.UpdatedByFullUser);
    }

    [Fact]
    public void PersistedApplicationDtos_WithAnId_ExposeTheSharedAuditContract()
    {
        var dtoAssembly = typeof(AuditDto).Assembly;
        var violations = dtoAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => type.Namespace?.Contains(".Application.Dtos", StringComparison.Ordinal) == true)
            .Where(type => type.GetProperty("Id")?.PropertyType is { } idType
                && (idType == typeof(long) || idType == typeof(int)))
            .Where(type => !typeof(AuditDto).IsAssignableFrom(type))
            .Where(type => type.Namespace?.Contains("Report", StringComparison.OrdinalIgnoreCase) != true)
            .Where(type => type.Namespace?.Contains("ProjectKpis", StringComparison.Ordinal) != true)
            .Where(type => type.Namespace?.Contains("Integrations", StringComparison.Ordinal) != true)
            .Select(type => type.FullName)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Persisted response DTOs missing {nameof(AuditDto)}: {string.Join(", ", violations)}");
    }

    private sealed class TestAuditDto : AuditDto
    {
    }
}
