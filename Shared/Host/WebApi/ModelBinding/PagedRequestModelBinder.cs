using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace aqua_api.Shared.Host.WebApi.ModelBinding
{
    public class PagedRequestModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.Query;
            var request = new PagedRequest
            {
                PageNumber = ParseInt(query, new[] { "pageNumber", "PageNumber" }, 1),
                PageSize = ParseInt(query, new[] { "pageSize", "PageSize" }, 20),
                SortBy = ParseString(query, new[] { "sortBy", "SortBy" }) ?? "Id",
                SortDirection = ParseString(query, new[] { "sortDirection", "SortDirection" }) ?? "desc"
            };

            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            request.Filters = ParseJsonFilters(query) ?? ParseIndexedFilters(query) ?? new List<Filter>();

            var filterLogic = ParseString(query, new[] { "filterLogic", "FilterLogic" });
            request.FilterLogic = string.Equals(filterLogic, "or", StringComparison.OrdinalIgnoreCase) ? "or" : "and";

            bindingContext.Result = ModelBindingResult.Success(request);
            return Task.CompletedTask;
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
