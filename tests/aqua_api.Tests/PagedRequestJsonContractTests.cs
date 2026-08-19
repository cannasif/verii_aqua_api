using System.Text.Json;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Exceptions;
using aqua_api.Shared.Common.Helpers;
using Xunit;

namespace aqua_api.Tests;

public sealed class PagedRequestJsonContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_ReadsCanonicalPostBody()
    {
        const string json = """
        {
          "pageNumber": 1,
          "pageSize": 50,
          "search": "BATCH-001",
          "searchFields": ["BatchCode", "ProjectCode"],
          "sortBy": "Id",
          "sortDirection": "asc",
          "filterLogic": "or",
          "filters": [
            { "column": "ProjectCode", "operator": "contains", "value": "OLIVKA" }
          ]
        }
        """;

        var request = JsonSerializer.Deserialize<PagedRequest>(json, JsonOptions);

        Assert.NotNull(request);
        Assert.Equal(1, request.PageNumber);
        Assert.Equal(50, request.PageSize);
        Assert.Equal("BATCH-001", request.Search);
        Assert.Equal(new[] { "BatchCode", "ProjectCode" }, request.SearchFields);
        Assert.True(request.SearchFieldsSpecified);
        Assert.Equal("Id", request.SortBy);
        Assert.Equal("asc", request.SortDirection);
        Assert.Equal("or", request.FilterLogic);
        var filter = Assert.Single(request.Filters!);
        Assert.Equal("ProjectCode", filter.Column);
        Assert.Equal("OLIVKA", filter.Value);
    }

    [Fact]
    public void Deserialize_DistinguishesMissingSearchFieldsFromExplicitEmptySelection()
    {
        var missing = JsonSerializer.Deserialize<PagedRequest>(
            """{ "search": "BATCH-001" }""",
            JsonOptions)!;
        var explicitEmpty = JsonSerializer.Deserialize<PagedRequest>(
            """{ "search": "BATCH-001", "searchFields": [] }""",
            JsonOptions)!;

        Assert.False(missing.SearchFieldsSpecified);
        Assert.True(explicitEmpty.SearchFieldsSpecified);
        Assert.Throws<PagedQueryValidationException>(() => QueryHelper.ValidateRequestContract(explicitEmpty));
    }

    [Fact]
    public void Deserialize_NormalizesNullSearchFieldsToEmptyList()
    {
        var request = JsonSerializer.Deserialize<PagedRequest>(
            """{ "search": "BATCH-001", "searchFields": null }""",
            JsonOptions)!;

        Assert.Empty(request.SearchFields);
        Assert.True(request.SearchFieldsSpecified);
    }

}
