using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using aqua_api.Shared.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace aqua_api.Shared.Common.Helpers;

public static class QueryHelper
{
    public const string SearchCollation = "Latin1_General_100_CI_AS";
    private const int MaximumFilterCount = 20;
    private const int MaximumSearchFieldCount = 12;
    private const int MaximumSearchTermCount = 10;
    private const int MaximumColumnLength = 100;
    private const int MaximumOperatorLength = 30;
    private const int MaximumFilterValueLength = 500;

    public sealed record SearchTerm(string Raw, string Normalized);

    public static void ValidateRequestContract(PagedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        PagedQueryExtensions.ValidateRequest(request);

        _ = ParseFilterLogic(request.FilterLogic);
        _ = ParseSortDirection(request.SortDirection);

        var fields = request.SearchFields ?? [];
        if (fields.Count > MaximumSearchFieldCount)
        {
            throw Invalid($"En fazla {MaximumSearchFieldCount} arama alanı seçilebilir.");
        }

        if (!string.IsNullOrWhiteSpace(request.Search) && request.SearchFieldsSpecified && fields.Count == 0)
        {
            throw Invalid("Arama yapılırken en az bir arama alanı seçilmelidir.");
        }

        if (fields.Any(string.IsNullOrWhiteSpace))
        {
            throw Invalid("Arama alanı boş olamaz.");
        }

        if (fields.Any(field => field.Length > MaximumColumnLength))
        {
            throw Invalid($"Arama alanı en fazla {MaximumColumnLength} karakter olabilir.");
        }

        if (BuildSearchTerms(request.Search).Count > MaximumSearchTermCount)
        {
            throw Invalid($"Arama metni en fazla {MaximumSearchTermCount} kelime içerebilir.");
        }

        var filters = request.Filters ?? [];
        if (filters.Count > MaximumFilterCount)
        {
            throw Invalid($"En fazla {MaximumFilterCount} gelişmiş filtre uygulanabilir.");
        }

        for (var index = 0; index < filters.Count; index++)
        {
            var filter = filters[index];
            if (string.IsNullOrWhiteSpace(filter.Column))
            {
                throw InvalidFilter(index, "kolon adı zorunludur");
            }

            if (filter.Column.Length > MaximumColumnLength)
            {
                throw InvalidFilter(index, $"kolon adı en fazla {MaximumColumnLength} karakter olabilir");
            }

            if (string.IsNullOrWhiteSpace(filter.Operator))
            {
                throw InvalidFilter(index, "operatör zorunludur");
            }

            if (filter.Operator.Length > MaximumOperatorLength)
            {
                throw InvalidFilter(index, $"operatör en fazla {MaximumOperatorLength} karakter olabilir");
            }

            if (filter.Value?.Length > MaximumFilterValueLength)
            {
                throw InvalidFilter(index, $"değer en fazla {MaximumFilterValueLength} karakter olabilir");
            }

            var operation = ParseOperator(filter.Operator, index);
            if (operation is not (FilterOperation.IsNull or FilterOperation.IsNotNull) && filter.Value is null)
            {
                throw InvalidFilter(index, $"'{filter.Column}' filtresi için değer zorunludur");
            }
        }
    }

    public static List<SearchTerm> BuildSearchTerms(string? search, bool includeCompoundTerm = true)
    {
        _ = includeCompoundTerm;
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        return search.Trim()
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(term => new SearchTerm(term, NormalizeSearchText(term)))
            .ToList();
    }

