using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace aqua_api.Shared.Common.Helpers
{
    public static class QueryHelper
    {
        private static readonly char[] SearchSeparators = [' ', '\t', '\r', '\n'];
        private static readonly char[] SearchTermTokenSeparators =
        [
            ' ', '\t', '\r', '\n', '.', ',', '-', '_', '/', '\\', '\'', '"', '’', '`',
            '(', ')', '[', ']', '{', '}', '&', '+', ':', ';', '!', '?', '|'
        ];

        public static readonly string[] CommonSearchableColumns =
        {
            "Name",
            "Title",
            "TitleName",
            "Description",
            "Code",
            "ERPCode",
            "ErpCode",
            "Value",
            "Type",
            "Status",
            "Reason",
            "Comment",
            "Comments",
            "Notes",
            "Text",
            "Content",
            "Message",
            "Subject",
            "Username",
            "Email",
            "FirstName",
            "LastName",
            "FullName",
            "Phone",
            "Phone1",
            "Phone2",
            "Address",
            "Website",
            "Url",
            "Path",
            "ProjectCode",
            "ProjectName",
            "CageCode",
            "CageName",
            "WarehouseCode",
            "WarehouseName",
            "BatchCode",
            "StockCode",
            "StockName",
            "ErpStockCode",
            "StokCode",
            "Barcode",
            "CurrencyCode",
            "CurrencyName",
            "Currency",
            "GrupKodu",
            "GrupAdi",
            "Kod1",
            "Kod1Adi",
            "Kod2",
            "Kod2Adi",
            "Kod3",
            "Kod3Adi",
            "Kod4",
            "Kod4Adi",
            "Kod5",
            "Kod5Adi",
            "SerialPrefix",
            "PermissionKey",
            "DisplayName",
            "SessionId",
            "IpAddress",
            "Browser",
            "Platform",
            "DeviceName",
            "RoleNavigation.Title",
            "PermissionDefinition.Name",
            "PermissionDefinition.Code",
            "Group.Name",
            "User.Username",
            "User.Email",
            "User.FirstName",
            "User.LastName",
            "Stock.StockCode",
            "Stock.StockName",
            "Stock.ErpStockCode",
            "Project.ProjectCode",
            "Project.ProjectName",
            "FishBatch.BatchCode",
            "FishBatch.Project.ProjectCode",
            "FishBatch.Project.ProjectName",
            "FishBatch.FishStock.ErpStockCode",
            "FishBatch.FishStock.StockName",
            "ProjectCage.Cage.CageCode",
            "ProjectCage.Cage.CageName",
            "Warehouse.WarehouseCode",
            "Warehouse.WarehouseName"
        };

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

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search, params string[] searchableColumns)
        {
            if (string.IsNullOrWhiteSpace(search) || searchableColumns.Length == 0)
            {
                return query;
            }

            var useEfSearch = IsEntityFrameworkQuery(query);
            var terms = BuildSearchTerms(search, includeCompoundTerm: !useEfSearch);

            if (terms.Count == 0)
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? searchPredicate = null;

            foreach (var term in terms)
            {
                Expression? termPredicate = null;

                foreach (var column in searchableColumns)
                {
                    var resolved = ResolvePropertyPath(parameter, typeof(T), column);
                    if (resolved == null || resolved.Value.property.PropertyType != typeof(string))
                    {
                        continue;
                    }

                    var member = resolved.Value.expression;
                    var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                    var columnPredicate = useEfSearch
                        ? BuildSqlServerSearchPredicate(member, term.Raw, term.Normalized)
                        : BuildInMemorySearchPredicate(member, term.Normalized);
                    var currentPredicate = Expression.AndAlso(notNull, columnPredicate);

                    termPredicate = termPredicate == null
                        ? currentPredicate
                        : Expression.OrElse(termPredicate, currentPredicate);
                }

                if (termPredicate == null)
                {
                    continue;
                }

                searchPredicate = searchPredicate == null
                    ? termPredicate
                    : Expression.AndAlso(searchPredicate, termPredicate);
            }

            if (searchPredicate == null)
            {
                return query;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(searchPredicate, parameter);
            return query.Where(lambda);
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

            return predicate ?? Expression.Constant(true);
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

        private static string ResolveColumnName(string column, IReadOnlyDictionary<string, string>? columnMapping)
        {
            if (columnMapping == null) return column;
            var mappingKey = columnMapping.Keys.FirstOrDefault(k => string.Equals(k, column, StringComparison.OrdinalIgnoreCase));
            return mappingKey != null ? columnMapping[mappingKey] : column;
        }

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<Filter>? filters, string filterLogic = "and", IReadOnlyDictionary<string, string>? columnMapping = null)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression? basePredicate = null;
            var useEfSearch = IsEntityFrameworkQuery(query);

            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null && (isDeletedProperty.PropertyType == typeof(bool) || isDeletedProperty.PropertyType == typeof(bool?)))
            {
                var isDeletedLeft = Expression.Property(param, isDeletedProperty);
                var isDeletedExp = Expression.Equal(isDeletedLeft, Expression.Constant(false));
                basePredicate = isDeletedExp;
            }

            if (filters == null || filters.Count == 0)
            {
                if (basePredicate == null) return query;
                var defaultLambda = Expression.Lambda<Func<T, bool>>(basePredicate, param);
                return query.Where(defaultLambda);
            }

            bool useOr = string.Equals(filterLogic, "or", StringComparison.OrdinalIgnoreCase);
            Expression? filterPredicate = null;

            foreach (var filter in filters)
            {
                if (string.IsNullOrEmpty(filter.Value)) continue;

                var columnName = ResolveColumnName(filter.Column, columnMapping);

                var resolved = ResolvePropertyPath(param, typeof(T), columnName);
                if (resolved == null) continue;
                var (left, property) = resolved.Value;

                Expression? exp = null;

                var operatorLower = filter.Operator.ToLowerInvariant();

                if (property.PropertyType == typeof(string))
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
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    if (int.TryParse(filter.Value, out int val))
                    {
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, Expression.Constant(val)),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, Expression.Constant(val)),
                            "<" or "lt" => Expression.LessThan(left, Expression.Constant(val)),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, Expression.Constant(val)),
                            _ => Expression.Equal(left, Expression.Constant(val))
                        };
                    }
                }
                else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                {
                    if (long.TryParse(filter.Value, out long val))
                    {
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, Expression.Constant(val)),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, Expression.Constant(val)),
                            "<" or "lt" => Expression.LessThan(left, Expression.Constant(val)),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, Expression.Constant(val)),
                            _ => Expression.Equal(left, Expression.Constant(val))
                        };
                    }
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    if (decimal.TryParse(filter.Value, out decimal val))
                    {
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, Expression.Constant(val)),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, Expression.Constant(val)),
                            "<" or "lt" => Expression.LessThan(left, Expression.Constant(val)),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, Expression.Constant(val)),
                            _ => Expression.Equal(left, Expression.Constant(val))
                        };
                    }
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(filter.Value, out DateTime val))
                    {
                        exp = operatorLower switch
                        {
                            ">" or "gt" => Expression.GreaterThan(left, Expression.Constant(val)),
                            ">=" or "gte" => Expression.GreaterThanOrEqual(left, Expression.Constant(val)),
                            "<" or "lt" => Expression.LessThan(left, Expression.Constant(val)),
                            "<=" or "lte" => Expression.LessThanOrEqual(left, Expression.Constant(val)),
                            _ => Expression.Equal(left, Expression.Constant(val))
                        };
                    }
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    if (bool.TryParse(filter.Value, out bool val))
                    {
                        exp = Expression.Equal(left, Expression.Constant(val));
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    if (Enum.TryParse(property.PropertyType, filter.Value, true, out var enumVal))
                    {
                        exp = Expression.Equal(left, Expression.Constant(enumVal));
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

            Expression? finalPredicate;
            if (basePredicate != null && filterPredicate != null)
                finalPredicate = Expression.AndAlso(basePredicate, filterPredicate);
            else
                finalPredicate = basePredicate ?? filterPredicate;

            if (finalPredicate == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalPredicate, param);
            return query.Where(lambda);
        }

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

        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            int skip = (pageNumber - 1) * pageSize;
            return query.Skip(skip).Take(pageSize);
        }

        public static IQueryable<T> ApplyPagedRequest<T>(this IQueryable<T> query, PagedRequest request, IReadOnlyDictionary<string, string>? columnMapping = null)
        {
            if (request == null) return query;

            query = query.ApplySearch(request.Search);
            query = query.ApplyFilters(request.Filters, request.FilterLogic, columnMapping);
            query = query.ApplySorting(request.SortBy, request.SortDirection, columnMapping);
            query = query.ApplyPagination(request.PageNumber, request.PageSize);

            return query;
        }
    }
}
