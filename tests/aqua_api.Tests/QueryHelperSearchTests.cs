namespace aqua_api.Tests;

using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.FishBatches.Domain.Entities;
using aqua_api.Modules.Projects.Domain.Entities;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class QueryHelperSearchTests
{
    [Theory]
    [InlineData("pen.")]
    [InlineData("pen.kolu")]
    [InlineData("PEN-KOLU")]
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
        Assert.Collection(punctuatedSearchResult, x => Assert.Equal("KORÇAY Özel Ürün", x.StockName));
        Assert.Collection(turkishSearchResult, x => Assert.Equal("KORÇAY Özel Ürün", x.StockName));
    }

    [Fact]
    public void ApplySearch_WithoutColumns_ShouldSearchEntityStringProperties()
    {
        var stocks = new List<Stock>
        {
            new() { Id = 1, ErpStockCode = "Y008", StockName = "8 Yem", IsDeleted = false },
            new() { Id = 2, ErpStockCode = "L001", StockName = "Levrek", IsDeleted = false },
        }.AsQueryable();

        var result = stocks.ApplySearch("Y008").ToList();

        Assert.Collection(result, x => Assert.Equal("8 Yem", x.StockName));
    }

    [Fact]
    public void ApplySearch_WithoutColumns_ShouldSearchKnownNavigationProperties()
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

        var projectResult = batches.ApplySearch("ILKNAK").ToList();
        var stockResult = batches.ApplySearch("Levrek").ToList();

        Assert.Collection(projectResult, x => Assert.Equal("BATCH-001", x.BatchCode));
        Assert.Collection(stockResult, x => Assert.Equal("BATCH-001", x.BatchCode));
    }

    [Fact]
    public void ApplySearch_WithoutColumns_ShouldTranslateKnownNavigationPropertiesToSqlServer()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SearchTranslationTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);

        var sql = db.FishBatches
            .ApplySearch("ILKNAK")
            .ToQueryString();

        Assert.Contains("ProjectCode", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ErpStockCode", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("COLLATE", sql, StringComparison.OrdinalIgnoreCase);
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
            new() { Column = nameof(Stock.StockName), Operator = "contains", Value = "pen kolu" },
        }).ToList();
        var turkishResult = stocks.ApplyFilters(new List<Filter>
        {
            new() { Column = nameof(Stock.StockName), Operator = "contains", Value = "korcay" },
        }).ToList();

        Assert.Collection(punctuatedResult, x => Assert.Equal("PEN.KOLU ABS Atlas", x.StockName));
        Assert.Collection(turkishResult, x => Assert.Equal("KORÇAY Özel Ürün", x.StockName));
    }
}
