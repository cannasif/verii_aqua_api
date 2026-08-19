namespace aqua_api.Tests;

using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.FishBatches.Domain.Entities;
using aqua_api.Modules.Projects.Domain.Entities;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Common.Exceptions;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class QueryHelperSearchTests
{
    [Theory]
    [InlineData("pen.")]
    [InlineData("pen.kolu")]
    [InlineData("kolu")]
    public void ApplySearch_ShouldMatchPunctuatedStockTerms(string search)
    {
        var stocks = new List<Stock>
        {
            new() { Id = 1, ErpStockCode = "PEN-001", StockName = "PEN.KOLU ABS ATLAS", IsDeleted = false },
            new() { Id = 2, ErpStockCode = "ABC-001", StockName = "BASKA URUN", IsDeleted = false },
        }.AsQueryable();

        var result = stocks.ApplySearch(search, nameof(Stock.StockName), nameof(Stock.ErpStockCode)).ToList();

        Assert.Collection(result, x => Assert.Equal("PEN.KOLU ABS ATLAS", x.StockName));
    }

    [Fact]
    public void ApplySearch_ShouldMatchTurkishCharactersCaseAndPunctuation()
    {
        var stocks = new List<Stock>
        {
            new() { Id = 1, ErpStockCode = "KRC-01", StockName = "KORÇAY Özel Ürün", IsDeleted = false },
            new() { Id = 2, ErpStockCode = "ABC-01", StockName = "Başka Ürün", IsDeleted = false },
        }.AsQueryable();

        var plainSearchResult = stocks.ApplySearch("korcay", nameof(Stock.StockName)).ToList();
        var punctuatedSearchResult = stocks.ApplySearch("KOR-CAY", nameof(Stock.StockName)).ToList();
        var turkishSearchResult = stocks.ApplySearch("Korçay", nameof(Stock.StockName)).ToList();

        Assert.Collection(plainSearchResult, x => Assert.Equal("KORÇAY Özel Ürün", x.StockName));
        Assert.Empty(punctuatedSearchResult);
        Assert.Collection(turkishSearchResult, x => Assert.Equal("KORÇAY Özel Ürün", x.StockName));
    }

    [Fact]
    public void ApplySearch_WithoutColumns_ShouldRejectImplicitWideSearch()
    {
        var stocks = new List<Stock>
        {
            new() { Id = 1, ErpStockCode = "Y008", StockName = "8 Yem", IsDeleted = false },
            new() { Id = 2, ErpStockCode = "L001", StockName = "Levrek", IsDeleted = false },
        }.AsQueryable();

        Assert.Throws<PagedQueryValidationException>(() => stocks.ApplySearch("Y008").ToList());
    }

    [Fact]
    public void ApplySearch_WithExplicitMapping_ShouldSearchNavigationProperties()
    {
        var batches = new List<FishBatch>
        {
            new()
            {
                Id = 1,
                BatchCode = "BATCH-001",
                Project = new Project { ProjectCode = "20240331ILKNAK", ProjectName = "15. PROJE" },
                FishStock = new Stock { ErpStockCode = "L001", StockName = "Levrek" },
            },
            new()
            {
                Id = 2,
                BatchCode = "BATCH-002",
                Project = new Project { ProjectCode = "OTHER", ProjectName = "Başka Proje" },
                FishStock = new Stock { ErpStockCode = "C001", StockName = "Çipura" },
            },
        }.AsQueryable();

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ProjectCode"] = "Project.ProjectCode",
            ["StockName"] = "FishStock.StockName"
        };
        var projectResult = batches.ApplySearch(
            new PagedRequest { Search = "ILKNAK", SearchFields = ["ProjectCode"] }, mapping).ToList();
        var stockResult = batches.ApplySearch(
            new PagedRequest { Search = "Levrek", SearchFields = ["StockName"] }, mapping).ToList();

        Assert.Collection(projectResult, x => Assert.Equal("BATCH-001", x.BatchCode));
        Assert.Collection(stockResult, x => Assert.Equal("BATCH-001", x.BatchCode));
    }

    [Fact]
    public void ApplySearch_WithSelectedFields_ShouldRestrictSearchScope()
    {
        var projects = new List<Project>
        {
            new() { Id = 1, ProjectCode = "MATCH-CODE", ProjectName = "First" },
            new() { Id = 2, ProjectCode = "OTHER", ProjectName = "MATCH-CODE" },
        }.AsQueryable();

        var request = new PagedRequest
        {
            Search = "MATCH-CODE",
            SearchFields = [nameof(Project.ProjectCode)]
        };

        var result = projects.ApplySearch(request).ToList();

        Assert.Collection(result, project => Assert.Equal(1, project.Id));
    }

    [Fact]
    public void ApplySearch_WithSelectedRecordId_ShouldUseExactNumericMatch()
    {
        var projects = new List<Project>
        {
            new() { Id = 12, ProjectCode = "P-12", ProjectName = "First" },
            new() { Id = 120, ProjectCode = "P-120", ProjectName = "Second" },
        }.AsQueryable();

        var request = new PagedRequest
        {
            Search = "12",
            SearchFields = [nameof(Project.Id)]
        };

        var result = projects.ApplySearch(request).ToList();

        Assert.Collection(result, project => Assert.Equal(12, project.Id));
    }

    [Fact]
    public void ApplySearch_WithSelectedNavigationAlias_ShouldResolveKnownPath()
    {
        var batches = new List<FishBatch>
        {
            new() { Id = 1, BatchCode = "BATCH-001", Project = new Project { ProjectCode = "ILKNAK" } },
            new() { Id = 2, BatchCode = "BATCH-002", Project = new Project { ProjectCode = "OTHER" } },
        }.AsQueryable();

        var request = new PagedRequest
        {
            Search = "ILKNAK",
            SearchFields = [nameof(Project.ProjectCode)]
        };

        var result = batches.ApplySearch(request, new Dictionary<string, string>
        {
            ["ProjectCode"] = "Project.ProjectCode"
        }).ToList();

        Assert.Collection(result, batch => Assert.Equal("BATCH-001", batch.BatchCode));
    }

    [Fact]
    public void SelectedSearchAndAdvancedFilters_ShouldTranslateOnMirrorProjection()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MirrorSearchTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);
        var request = new PagedRequest
        {
            Search = "YEM2026",
            SearchFields = [nameof(ErpReceiptShipmentMovementDto.DocumentNo)],
            Filters =
            [
                new Filter
                {
                    Column = nameof(ErpReceiptShipmentMovementDto.IsProcessed),
                    Operator = "eq",
                    Value = "true"
                }
            ]
        };

        var sql = db.ErpReceiptShipmentMovements
            .Select(movement => new ErpReceiptShipmentMovementDto
            {
                Id = movement.Id,
                DocumentNo = movement.DocumentNo,
                IsProcessed = movement.IsProcessed
            })
            .ApplySearch(request)
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ToQueryString();

        Assert.Contains("DocumentNo", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IsProcessed", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("YEM2026", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplySearch_WithMapping_ShouldTranslateNavigationPropertiesToSqlServer()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SearchTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);

        var sql = db.FishBatches
            .ApplySearch(
                new PagedRequest { Search = "ILKNAK", SearchFields = ["ProjectCode"] },
                new Dictionary<string, string> { ["ProjectCode"] = "Project.ProjectCode" })
            .ToQueryString();

        Assert.Contains("ProjectCode", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ESCAPE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeneralSearch_ShouldTranslateTurkishAsciiPatternWithoutColumnTransforms()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SearchPatternTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        using var db = new AquaDbContext(options);
        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["StockName"] = nameof(Stock.StockName),
        };

        var sql = db.Stocks
            .ApplySearch(new PagedRequest
            {
                Search = "ÇİĞ ÜRÜN",
                SearchFields = ["StockName"],
            }, columns)
            .ToQueryString();

        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ESCAPE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[cCçÇ]", sql, StringComparison.Ordinal);
        Assert.Contains("[iIİıîÎ]", sql, StringComparison.Ordinal);
        Assert.Contains("[uUüÜûÛ]", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("LOWER(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UPPER(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("REPLACE(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TRANSLATE(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("COLLATE ", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectedStringField_ShouldBeTheOnlySearchColumnInSqlWhereClause()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SelectedFieldTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        using var db = new AquaDbContext(options);
        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ErpStockCode"] = nameof(Stock.ErpStockCode),
            ["StockName"] = nameof(Stock.StockName),
        };

        var sql = db.Stocks
            .ApplySearch(new PagedRequest { Search = "PEN-KOLU", SearchFields = ["StockName"] }, columns)
            .ToQueryString();
        var whereClause = sql[sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase)..];

        Assert.Contains("StockName", whereClause, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ErpStockCode", whereClause, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PEN-K", whereClause, StringComparison.Ordinal);
    }

    [Fact]
    public void NumericIdSearch_ShouldTranslateToEqualityWithoutStringConversionOrLike()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NumericSearchTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        using var db = new AquaDbContext(options);
        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = nameof(Stock.Id),
        };

        var sql = db.Stocks
            .ApplySearch(new PagedRequest { Search = "42", SearchFields = ["Id"] }, columns)
            .ToQueryString();
        var whereClause = sql[sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase)..];

        Assert.Contains("Id", whereClause, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LIKE", whereClause, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CONVERT", whereClause, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("nvarchar", whereClause, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyFilters_ShouldMatchPunctuatedContainsValues()
    {
        var stocks = new List<Stock>
        {
            new() { Id = 1, ErpStockCode = "PN-01", StockName = "PEN.KOLU ABS Atlas", IsDeleted = false },
            new() { Id = 2, ErpStockCode = "KRC-01", StockName = "KORÇAY Özel Ürün", IsDeleted = false },
            new() { Id = 3, ErpStockCode = "ABC-01", StockName = "Başka Ürün", IsDeleted = false },
        }.AsQueryable();

        var punctuatedResult = stocks.ApplyFilters(new List<Filter>
        {
            new() { Column = nameof(Stock.StockName), Operator = "contains", Value = "PEN.KOLU" },
        }).ToList();
        var turkishResult = stocks.ApplyFilters(new List<Filter>
        {
            new() { Column = nameof(Stock.StockName), Operator = "contains", Value = "korcay" },
        }).ToList();

        Assert.Collection(punctuatedResult, x => Assert.Equal("PEN.KOLU ABS Atlas", x.StockName));
        Assert.Empty(turkishResult);
    }
}
