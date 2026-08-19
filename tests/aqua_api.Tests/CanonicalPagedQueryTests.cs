using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Exceptions;
using aqua_api.Shared.Common.Helpers;
using Xunit;

namespace aqua_api.Tests;

public sealed class CanonicalPagedQueryTests
{
    private enum SampleStatus
    {
        Draft = 0,
        Posted = 1
    }

    private sealed class Row
    {
        public long Id { get; init; }
        public int TenantId { get; init; }
        public string? Code { get; init; }
        public string? Name { get; init; }
        public int Quantity { get; init; }
        public Guid ExternalId { get; init; }
        public bool IsActive { get; init; }
        public SampleStatus Status { get; init; }
        public DateTime? RecordedAt { get; init; }
        public NestedRow? Nested { get; init; }
    }

    private sealed class NestedRow
    {
        public string? Name { get; init; }
    }

    private static readonly IReadOnlyDictionary<string, string> Columns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = "Id",
            ["Code"] = "Code",
            ["Name"] = "Name",
            ["Quantity"] = "Quantity",
            ["ExternalId"] = "ExternalId",
            ["IsActive"] = "IsActive",
            ["Status"] = "Status",
            ["RecordedAt"] = "RecordedAt"
        };

    [Fact]
    public void Punctuation_IsPreservedAsPartOfOneWhitespaceTerm()
    {
        var rows = new[]
        {
            new Row { Id = 1, Code = "PEN-KOLU", Name = "Tam" },
            new Row { Id = 2, Code = "PEN YEDEK KOLU", Name = "Aralıklı" },
            new Row { Id = 3, Code = "PEN", Name = "KOLU" },
            new Row { Id = 4, Code = "PEN.KOLU", Name = "Noktalı" }
        }.AsQueryable();

        var result = rows.ApplySearch(new PagedRequest
        {
            Search = "PEN-KOLU",
            SearchFields = ["Code", "Name"]
        }, Columns).ToList();

        Assert.Collection(result, row => Assert.Equal(1, row.Id));
    }

    [Fact]
    public void WhitespaceTerms_AreAndedWhileFieldsAreOred()
    {
        var rows = new[]
        {
            new Row { Id = 1, Code = "PEN", Name = "KOLU" },
            new Row { Id = 2, Code = "PEN", Name = "GOVDE" }
        }.AsQueryable();

        var result = rows.ApplySearch(new PagedRequest
        {
            Search = "PEN KOLU",
            SearchFields = ["Code", "Name"]
        }, Columns).ToList();

        Assert.Collection(result, row => Assert.Equal(1, row.Id));
    }

    [Theory]
    [InlineData("CIG", "ÇİĞ")]
    [InlineData("CAGRI", "ÇAĞRI")]
    [InlineData("KORCAY", "KORÇAY")]
    [InlineData("isik", "IŞIK")]
    [InlineData("ışık", "İŞİK")]
    public void InMemorySearch_FoldsTurkishAndAsciiVariants(string search, string candidate)
    {
        var rows = new[] { new Row { Id = 1, Name = candidate } }.AsQueryable();

        var result = rows.ApplySearch(new PagedRequest
        {
            Search = search,
            SearchFields = ["Name"]
        }, Columns).ToList();

        Assert.Single(result);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("^")]
    [InlineData("\\")]
    public void LikeSpecialCharacters_AreLiteralInMemory(string character)
    {
        var rows = new[]
        {
            new Row { Id = 1, Name = $"A{character}B" },
            new Row { Id = 2, Name = "AB" }
        }.AsQueryable();

        var result = rows.ApplySearch(new PagedRequest
        {
            Search = character,
            SearchFields = ["Name"]
        }, Columns).ToList();

        Assert.Collection(result, row => Assert.Equal(1, row.Id));
    }

    [Fact]
    public void SelectedSearchFields_RestrictTheSearchScope()
    {
        var rows = new[]
        {
            new Row { Id = 1, Code = "MATCH", Name = "Other" },
            new Row { Id = 2, Code = "Other", Name = "MATCH" }
        }.AsQueryable();

        var result = rows.ApplySearch(new PagedRequest
        {
            Search = "MATCH",
            SearchFields = ["Name"]
        }, Columns).ToList();

        Assert.Collection(result, row => Assert.Equal(2, row.Id));
    }

    [Fact]
    public void EndpointAliasMapping_TakesPrecedenceForSearchFilterAndSort()
    {
        var rows = new[]
        {
            new Row { Id = 2, Name = "local-b", Nested = new NestedRow { Name = "ALPHA" } },
            new Row { Id = 1, Name = "local-a", Nested = new NestedRow { Name = "BETA" } }
        }.AsQueryable();
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DisplayName"] = "Nested.Name"
        };

        var searched = rows.ApplySearch(new PagedRequest
        {
            Search = "ALPHA",
            SearchFields = ["DisplayName"]
        }, mapping).ToList();
        var filtered = rows.ApplyFilters(
            [new Filter { Column = "DisplayName", Operator = "equals", Value = "BETA" }],
            "and",
            mapping).ToList();
        var sorted = rows.ApplySorting("DisplayName", "asc", mapping).ToList();

        Assert.Equal(2, Assert.Single(searched).Id);
        Assert.Equal(1, Assert.Single(filtered).Id);
        Assert.Equal([2L, 1L], sorted.Select(row => row.Id));
    }

    [Fact]
    public void ScalarSearch_UsesExactTypedEquality()
    {
        var externalId = Guid.NewGuid();
        var rows = new[]
        {
            new Row { Id = 1, Quantity = 12, ExternalId = externalId, IsActive = true, Status = SampleStatus.Posted },
            new Row { Id = 2, Quantity = 120, ExternalId = Guid.NewGuid(), IsActive = false, Status = SampleStatus.Draft }
        }.AsQueryable();

        Assert.Equal(1, Search(rows, "12", "Quantity").Single().Id);
        Assert.Equal(1, Search(rows, externalId.ToString(), "ExternalId").Single().Id);
        Assert.Equal(1, Search(rows, "1", "IsActive").Single().Id);
        Assert.Equal(1, Search(rows, "Posted", "Status").Single().Id);
    }

    [Fact]
    public void InvalidSearchFilterAndSortContracts_ThrowValidationException()
    {
        var rows = Array.Empty<Row>().AsQueryable();

        Assert.Throws<PagedQueryValidationException>(() =>
            rows.ApplySearch(new PagedRequest { Search = "x", SearchFields = ["Missing"] }, Columns));
        Assert.Throws<PagedQueryValidationException>(() =>
            rows.ApplyFilters([new Filter { Column = "Missing", Operator = "equals", Value = "1" }], "and", Columns));
        Assert.Throws<PagedQueryValidationException>(() =>
            rows.ApplyFilters([new Filter { Column = "Quantity", Operator = "contains", Value = "1" }], "and", Columns));
        Assert.Throws<PagedQueryValidationException>(() =>
            rows.ApplyFilters([new Filter { Column = "Quantity", Operator = "equals", Value = "NaN" }], "and", Columns));
        Assert.Throws<PagedQueryValidationException>(() => rows.ApplyFilters([], "xor", Columns));
        Assert.Throws<PagedQueryValidationException>(() => rows.ApplySorting("Missing", "asc", Columns));
        Assert.Throws<PagedQueryValidationException>(() => rows.ApplySorting("Id", "sideways", Columns));
    }

    [Fact]
    public void Filters_SupportAndOrAndNullOperators()
    {
        var rows = new[]
        {
            new Row { Id = 1, Code = "A", Quantity = 10, RecordedAt = null },
            new Row { Id = 2, Code = "B", Quantity = 20, RecordedAt = DateTime.UtcNow },
            new Row { Id = 3, Code = "C", Quantity = 30, RecordedAt = null }
        }.AsQueryable();

        var andResult = rows.ApplyFilters(
            [
                new Filter { Column = "Quantity", Operator = ">=", Value = "10" },
                new Filter { Column = "RecordedAt", Operator = "isNull" }
            ], "and", Columns).ToList();
        var orResult = rows.ApplyFilters(
            [
                new Filter { Column = "Code", Operator = "equals", Value = "A" },
                new Filter { Column = "RecordedAt", Operator = "isNotNull" }
            ], "or", Columns).ToList();

        Assert.Equal([1L, 3L], andResult.Select(row => row.Id));
        Assert.Equal([1L, 2L], orResult.Select(row => row.Id));
    }

    [Fact]
    public void UserOrFilters_CannotEscapeMandatoryScope()
    {
        var rows = new[]
        {
            new Row { Id = 1, TenantId = 1, Code = "A" },
            new Row { Id = 2, TenantId = 2, Code = "B" }
        }.AsQueryable();

        var result = rows
            .Where(row => row.TenantId == 1)
            .ApplyFilters(
                [
                    new Filter { Column = "Code", Operator = "equals", Value = "A" },
                    new Filter { Column = "Code", Operator = "equals", Value = "B" }
                ],
                "or",
                Columns)
            .ToList();

        Assert.Collection(result, row => Assert.Equal(1, row.TenantId));
    }

    [Fact]
    public void RequestLimitsAndPaginationOverflow_AreRejected()
    {
        Assert.Throws<PagedQueryValidationException>(() => QueryHelper.ValidateRequestContract(new PagedRequest
        {
            PageNumber = 0
        }));
        Assert.Throws<PagedQueryValidationException>(() => QueryHelper.ValidateRequestContract(new PagedRequest
        {
            PageSize = 501
        }));
        Assert.Throws<PagedQueryValidationException>(() => QueryHelper.ValidateRequestContract(new PagedRequest
        {
            Search = new string('x', 201)
        }));
        Assert.Throws<PagedQueryValidationException>(() => QueryHelper.ValidateRequestContract(new PagedRequest
        {
            SearchFields = Enumerable.Range(0, 13).Select(index => $"Field{index}").ToList()
        }));
        Assert.Throws<PagedQueryValidationException>(() => Array.Empty<Row>().AsQueryable()
            .ApplyPagination(int.MaxValue, 500));
    }

    [Fact]
    public void Sorting_AddsIdTieBreakerForStablePages()
    {
        var rows = new[]
        {
            new Row { Id = 3, Name = "Same" },
            new Row { Id = 1, Name = "Same" },
            new Row { Id = 2, Name = "Same" }
        }.AsQueryable();

        var result = rows.ApplySorting("Name", "asc", Columns).ToList();

        Assert.Equal([1L, 2L, 3L], result.Select(row => row.Id));
    }

    private static List<Row> Search(IQueryable<Row> rows, string search, string field) =>
        rows.ApplySearch(new PagedRequest
        {
            Search = search,
            SearchFields = [field]
        }, Columns).ToList();
}
