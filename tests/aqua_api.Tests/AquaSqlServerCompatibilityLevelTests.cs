using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace aqua_api.Tests;

public class AquaSqlServerCompatibilityLevelTests
{
    [Fact]
    public void Contains_query_with_sql_server_2014_compatibility_does_not_generate_openjson()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=CompatibilityTest;Trusted_Connection=True;TrustServerCertificate=True;",
                sqlOptions => sqlOptions.UseCompatibilityLevel(120))
            .Options;

        using var db = new AquaDbContext(options);
        var projectIds = new List<long> { 2, 3, 5, 7 };

        var sql = db.Projects
            .Where(x => projectIds.Contains(x.Id))
            .ToQueryString();

        Assert.DoesNotContain("OPENJSON", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WITH (", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(" IN (", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Contains_query_without_legacy_compatibility_uses_openjson_translation()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CompatibilityTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);
        var projectIds = new List<long> { 2, 3, 5, 7 };

        var sql = db.Projects
            .Where(x => projectIds.Contains(x.Id))
            .ToQueryString();

        Assert.Contains("OPENJSON", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WITH (", sql, StringComparison.OrdinalIgnoreCase);
    }
}
