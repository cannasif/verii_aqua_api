using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Exceptions;

namespace aqua_api.Shared.Common.Helpers
{
    public sealed class PagedQueryResult<T>
    {
        public List<T> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }

        public PagedResponse<TDto> ToResponse<TDto>(IEnumerable<TDto> items)
        {
            return new PagedResponse<TDto>
            {
                Items = items.ToList(),
                TotalCount = TotalCount,
                PageNumber = PageNumber,
                PageSize = PageSize
            };
        }
    }

    public static class PagedQueryExtensions
    {
        public const int DefaultMaxPageSize = 500;
        public const int DefaultMaxSearchLength = 200;
        public const int MaximumSearchFieldCount = 12;

        public static async Task<PagedQueryResult<T>> ToPagedItemsAsync<T>(
            this IQueryable<T> query,
            PagedRequest request,
            bool useSeekCountForSearch = false,
            int maxPageSize = DefaultMaxPageSize,
            CancellationToken cancellationToken = default)
        {
            request ??= new PagedRequest();
            ValidateRequest(request, maxPageSize);

            return await query
                .ToPagedItemsAsync(
                    request.PageNumber,
                    request.PageSize,
                    HasSearch(request),
                    useSeekCountForSearch,
                    maxPageSize,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<PagedQueryResult<T>> ToPagedItemsAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            bool hasSearch,
            bool useSeekCountForSearch = false,
            int maxPageSize = DefaultMaxPageSize,
            CancellationToken cancellationToken = default)
        {
            ValidatePagination(pageNumber, pageSize, maxPageSize);
            var skipLong = (long)(pageNumber - 1) * pageSize;
            if (skipLong > int.MaxValue)
            {
                throw new PagedQueryValidationException("İstenen sayfa numarası desteklenen sınırı aşıyor.");
            }

            var skip = (int)skipLong;

            _ = hasSearch;
            _ = useSeekCountForSearch;

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var pageItems = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedQueryResult<T>
            {
                Items = pageItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static PagedResponse<TDto> ToPagedResponse<TSource, TDto>(
            this PagedQueryResult<TSource> page,
            Func<TSource, TDto> map)
        {
            return page.ToResponse(page.Items.Select(map));
        }

        private static bool HasSearch(PagedRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.Search);
        }

        internal static void ValidateRequest(PagedRequest request, int maxPageSize = DefaultMaxPageSize)
        {
            ValidatePagination(request.PageNumber, request.PageSize, maxPageSize);
            if (request.Search?.Length > DefaultMaxSearchLength)
            {
                throw new PagedQueryValidationException($"Arama metni en fazla {DefaultMaxSearchLength} karakter olabilir.");
            }

            if ((request.SearchFields?.Count ?? 0) > MaximumSearchFieldCount)
            {
                throw new PagedQueryValidationException($"En fazla {MaximumSearchFieldCount} arama alanı seçilebilir.");
            }
        }

        internal static void ValidatePagination(
            int pageNumber,
            int pageSize,
            int maxPageSize = DefaultMaxPageSize)
        {
            if (pageNumber < 1)
            {
                throw new PagedQueryValidationException("Sayfa numarası en az 1 olmalıdır.");
            }

            if (pageSize < 1)
            {
                throw new PagedQueryValidationException("Sayfa boyutu en az 1 olmalıdır.");
            }

            if (maxPageSize > 0 && pageSize > maxPageSize)
            {
                throw new PagedQueryValidationException($"Sayfa boyutu en fazla {maxPageSize} olabilir.");
            }

            if ((long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new PagedQueryValidationException("İstenen sayfa numarası desteklenen sınırı aşıyor.");
            }
        }
    }
}
