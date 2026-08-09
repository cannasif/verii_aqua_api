using System.Globalization;
using System.Text;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Host.WebApi.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace aqua_api.Tests;

public class PagedRequestModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_ShouldReadPagedRequestFromJsonBody()
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

        var request = await BindAsync(HttpMethods.Post, json);

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(50, request.PageSize);
        Assert.Equal("BATCH-001", request.Search);
        Assert.Equal(new[] { "BatchCode", "ProjectCode" }, request.SearchFields);
        Assert.Equal("Id", request.SortBy);
        Assert.Equal("asc", request.SortDirection);
        Assert.Equal("or", request.FilterLogic);
        Assert.NotNull(request.Filters);
        var filter = Assert.Single(request.Filters);
        Assert.Equal("ProjectCode", filter.Column);
        Assert.Equal("OLIVKA", filter.Value);
    }

    [Fact]
    public async Task BindModelAsync_ShouldKeepQueryStringSupportForGetRequests()
    {
        var binder = new PagedRequestModelBinder();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.QueryString = new QueryString("?pageNumber=2&pageSize=75&search=YEM&searchFields=ErpStockCode&searchFields=StockName&filterLogic=or&filters=%5B%7B%22column%22%3A%22GrupKodu%22%2C%22operator%22%3A%22eq%22%2C%22value%22%3A%22YEM%22%7D%5D");

        var bindingContext = CreateBindingContext(httpContext);

        await binder.BindModelAsync(bindingContext);

        var request = Assert.IsType<PagedRequest>(bindingContext.Result.Model);
        Assert.Equal(2, request.PageNumber);
        Assert.Equal(75, request.PageSize);
        Assert.Equal("YEM", request.Search);
        Assert.Equal(new[] { "ErpStockCode", "StockName" }, request.SearchFields);
        Assert.Equal("or", request.FilterLogic);
        Assert.NotNull(request.Filters);
        var filter = Assert.Single(request.Filters);
        Assert.Equal("GrupKodu", filter.Column);
        Assert.Equal("YEM", filter.Value);
    }

    [Fact]
    public async Task BindModelAsync_ShouldDefaultGetRequestsToTwentyRows()
    {
        var request = await BindAsync(HttpMethods.Get);

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public async Task BindModelAsync_ShouldKeepAllAllowlistedGridSearchFields()
    {
        var fields = Enumerable.Range(1, 25).Select(index => $"Field{index}").ToArray();
        var query = string.Join("&", fields.Select(field => $"searchFields={field}"));
        var binder = new PagedRequestModelBinder();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.QueryString = new QueryString($"?{query}");

        var bindingContext = CreateBindingContext(httpContext);
        await binder.BindModelAsync(bindingContext);

        var request = Assert.IsType<PagedRequest>(bindingContext.Result.Model);
        Assert.Equal(fields, request.SearchFields);
    }

    [Fact]
    public async Task BindModelAsync_ShouldNormalizeInvalidBodyPageSizeToTwentyRows()
    {
        const string json = """
        {
          "pageNumber": 0,
          "pageSize": 0,
          "search": "  Korçay  "
        }
        """;

        var request = await BindAsync(HttpMethods.Post, json);

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(20, request.PageSize);
        Assert.Equal("Korçay", request.Search);
        Assert.NotNull(request.Filters);
        Assert.Empty(request.Filters);
    }

    [Fact]
    public async Task BindModelAsync_ShouldNormalizeNullSearchFieldsToEmptyList()
    {
        const string json = """
        {
          "search": "BATCH-001",
          "searchFields": null
        }
        """;

        var request = await BindAsync(HttpMethods.Post, json);

        Assert.NotNull(request.SearchFields);
        Assert.Empty(request.SearchFields);
    }

    private static async Task<PagedRequest> BindAsync(string method, string? json = null)
    {
        var binder = new PagedRequestModelBinder();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;

        if (json != null)
        {
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            httpContext.Request.ContentLength = httpContext.Request.Body.Length;
        }

        var bindingContext = CreateBindingContext(httpContext);

        await binder.BindModelAsync(bindingContext);

        return Assert.IsType<PagedRequest>(bindingContext.Result.Model);
    }

    private static DefaultModelBindingContext CreateBindingContext(HttpContext httpContext)
    {
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);
        var metadataProvider = new EmptyModelMetadataProvider();

        return new DefaultModelBindingContext
        {
            ActionContext = actionContext,
            ModelMetadata = metadataProvider.GetMetadataForType(typeof(PagedRequest)),
            ModelName = "request",
            ModelState = modelState,
            ValueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                httpContext.Request.Query,
                CultureInfo.InvariantCulture)
        };
    }
}
