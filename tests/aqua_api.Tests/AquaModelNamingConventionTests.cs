using System.Text.RegularExpressions;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaModelNamingConventionTests
{
    private static readonly Regex UppercaseRiiTableNamePattern = new(@"^RII_[A-Z0-9_]+$", RegexOptions.Compiled);

    [Fact]
    public void RiiTables_ShouldUseUppercaseSnakeCaseNames()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(local);Database=ModelOnly;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);

        var invalidTableNames = db.Model
            .GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
            .Where(tableName => tableName!.StartsWith("RII_", StringComparison.OrdinalIgnoreCase))
            .Where(tableName => !UppercaseRiiTableNamePattern.IsMatch(tableName!))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tableName => tableName, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            invalidTableNames.Count == 0,
            "RII table names must stay uppercase snake_case to match production SQL table renames. Invalid names: " +
            string.Join(", ", invalidTableNames));
    }
}
