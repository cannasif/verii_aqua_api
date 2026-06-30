using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using aqua_api.Shared.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace aqua_api.Shared.Host.WebApi.ModelBinding
{
    public class PagedRequestModelBinder : IModelBinder
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var bodyRequest = await TryBindFromBodyAsync(bindingContext).ConfigureAwait(false);
            if (bodyRequest != null)
            {
                Normalize(bodyRequest);
                bindingContext.Result = ModelBindingResult.Success(bodyRequest);
                return;
            }

            var query = bindingContext.HttpContext.Request.Query;
            var request = CreateRequestInstance(bindingContext);
            request.PageNumber = ParseInt(query, new[] { "pageNumber", "PageNumber" }, 1);
            request.PageSize = ParseInt(query, new[] { "pageSize", "PageSize" }, 20);
            request.Search = ParseString(query, new[] { "search", "Search" });
            request.SortBy = ParseString(query, new[] { "sortBy", "SortBy" }) ?? "Id";
            request.SortDirection = ParseString(query, new[] { "sortDirection", "SortDirection" }) ?? "desc";
            request.Filters = ParseJsonFilters(query) ?? ParseIndexedFilters(query) ?? new List<Filter>();

            var filterLogic = ParseString(query, new[] { "filterLogic", "FilterLogic" });
            request.FilterLogic = string.Equals(filterLogic, "or", StringComparison.OrdinalIgnoreCase) ? "or" : "and";

            BindDerivedBooleanProperties(request, query);
            BindDerivedNullableLongProperties(request, query);

            Normalize(request);
            bindingContext.Result = ModelBindingResult.Success(request);
        }

        private static async Task<PagedRequest?> TryBindFromBodyAsync(ModelBindingContext bindingContext)
        {
            var httpRequest = bindingContext.HttpContext.Request;
            if (!CanReadJsonBody(httpRequest))
            {
                return null;
            }

            httpRequest.EnableBuffering();
            if (httpRequest.Body.CanSeek)
            {
                httpRequest.Body.Position = 0;
            }

            try
            {
                var parsed = await JsonSerializer
                    .DeserializeAsync(httpRequest.Body, bindingContext.ModelType, JsonOptions)
                    .ConfigureAwait(false);

                if (httpRequest.Body.CanSeek)
                {
                    httpRequest.Body.Position = 0;
                }

                return parsed as PagedRequest;
            }
            catch (JsonException)
            {
                if (httpRequest.Body.CanSeek)
                {
                    httpRequest.Body.Position = 0;
                }

                return null;
            }
        }

        private static bool CanReadJsonBody(HttpRequest request)
        {
            if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            if (!request.HasJsonContentType())
            {
                return false;
            }

            return request.ContentLength is null or > 0;
        }

        private static PagedRequest CreateRequestInstance(ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;
            if (typeof(PagedRequest).IsAssignableFrom(modelType) &&
                Activator.CreateInstance(modelType) is PagedRequest typedRequest)
            {
                return typedRequest;
            }

            return new PagedRequest();
        }

        private static void Normalize(PagedRequest request)
        {
            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;
            request.Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
            request.SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "Id" : request.SortBy.Trim();
            request.SortDirection = string.IsNullOrWhiteSpace(request.SortDirection) ? "desc" : request.SortDirection.Trim();
            request.FilterLogic = string.Equals(request.FilterLogic, "or", StringComparison.OrdinalIgnoreCase) ? "or" : "and";
            request.Filters = request.Filters?
                .Where(IsValidFilter)
                .Select(filter => new Filter
                {
                    Column = filter.Column.Trim(),
                    Operator = string.IsNullOrWhiteSpace(filter.Operator) ? "Equals" : filter.Operator.Trim(),
                    Value = filter.Value?.Trim() ?? string.Empty
                })
                .ToList() ?? new List<Filter>();
        }

        private static void BindDerivedBooleanProperties(PagedRequest request, Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var requestType = request.GetType();
            foreach (var property in requestType.GetProperties().Where(property => property.PropertyType == typeof(bool)))
            {
                var rawValue = ParseString(query, new[] { ToCamelCase(property.Name), property.Name });
                if (bool.TryParse(rawValue, out var parsed))
                {
                    property.SetValue(request, parsed);
                }
            }
        }

        private static void BindDerivedNullableLongProperties(PagedRequest request, Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var requestType = request.GetType();
            foreach (var property in requestType.GetProperties().Where(property => property.PropertyType == typeof(long?)))
            {
                var rawValue = ParseString(query, new[] { ToCamelCase(property.Name), property.Name });
                if (long.TryParse(rawValue, out var parsed) && parsed > 0)
                {
                    property.SetValue(request, parsed);
                }
            }
        }

        private static string ToCamelCase(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : char.ToLowerInvariant(value[0]) + value[1..];
        }

        private static int ParseInt(Microsoft.AspNetCore.Http.IQueryCollection query, IEnumerable<string> keys, int fallback)
        {
            var raw = ParseString(query, keys);
            return int.TryParse(raw, out var value) ? value : fallback;
        }

        private static string? ParseString(Microsoft.AspNetCore.Http.IQueryCollection query, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (query.TryGetValue(key, out var value) && !StringValues.IsNullOrEmpty(value))
                {
                    return value.ToString();
                }
            }

            return null;
        }

        private static List<Filter>? ParseJsonFilters(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var raw = ParseString(query, new[] { "filters", "Filters" });
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                if (!raw.TrimStart().StartsWith("[", StringComparison.Ordinal))
                {
                    return null;
                }

                var parsed = JsonSerializer.Deserialize<List<Filter>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return parsed?
                    .Where(IsValidFilter)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        private static List<Filter>? ParseIndexedFilters(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var filters = new List<Filter>();

            for (var index = 0; index < 200; index++)
            {
                var column = ParseString(query, new[]
                {
                    $"filters[{index}].column",
                    $"filters[{index}].Column",
                    $"Filters[{index}].column",
                    $"Filters[{index}].Column"
                });
                var filterOperator = ParseString(query, new[]
                {
                    $"filters[{index}].operator",
                    $"filters[{index}].Operator",
                    $"Filters[{index}].operator",
                    $"Filters[{index}].Operator"
                });
                var value = ParseString(query, new[]
                {
                    $"filters[{index}].value",
                    $"filters[{index}].Value",
                    $"Filters[{index}].value",
                    $"Filters[{index}].Value"
                });

                if (column == null && filterOperator == null && value == null)
                {
                    if (index == 0)
                    {
                        return null;
                    }

                    break;
                }

                var filter = new Filter
                {
                    Column = column ?? string.Empty,
                    Operator = string.IsNullOrWhiteSpace(filterOperator) ? "Equals" : filterOperator,
                    Value = value ?? string.Empty
                };

                if (IsValidFilter(filter))
                {
                    filters.Add(filter);
                }
            }

            return filters.Count == 0 ? null : filters;
        }

        private static bool IsValidFilter(Filter filter)
        {
            return !string.IsNullOrWhiteSpace(filter.Column) &&
                   !string.IsNullOrWhiteSpace(filter.Operator);
        }
    }
}
