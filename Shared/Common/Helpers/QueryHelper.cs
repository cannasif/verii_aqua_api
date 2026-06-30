using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace aqua_api.Shared.Common.Helpers
{
    public static class QueryHelper
    {
        public const string GlobalSearchFilterColumn = "__search";
        private static readonly char[] SearchSeparators = [' ', '\t', '\r', '\n'];
        private static readonly char[] SearchTermTokenSeparators =
        [
            ' ', '\t', '\r', '\n', '.', ',', '-', '_', '/', '\\', '\'', '"', '’', '`',
            '(', ')', '[', ']', '{', '}', '&', '+', ':', ';', '!', '?', '|'
        ];

        public sealed record SearchTerm(string Raw, string Normalized);

        public static List<SearchTerm> BuildSearchTerms(string? search, bool includeCompoundTerm = true)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return new List<SearchTerm>();
            }

            var terms = new List<SearchTerm>();
            foreach (var chunk in search.Trim()
                .Split(SearchSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(term => !string.IsNullOrWhiteSpace(term)))
            {
                var tokens = chunk
                    .Split(SearchTermTokenSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (includeCompoundTerm || tokens.Count <= 1)
                {
                    AddSearchTerm(terms, chunk);
                    if (tokens.Count <= 1)
                    {
                        continue;
                    }
                }

                foreach (var token in tokens)
                {
                    AddSearchTerm(terms, token);
                }
            }

            return terms;
        }

        public static List<string> BuildNormalizedSearchTerms(string? search)
        {
            return BuildSearchTerms(search)
                .Select(term => term.Normalized)
                .Where(term => !string.IsNullOrWhiteSpace(term))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().ToLower(new CultureInfo("tr-TR")).Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                var mapped = ch switch
                {
                    'ı' => 'i',
                    'ğ' => 'g',
                    'ü' => 'u',
                    'ş' => 's',
                    'ö' => 'o',
                    'ç' => 'c',
                    _ => ch
                };

                if (char.IsLetterOrDigit(mapped))
                {
                    builder.Append(mapped);
                }
            }

            return builder.ToString();
        }

        private static void AddSearchTerm(ICollection<SearchTerm> terms, string raw)
        {
            var normalized = NormalizeSearchText(raw);
            if (string.IsNullOrWhiteSpace(raw) && string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            if (terms.Any(term =>
                string.Equals(term.Raw, raw, StringComparison.Ordinal) &&
                string.Equals(term.Normalized, normalized, StringComparison.Ordinal)))
            {
                return;
            }

            terms.Add(new SearchTerm(raw, normalized));
        }

        private static bool IsEntityFrameworkQuery<T>(IQueryable<T> query)
        {
            return query.Provider.GetType().Namespace?.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static Expression BuildInMemorySearchPredicate(Expression member, string normalizedTerm)
        {
            if (string.IsNullOrWhiteSpace(normalizedTerm))
            {
                return Expression.Constant(false);
            }

            var normalizeMethod = typeof(QueryHelper).GetMethod(nameof(NormalizeSearchText), BindingFlags.Public | BindingFlags.Static)!;
            var normalizedMember = Expression.Call(normalizeMethod, member);
            return Expression.Call(
                normalizedMember,
                typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                Expression.Constant(normalizedTerm));
        }

        private static Expression BuildSqlServerSearchPredicate(Expression member, string rawTerm, string normalizedTerm)
        {
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            Expression? predicate = null;
            if (!string.IsNullOrWhiteSpace(rawTerm))
            {
                predicate = Expression.Call(member, containsMethod, Expression.Constant(rawTerm.Trim()));
            }

            return predicate ?? Expression.Constant(!string.IsNullOrWhiteSpace(normalizedTerm));
        }

        private static Expression BuildStringFilterPredicate(Expression member, string rawValue, string normalizedValue, string operatorLower, bool useEfSearch)
        {
            if (string.IsNullOrWhiteSpace(rawValue) && string.IsNullOrWhiteSpace(normalizedValue))
            {
                return Expression.Constant(true);
            }

            var methodName = operatorLower switch
            {
                "contains" => nameof(string.Contains),
                "startswith" => nameof(string.StartsWith),
                "endswith" => nameof(string.EndsWith),
                _ => null
            };

            if (methodName == null)
            {
                return Expression.Equal(member, Expression.Constant(rawValue));
            }

            var method = typeof(string).GetMethod(methodName, new[] { typeof(string) })!;

            if (!useEfSearch)
            {
                var normalizeMethod = typeof(QueryHelper).GetMethod(nameof(NormalizeSearchText), BindingFlags.Public | BindingFlags.Static)!;
                var normalizedMember = Expression.Call(normalizeMethod, member);
                return Expression.Call(normalizedMember, method, Expression.Constant(normalizedValue));
            }

            Expression? predicate = null;

            if (!string.IsNullOrWhiteSpace(rawValue))
            {
                if (operatorLower == "contains")
                {
                    foreach (var term in BuildSearchTerms(rawValue, includeCompoundTerm: false))
                    {
                        if (string.IsNullOrWhiteSpace(term.Raw))
                        {
                            continue;
                        }

                        var termPredicate = Expression.Call(member, method, Expression.Constant(term.Raw.Trim()));
                        predicate = predicate == null ? termPredicate : Expression.AndAlso(predicate, termPredicate);
                    }
                }
                else
                {
                    predicate = Expression.Call(member, method, Expression.Constant(rawValue.Trim()));
                }
            }

            return predicate ?? Expression.Constant(true);
        }

        /// <summary>
        /// Resolves a property path (possibly dot-notation like "Country.Name")
        /// to a chain of Expression.Property calls.
        /// Returns null if any part of the path does not exist.
        /// </summary>
        private static (Expression expression, PropertyInfo property)? ResolvePropertyPath(Expression param, Type rootType, string path)
        {
            var parts = path.Split('.');
            Expression current = param;
            PropertyInfo? prop = null;
            Type currentType = rootType;

            foreach (var part in parts)
            {
                prop = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;
                current = Expression.Property(current, prop);
                currentType = prop.PropertyType;
            }

            return prop == null ? null : (current, prop);
        }

        /// <summary>
        /// Resolves the actual entity column name from DTO/frontend column name using the columnMapping dictionary.
        /// </summary>
        private static string ResolveColumnName(string column, IReadOnlyDictionary<string, string>? columnMapping)
        {
            if (columnMapping == null) return column;
            var mappingKey = columnMapping.Keys.FirstOrDefault(k => string.Equals(k, column, StringComparison.OrdinalIgnoreCase));
            return mappingKey != null ? columnMapping[mappingKey] : column;
        }

        private static Type GetNonNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static Expression CreateTypedConstant(Type propertyType, object value)
        {
            var targetType = GetNonNullableType(propertyType);
            var constant = Expression.Constant(value, targetType);
            return targetType == propertyType ? constant : Expression.Convert(constant, propertyType);
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)
                   || decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
        }

        private static bool TryParseDateTime(string value, out DateTime result)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
                   || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result);
        }

        /// <summary>
        /// Applies filters to the query based on Filter list.
        /// columnMapping: DTO/frontend column names to entity property names (e.g. "name" -> "CustomerName", "countryName" -> "Country.Name").
        /// Supports dot-notation for navigation properties (left join / Include).
        /// </summary>
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<Filter>? filters, string filterLogic = "and", IReadOnlyDictionary<string, string>? columnMapping = null)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression? basePredicate = null;
            var useEfSearch = IsEntityFrameworkQuery(query);

            // Default filter: IsDeleted = false (if IsDeleted property exists)
            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null && (isDeletedProperty.PropertyType == typeof(bool) || isDeletedProperty.PropertyType == typeof(bool?)))
            {
                var isDeletedLeft = Expression.Property(param, isDeletedProperty);
                var isDeletedExp = Expression.Equal(isDeletedLeft, Expression.Constant(false));
                basePredicate = isDeletedExp;
            }

            var globalSearchValues = filters?
                .Where(filter => string.Equals(filter.Column, GlobalSearchFilterColumn, StringComparison.OrdinalIgnoreCase) &&
                                 !string.IsNullOrWhiteSpace(filter.Value))
                .Select(filter => filter.Value)
                .ToList() ?? new List<string>();

            var regularFilters = filters?
                .Where(filter => !string.Equals(filter.Column, GlobalSearchFilterColumn, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if ((regularFilters == null || regularFilters.Count == 0) && globalSearchValues.Count == 0)
            {
                if (basePredicate == null) return query;
                var defaultLambda = Expression.Lambda<Func<T, bool>>(basePredicate, param);
                return query.Where(defaultLambda);
            }

            bool useOr = string.Equals(filterLogic, "or", StringComparison.OrdinalIgnoreCase);
            Expression? filterPredicate = null;

            foreach (var filter in regularFilters ?? new List<Filter>())
            {
                if (string.IsNullOrEmpty(filter.Value)) continue;

                var columnName = ResolveColumnName(filter.Column, columnMapping);

                var resolved = ResolvePropertyPath(param, typeof(T), columnName);
                if (resolved == null) continue;
                var (left, property) = resolved.Value;

                Expression? exp = null;

                var operatorLower = filter.Operator.ToLowerInvariant();

                var propertyType = GetNonNullableType(property.PropertyType);

                if (propertyType == typeof(string))
                {
                    exp = BuildStringFilterPredicate(
                        left,
                        filter.Value,
                        NormalizeSearchText(filter.Value),
                        operatorLower,
                        useEfSearch);
                    exp = Expression.AndAlso(
                        Expression.NotEqual(left, Expression.Constant(null, typeof(string))),
                        exp);
                }
                else if (propertyType == typeof(int))
                {
                    if (int.TryParse(filter.Value, out int val))
                    {
                        var typedValue = CreateTypedConstant(property.PropertyType, val);
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, typedValue),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, typedValue),
                            "<" or "lt" => Expression.LessThan(left, typedValue),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, typedValue),
                            _ => Expression.Equal(left, typedValue)
                        };
                    }
                }
                else if (propertyType == typeof(long))
                {
                    if (long.TryParse(filter.Value, out long val))
                    {
                        var typedValue = CreateTypedConstant(property.PropertyType, val);
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, typedValue),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, typedValue),
                            "<" or "lt" => Expression.LessThan(left, typedValue),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, typedValue),
                            _ => Expression.Equal(left, typedValue)
                        };
                    }
                }
                else if (propertyType == typeof(decimal))
                {
                    if (TryParseDecimal(filter.Value, out decimal val))
                    {
                        var typedValue = CreateTypedConstant(property.PropertyType, val);
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, typedValue),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, typedValue),
                            "<" or "lt" => Expression.LessThan(left, typedValue),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, typedValue),
                            _ => Expression.Equal(left, typedValue)
                        };
                    }
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (TryParseDateTime(filter.Value, out DateTime val))
                    {
                        var typedValue = CreateTypedConstant(property.PropertyType, val);
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, typedValue),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, typedValue),
                            "<" or "lt" => Expression.LessThan(left, typedValue),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, typedValue),
                            _ => Expression.Equal(left, typedValue)
                        };
                    }
                }
                else if (propertyType == typeof(bool))
                {
                    if (bool.TryParse(filter.Value, out bool val))
                    {
                        exp = Expression.Equal(left, CreateTypedConstant(property.PropertyType, val));
                    }
                }
                else if (propertyType.IsEnum)
                {
                    if (Enum.TryParse(propertyType, filter.Value, true, out var enumVal) && enumVal != null)
                    {
                        exp = Expression.Equal(left, CreateTypedConstant(property.PropertyType, enumVal));
                    }
                }

                if (exp != null)
                {
                    filterPredicate = filterPredicate == null
                        ? exp
                        : useOr
                            ? Expression.OrElse(filterPredicate, exp)
                            : Expression.AndAlso(filterPredicate, exp);
                }
            }

            Expression? searchPredicate = null;
            foreach (var searchValue in globalSearchValues)
            {
                searchPredicate = CombinePredicates(
                    searchPredicate,
                    BuildSearchPredicate(param, typeof(T), searchValue, useEfSearch),
                    useOr: false);
            }

            // Combine: basePredicate (IsDeleted) is always AND, filterPredicate is AND/OR based on filterLogic
            Expression? finalPredicate;
            finalPredicate = CombinePredicates(basePredicate, filterPredicate, useOr: false);
            finalPredicate = CombinePredicates(finalPredicate, searchPredicate, useOr: false);

            if (finalPredicate == null) return query;
            
            var lambda = Expression.Lambda<Func<T, bool>>(finalPredicate, param);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search, params string[] searchableColumns)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var predicate = BuildSearchPredicate(parameter, typeof(T), search, IsEntityFrameworkQuery(query), searchableColumns);
            if (predicate == null)
            {
                return query;
            }

            return query.Where(Expression.Lambda<Func<T, bool>>(predicate, parameter));
        }

        public static List<Filter> WithoutGlobalSearchFilter(IEnumerable<Filter>? filters)
        {
            return filters?
                .Where(filter => !string.Equals(filter.Column, GlobalSearchFilterColumn, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<Filter>();
        }

        private static Expression? BuildSearchPredicate(Expression parameter, Type rootType, string search, bool useEfSearch, params string[] searchableColumns)
        {
            var terms = BuildSearchTerms(search, includeCompoundTerm: false);

            if (terms.Count == 0)
            {
                return null;
            }

            var columns = searchableColumns.Length > 0
                ? searchableColumns
                : rootType
                    .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.PropertyType == typeof(string))
                    .Select(property => property.Name)
                    .ToArray();

            if (columns.Length == 0)
            {
                return null;
            }

            Expression? combinedTerms = null;
            foreach (var term in terms)
            {
                Expression? combinedColumns = null;
                foreach (var column in columns)
                {
                    var resolved = ResolvePropertyPath(parameter, rootType, column);
                    if (resolved == null || resolved.Value.property.PropertyType != typeof(string))
                    {
                        continue;
                    }

                    var member = resolved.Value.expression;
                    var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                    var contains = useEfSearch
                        ? BuildSqlServerSearchPredicate(member, term.Raw, term.Normalized)
                        : BuildInMemorySearchPredicate(member, term.Normalized);
                    var columnPredicate = Expression.AndAlso(notNull, contains);

                    combinedColumns = CombinePredicates(combinedColumns, columnPredicate, useOr: true);
                }

                combinedTerms = CombinePredicates(combinedTerms, combinedColumns, useOr: false);
            }

            return combinedTerms;
        }

        private static Expression? CombinePredicates(Expression? left, Expression? right, bool useOr)
        {
            if (left == null) return right;
            if (right == null) return left;
            return useOr ? Expression.OrElse(left, right) : Expression.AndAlso(left, right);
        }

        /// <summary>
        /// Applies sorting to the query.
        /// Supports columnMapping and dot-notation for navigation properties (e.g. "countryName" -> "Country.Name").
        /// </summary>
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string? sortDirection, IReadOnlyDictionary<string, string>? columnMapping = null)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                sortBy = "Id";
            }

            var resolvedSortBy = ResolveColumnName(sortBy, columnMapping);

            var parameter = Expression.Parameter(typeof(T), "x");
            var resolved = ResolvePropertyPath(parameter, typeof(T), resolvedSortBy);
            if (resolved == null)
            {
                // Fallback to Id if resolved property doesn't exist
                resolved = ResolvePropertyPath(parameter, typeof(T), "Id");
                if (resolved == null) return query;
            }

            var (member, _) = resolved.Value;
            var keySelector = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(T), member.Type),
                member,
                parameter
            );

            bool isDescending = string.IsNullOrWhiteSpace(sortDirection) 
                ? false 
                : sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) 
                   || sortDirection.Equals("descending", StringComparison.OrdinalIgnoreCase);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var call = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), member.Type },
                query.Expression,
                keySelector
            );
            
            return query.Provider.CreateQuery<T>(call);
        }

        /// <summary>
        /// Applies pagination to the query (1-based page number)
        /// </summary>
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            
            int skip = (pageNumber - 1) * pageSize;
            return query.Skip(skip).Take(pageSize);
        }

        /// <summary>
        /// Applies all PagedRequest operations (filters, sorting, pagination) to the query.
        /// Supports columnMapping for DTO-to-entity property name resolution.
        /// </summary>
        public static IQueryable<T> ApplyPagedRequest<T>(this IQueryable<T> query, PagedRequest request, IReadOnlyDictionary<string, string>? columnMapping = null)
        {
            if (request == null) return query;

            if (!string.IsNullOrWhiteSpace(request.Search) &&
                (request.Filters == null || !request.Filters.Any(filter => string.Equals(filter.Column, GlobalSearchFilterColumn, StringComparison.OrdinalIgnoreCase))))
            {
                query = query.ApplySearch(request.Search);
            }
            query = query.ApplyFilters(request.Filters, request.FilterLogic, columnMapping);
            query = query.ApplySorting(request.SortBy, request.SortDirection, columnMapping);
            query = query.ApplyPagination(request.PageNumber, request.PageSize);

            return query;
        }
    }
}
