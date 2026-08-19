using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Common.Exceptions;
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
            request.PageNumber = ParseInt(query, new[] { "pageNumber", "PageNumber" }, 1, "pageNumber");
            request.PageSize = ParseInt(query, new[] { "pageSize", "PageSize" }, 20, "pageSize");
            request.Search = ParseString(query, new[] { "search", "Search" });
            request.SearchFields = ParseStringList(query, new[] { "searchFields", "SearchFields" });
            if (query.ContainsKey("searchFields") || query.ContainsKey("SearchFields"))
            {
                request.MarkSearchFieldsSpecified();
            }
            request.SortBy = ParseString(query, new[] { "sortBy", "SortBy" });
            request.SortDirection = ParseString(query, new[] { "sortDirection", "SortDirection" }) ?? "desc";
            request.Filters = ParseJsonFilters(query) ?? ParseIndexedFilters(query) ?? new List<Filter>();

            var filterLogic = ParseString(query, new[] { "filterLogic", "FilterLogic" });
            request.FilterLogic = filterLogic ?? "and";

            BindDerivedBooleanProperties(request, query);
            BindDerivedNullableLongProperties(request, query);
            BindDerivedNullableDateTimeProperties(request, query);

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
                using var document = await JsonDocument
                    .ParseAsync(httpRequest.Body)
                    .ConfigureAwait(false);
                var parsed = document.RootElement.Deserialize(bindingContext.ModelType, JsonOptions);

                if (httpRequest.Body.CanSeek)
                {
                    httpRequest.Body.Position = 0;
                }

                if (parsed is PagedRequest request && document.RootElement.ValueKind == JsonValueKind.Object &&
                    document.RootElement.EnumerateObject().Any(property =>
                        property.Name.Equals("searchFields", StringComparison.OrdinalIgnoreCase)))
                {
                    request.MarkSearchFieldsSpecified();
                }

                return parsed as PagedRequest;
            }
            catch (JsonException exception)
            {
                if (httpRequest.Body.CanSeek)
                {
                    httpRequest.Body.Position = 0;
                }

                throw new PagedQueryValidationException($"Paged request JSON gövdesi geçersiz: {exception.Message}");
            }
        }

        private static bool CanReadJsonBody(HttpRequest request)
        {
            var isPagedPostRewrite = request.HttpContext.Items.ContainsKey("Aqua.PagedPostRewrite");
            if ((HttpMethods.IsGet(request.Method) && !isPagedPostRewrite) || HttpMethods.IsHead(request.Method))
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
            request.Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
            request.SearchFields = (request.SearchFields ?? new List<string>())
                .Select(field => field?.Trim() ?? string.Empty)
                .ToList();
            request.SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? null : request.SortBy.Trim();
            request.SortDirection = string.IsNullOrWhiteSpace(request.SortDirection) ? "desc" : request.SortDirection.Trim();
            request.FilterLogic = string.IsNullOrWhiteSpace(request.FilterLogic) ? "and" : request.FilterLogic.Trim();
            request.Filters = request.Filters?
                .Select(filter => new Filter
                {
                    Column = filter.Column?.Trim() ?? string.Empty,
                    Operator = filter.Operator?.Trim() ?? string.Empty,
                    Value = filter.Value?.Trim()
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

        private static void BindDerivedNullableDateTimeProperties(PagedRequest request, Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var requestType = request.GetType();
            foreach (var property in requestType.GetProperties().Where(property => property.PropertyType == typeof(DateTime?)))
            {
                var rawValue = ParseString(query, new[] { ToCamelCase(property.Name), property.Name });
                if (rawValue is null)
                {
                    continue;
                }

                if (!DateTime.TryParse(
                        rawValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                        out var parsed))
                {
                    throw new PagedQueryValidationException($"'{ToCamelCase(property.Name)}' ISO tarih değeri olmalıdır.");
                }

                property.SetValue(request, parsed);
            }
        }

        private static string ToCamelCase(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : char.ToLowerInvariant(value[0]) + value[1..];
        }

        private static int ParseInt(
            Microsoft.AspNetCore.Http.IQueryCollection query,
            IEnumerable<string> keys,
            int fallback,
            string parameterName)
        {
            var raw = ParseString(query, keys);
            if (raw is null)
            {
                return fallback;
            }

            return int.TryParse(raw, out var value)
                ? value
                : throw new PagedQueryValidationException($"'{parameterName}' tam sayı olmalıdır.");
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

        private static List<string> ParseStringList(
            Microsoft.AspNetCore.Http.IQueryCollection query,
            IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (!query.TryGetValue(key, out var values) || StringValues.IsNullOrEmpty(values))
                {
                    continue;
                }

                return values
                    .SelectMany(value => (value ?? string.Empty).Split(','))
                    .Select(value => value.Trim())
                    .ToList();
            }

            return new List<string>();
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

                return parsed;
            }
            catch (JsonException exception)
            {
                throw new PagedQueryValidationException($"'filters' JSON değeri geçersiz: {exception.Message}");
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

                filters.Add(new Filter
                {
                    Column = column ?? string.Empty,
                    Operator = filterOperator ?? string.Empty,
                    Value = value
                });
            }

            return filters.Count == 0 ? null : filters;
        }
    }
}