    public static List<string> BuildNormalizedSearchTerms(string? search) =>
        BuildSearchTerms(search)
            .Select(term => term.Normalized)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    public static string NormalizeSearchText(string? value) =>
        AsciiTurkishSearch.Fold(value?.Trim() ?? string.Empty);

    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? search,
        params string[] searchableColumns)
    {
        if (!string.IsNullOrWhiteSpace(search) && searchableColumns.Length == 0)
        {
            throw Invalid("Genel arama için aranabilir kolon allowlist'i tanımlanmalıdır.");
        }

        var mapping = CreateColumnMapping<T>(searchableColumns);
        var request = new PagedRequest
        {
            Search = search,
            SearchFields = searchableColumns.Length == 0 ? [] : mapping.Keys.ToList()
        };
        return ApplySearch(query, request, mapping, mapping.Keys.ToArray());
    }

    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        PagedRequest request,
        params string[] defaultSearchableColumns)
    {
        var mapping = CreateColumnMapping<T>(defaultSearchableColumns);
        return ApplySearch(query, request, mapping, mapping.Keys.ToArray());
    }

    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        PagedRequest request,
        IReadOnlyDictionary<string, string> columnMapping,
        IReadOnlyCollection<string>? defaultColumns = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(columnMapping);

        request.MarkSearchApplied();
        var search = request.Search?.Trim();
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        if (columnMapping.Count == 0)
        {
            throw Invalid("En az bir aranabilir kolon tanımlanmalıdır.");
        }

        var requestedColumns = request.SearchFields
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Select(field => field.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (requestedColumns.Length == 0)
        {
            requestedColumns = (defaultColumns is { Count: > 0 } ? defaultColumns : columnMapping.Keys)
                .Where(field => !string.IsNullOrWhiteSpace(field))
                .Select(field => field.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (request.SearchFields.Count > 0 && requestedColumns.Length > MaximumSearchFieldCount)
        {
            throw Invalid($"En fazla {MaximumSearchFieldCount} arama alanı seçilebilir.");
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var members = requestedColumns.Select(column =>
        {
            if (column.Length > MaximumColumnLength)
            {
                throw Invalid($"Arama alanı en fazla {MaximumColumnLength} karakter olabilir.");
            }

            var path = ResolveColumn(column, columnMapping, "aranabilir", allowDirectTopLevel: true);
            var resolved = ResolvePath(parameter, typeof(T), path)
                ?? throw Invalid($"'{column}' aranabilir bir kolon değildir.");
            if (!SupportsGeneralSearch(resolved.property.PropertyType))
            {
                throw Invalid($"'{column}' genel aramayı destekleyen bir kolon değildir.");
            }

            return resolved.member;
        }).ToArray();

        var terms = BuildSearchTerms(search);
        if (terms.Count > MaximumSearchTermCount)
        {
            throw Invalid($"Arama metni en fazla {MaximumSearchTermCount} kelime içerebilir.");
        }

        var useSqlPattern = query.Provider is IAsyncQueryProvider;
        Expression? allTerms = null;
        foreach (var term in terms)
        {
            Expression? anyColumn = null;
            foreach (var member in members)
            {
                var current = BuildGeneralSearchMatch(member, term.Raw, useSqlPattern);
                if (current is null)
                {
                    continue;
                }

                anyColumn = anyColumn is null ? current : Expression.OrElse(anyColumn, current);
            }

            anyColumn ??= Expression.Constant(false);
            allTerms = allTerms is null ? anyColumn : Expression.AndAlso(allTerms, anyColumn);
        }

        return allTerms is null
            ? query
            : query.Where(Expression.Lambda<Func<T, bool>>(allTerms, parameter));
    }

    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        List<Filter>? filters,
        string filterLogic = "and",
        IReadOnlyDictionary<string, string>? columnMapping = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        var list = filters?.ToList() ?? [];
        if (list.Count > MaximumFilterCount)
        {
            throw Invalid($"En fazla {MaximumFilterCount} gelişmiş filtre uygulanabilir.");
        }

        var logic = ParseFilterLogic(filterLogic);
        if (list.Count == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;
        for (var index = 0; index < list.Count; index++)
        {
            var filter = list[index];
            if (string.IsNullOrWhiteSpace(filter.Column))
            {
                throw InvalidFilter(index, "kolon adı zorunludur");
            }

            if (filter.Column.Length > MaximumColumnLength)
            {
                throw InvalidFilter(index, $"kolon adı en fazla {MaximumColumnLength} karakter olabilir");
            }

            if (filter.Operator?.Length > MaximumOperatorLength)
            {
                throw InvalidFilter(index, $"operatör en fazla {MaximumOperatorLength} karakter olabilir");
            }

            if (filter.Value?.Length > MaximumFilterValueLength)
            {
                throw InvalidFilter(index, $"değer en fazla {MaximumFilterValueLength} karakter olabilir");
            }

            var path = ResolveColumn(filter.Column.Trim(), columnMapping, "filtrelenebilir", index, allowDirectTopLevel: true);
            var resolved = ResolvePath(parameter, typeof(T), path)
                ?? throw InvalidFilter(index, $"'{filter.Column}' filtrelenebilir bir kolon değildir");
            var current = BuildFilter(resolved.member, resolved.property.PropertyType, filter, index);
            combined = combined is null
                ? current
                : logic == FilterLogic.Or
                    ? Expression.OrElse(combined, current)
                    : Expression.AndAlso(combined, current);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(combined!, parameter));
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? sortDirection,
        IReadOnlyDictionary<string, string>? columnMapping = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (sortBy?.Length > MaximumColumnLength)
        {
            throw Invalid($"Sıralama kolonu en fazla {MaximumColumnLength} karakter olabilir.");
        }

        var descending = ParseSortDirection(sortDirection);
        var requestedColumn = string.IsNullOrWhiteSpace(sortBy) ? "Id" : sortBy.Trim();
        var path = ResolveColumn(requestedColumn, columnMapping, "sıralanabilir", allowDirectTopLevel: true);
        var parameter = Expression.Parameter(typeof(T), "x");
        var resolved = ResolvePath(parameter, typeof(T), path)
            ?? throw Invalid($"'{requestedColumn}' sıralanabilir bir kolon değildir.");

        var lambda = Expression.Lambda(resolved.member, parameter);
        var method = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var call = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(T), resolved.member.Type],
            query.Expression,
            Expression.Quote(lambda));
        var sorted = query.Provider.CreateQuery<T>(call);

        var id = ResolvePath(parameter, typeof(T), "Id");
        if (id is null || path.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            return sorted;
        }

        var idLambda = Expression.Lambda(id.Value.member, parameter);
        var thenMethod = descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);
        var stableCall = Expression.Call(
            typeof(Queryable),
            thenMethod,
            [typeof(T), id.Value.member.Type],
            sorted.Expression,
            Expression.Quote(idLambda));
        return sorted.Provider.CreateQuery<T>(stableCall);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        PagedQueryExtensions.ValidatePagination(pageNumber, pageSize);
        var skipLong = (long)(pageNumber - 1) * pageSize;
        if (skipLong > int.MaxValue)
        {
            throw Invalid("İstenen sayfa numarası desteklenen sınırı aşıyor.");
        }

        return query.Skip((int)skipLong).Take(pageSize);
    }

    public static IQueryable<T> ApplyPagedRequest<T>(
        this IQueryable<T> query,
        PagedRequest request,
        IReadOnlyDictionary<string, string>? columnMapping = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        var searchMapping = columnMapping ?? CreateColumnMapping<T>([]);
        query = query.ApplySearch(request, searchMapping);
        query = query.ApplyFilters(request.Filters, request.FilterLogic, columnMapping);
        query = query.ApplySorting(request.SortBy, request.SortDirection, columnMapping);
        return query.ApplyPagination(request.PageNumber, request.PageSize);
    }

    private static IReadOnlyDictionary<string, string> CreateColumnMapping<T>(IReadOnlyCollection<string> paths)
    {
        if (paths.Count == 0)
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property =>
                    property.CanRead
                    && property.GetIndexParameters().Length == 0
                    && property.GetCustomAttribute<JsonIgnoreAttribute>() is null
                    && SupportsGeneralSearch(property.PropertyType))
                .ToDictionary(
                    property => property.Name,
                    property => property.Name,
                    StringComparer.OrdinalIgnoreCase);
        }

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            var trimmed = path.Trim();
            var alias = trimmed.Contains('.', StringComparison.Ordinal)
                ? trimmed[(trimmed.LastIndexOf('.') + 1)..]
                : trimmed;
            if (!mapping.TryAdd(alias, trimmed))
            {
                throw Invalid($"'{alias}' arama alanı birden fazla iç kolona eşleniyor; açık bir alias eşlemesi gereklidir.");
            }
        }

        return mapping;
    }

    private static bool SupportsGeneralSearch(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return type == typeof(string)
            || type == typeof(Guid)
            || type == typeof(bool)
            || type.IsEnum
            || IsNumericType(type);
    }

    private static Expression? BuildGeneralSearchMatch(Expression member, string term, bool useSqlPattern)
    {
        var propertyType = member.Type;
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (underlying == typeof(string))
        {
            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            var contains = useSqlPattern
                ? Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Like),
                    Type.EmptyTypes,
                    Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                    member,
                    Expression.Constant(AsciiTurkishSearch.BuildContainsPattern(term)),
                    Expression.Constant(AsciiTurkishSearch.LikeEscapeCharacter))
                : Expression.Call(
                    typeof(AsciiTurkishSearch),
                    nameof(AsciiTurkishSearch.Contains),
                    Type.EmptyTypes,
                    member,
                    Expression.Constant(term));
            return Expression.AndAlso(notNull, contains);
        }

        if (!TryParseGeneralSearchValue(term, underlying, out var parsed))
        {
            return null;
        }

        Expression valueMember = member;
        Expression? hasValue = null;
        if (Nullable.GetUnderlyingType(propertyType) is not null)
        {
            hasValue = Expression.Property(member, "HasValue");
            valueMember = Expression.Property(member, "Value");
        }

        var equals = Expression.Equal(valueMember, Expression.Constant(parsed, underlying));
        return hasValue is null ? equals : Expression.AndAlso(hasValue, equals);
    }

    private static bool TryParseGeneralSearchValue(string term, Type targetType, out object? parsed)
    {
        parsed = null;
        var value = term.Trim();
        if (targetType.IsEnum)
        {
            if (!Enum.TryParse(targetType, value, true, out var enumValue)
                || enumValue is null
                || !Enum.IsDefined(targetType, enumValue))
            {
                return false;
            }

            parsed = enumValue;
            return true;
        }

        if (targetType == typeof(Guid))
        {
            if (!Guid.TryParse(value, out var guid))
            {
                return false;
            }

            parsed = guid;
            return true;
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out var boolean))
            {
                parsed = boolean;
                return true;
            }

            if (value == "1" || value == "0")
            {
                parsed = value == "1";
                return true;
            }

            return false;
        }

        if (!IsNumericType(targetType))
        {
            return false;
        }

        try
        {
            parsed = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return parsed is not null;
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return false;
        }
    }

    private static bool IsNumericType(Type type) =>
        type == typeof(byte)
        || type == typeof(sbyte)
        || type == typeof(short)
        || type == typeof(ushort)
        || type == typeof(int)
        || type == typeof(uint)
        || type == typeof(long)
        || type == typeof(ulong)
        || type == typeof(float)
        || type == typeof(double)
        || type == typeof(decimal);

    private static Expression BuildFilter(Expression member, Type propertyType, Filter filter, int index)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var operation = ParseOperator(filter.Operator, index);
        if (operation is FilterOperation.IsNull or FilterOperation.IsNotNull)
        {
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null)
            {
                throw InvalidFilter(index, $"'{filter.Column}' kolonu null karşılaştırmasını desteklemiyor");
            }

            var nullValue = Expression.Constant(null, propertyType);
            return operation == FilterOperation.IsNull
                ? Expression.Equal(member, nullValue)
                : Expression.NotEqual(member, nullValue);
        }

        if (filter.Value is null)
        {
            throw InvalidFilter(index, $"'{filter.Column}' filtresi için değer zorunludur");
        }

        try
        {
            if (underlying == typeof(string))
            {
                if (operation is not (FilterOperation.Contains or FilterOperation.NotContains
                    or FilterOperation.Equals or FilterOperation.NotEquals
                    or FilterOperation.StartsWith or FilterOperation.EndsWith))
                {
                    throw InvalidFilter(index, $"'{filter.Operator}' metin kolonunda kullanılamaz");
                }

                var value = Expression.Constant(filter.Value);
                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                var isNull = Expression.Equal(member, Expression.Constant(null, typeof(string)));
                var methodName = operation switch
                {
                    FilterOperation.Equals or FilterOperation.NotEquals => nameof(string.Equals),
                    FilterOperation.StartsWith => nameof(string.StartsWith),
                    FilterOperation.EndsWith => nameof(string.EndsWith),
                    _ => nameof(string.Contains)
                };
                var stringComparison = Expression.Call(
                    member,
                    typeof(string).GetMethod(methodName, [typeof(string)])!,
                    value);
                return operation switch
                {
                    FilterOperation.NotEquals or FilterOperation.NotContains =>
                        Expression.OrElse(isNull, Expression.Not(stringComparison)),
                    _ => Expression.AndAlso(notNull, stringComparison)
                };
            }

            if (operation is FilterOperation.Contains or FilterOperation.NotContains
                or FilterOperation.StartsWith or FilterOperation.EndsWith)
            {
                throw InvalidFilter(index, $"'{filter.Operator}' operatörü {underlying.Name} kolonunda kullanılamaz");
            }

            var converted = ParseValue(filter.Value, underlying, index, filter.Column);
            var constant = Expression.Constant(converted, underlying);
            Expression valueMember = member;
            Expression? hasValue = null;
            if (Nullable.GetUnderlyingType(propertyType) is not null)
            {
                hasValue = Expression.Property(member, "HasValue");
                valueMember = Expression.Property(member, "Value");
            }

            var comparison = operation switch
            {
                FilterOperation.NotEquals => Expression.NotEqual(valueMember, constant),
                FilterOperation.GreaterThan => Expression.GreaterThan(valueMember, constant),
                FilterOperation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(valueMember, constant),
                FilterOperation.LessThan => Expression.LessThan(valueMember, constant),
                FilterOperation.LessThanOrEqual => Expression.LessThanOrEqual(valueMember, constant),
                FilterOperation.Equals => Expression.Equal(valueMember, constant),
                _ => throw InvalidFilter(index, $"'{filter.Operator}' operatörü desteklenmiyor")
            };
            if (hasValue is null)
            {
                return comparison;
            }

            return operation == FilterOperation.NotEquals
                ? Expression.OrElse(Expression.Not(hasValue), comparison)
                : Expression.AndAlso(hasValue, comparison);
        }
        catch (PagedQueryValidationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw InvalidFilter(index, $"'{filter.Value}' değeri '{filter.Column}' kolonu için geçersiz");
        }
    }

    private static object ParseValue(string rawValue, Type targetType, int index, string column)
    {
        var value = rawValue.Trim();
        if (targetType.IsEnum)
        {
            if (!Enum.TryParse(targetType, value, true, out var enumValue)
                || enumValue is null
                || !Enum.IsDefined(targetType, enumValue))
            {
                throw InvalidFilter(index, $"'{rawValue}' değeri '{column}' enum kolonu için geçersiz");
            }

            return enumValue;
        }

        if (targetType == typeof(Guid))
        {
            return Guid.TryParse(value, out var guid)
                ? guid
                : throw InvalidFilter(index, $"'{rawValue}' geçerli bir Guid değildir");
        }

        if (targetType == typeof(DateOnly))
        {
            return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed)
                ? parsed
                : throw InvalidFilter(index, $"'{rawValue}' geçerli bir ISO tarih değildir");
        }

        if (targetType == typeof(TimeOnly))
        {
            return TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed)
                ? parsed
                : throw InvalidFilter(index, $"'{rawValue}' geçerli bir saat değildir");
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : throw InvalidFilter(index, $"'{rawValue}' geçerli bir ISO tarih/saat değildir");
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : throw InvalidFilter(index, $"'{rawValue}' geçerli bir ISO tarih/saat değildir");
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out var boolean))
            {
                return boolean;
            }

            if (value == "1" || value == "0")
            {
                return value == "1";
            }

            throw InvalidFilter(index, $"'{rawValue}' geçerli bir boolean değildir");
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture)
            ?? throw InvalidFilter(index, $"'{rawValue}' değeri '{column}' kolonu için geçersiz");
    }

    private static string ResolveColumn(
        string column,
        IReadOnlyDictionary<string, string>? mapping,
        string capability,
        int? filterIndex = null,
        bool allowDirectTopLevel = false)
    {
        if (mapping is null)
        {
            if (column.Contains('.', StringComparison.Ordinal))
            {
                throw filterIndex.HasValue
                    ? InvalidFilter(filterIndex.Value, $"'{column}' iç içe alan yolu doğrudan kullanılamaz")
                    : Invalid($"'{column}' iç içe alan yolu doğrudan kullanılamaz.");
            }

            return column;
        }

        var key = mapping.Keys.FirstOrDefault(item => item.Equals(column, StringComparison.OrdinalIgnoreCase));
        if (key is not null)
        {
            return mapping[key];
        }

        if (column.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            return "Id";
        }

        if (allowDirectTopLevel && !column.Contains('.', StringComparison.Ordinal))
        {
            return column;
        }

        throw filterIndex.HasValue
            ? InvalidFilter(filterIndex.Value, $"'{column}' izin verilen kolonlar arasında değildir")
            : Invalid($"'{column}' {capability} kolonlar arasında değildir.");
    }

    private static (Expression member, PropertyInfo property)? ResolvePath(Expression root, Type rootType, string path)
    {
        Expression member = root;
        PropertyInfo? property = null;
        var type = rootType;
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length is < 1 or > 4)
        {
            return null;
        }

        foreach (var segment in segments)
        {
            property = type.GetProperty(segment, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property is null)
            {
                return null;
            }

            member = Expression.Property(member, property);
            type = property.PropertyType;
        }

        return property is null ? null : (member, property);
    }

    private static FilterOperation ParseOperator(string? value, int index) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "eq" or "equal" or "equals" or "=" => FilterOperation.Equals,
            "ne" or "neq" or "notequal" or "notequals" or "!=" or "<>" => FilterOperation.NotEquals,
            "contains" => FilterOperation.Contains,
            "notcontains" => FilterOperation.NotContains,
            "startswith" => FilterOperation.StartsWith,
            "endswith" => FilterOperation.EndsWith,
            "gt" or ">" => FilterOperation.GreaterThan,
            "gte" or ">=" => FilterOperation.GreaterThanOrEqual,
            "lt" or "<" => FilterOperation.LessThan,
            "lte" or "<=" => FilterOperation.LessThanOrEqual,
            "isnull" => FilterOperation.IsNull,
            "isnotnull" => FilterOperation.IsNotNull,
            _ => throw InvalidFilter(index, $"'{value}' filtre operatörü desteklenmiyor")
        };

    private static FilterLogic ParseFilterLogic(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "and" => FilterLogic.And,
            "or" => FilterLogic.Or,
            _ => throw Invalid("Filtre mantığı yalnızca 'and' veya 'or' olabilir.")
        };

    private static bool ParseSortDirection(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "asc" => false,
            "desc" => true,
            _ => throw Invalid("Sıralama yönü yalnızca 'asc' veya 'desc' olabilir.")
        };

    private static PagedQueryValidationException Invalid(string message) => new(message);

    private static PagedQueryValidationException InvalidFilter(int index, string message) =>
        Invalid($"{index + 1}. gelişmiş filtre geçersiz: {message}.");

    private enum FilterLogic
    {
        And,
        Or
    }

    private enum FilterOperation
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        IsNull,
        IsNotNull
    }
}
