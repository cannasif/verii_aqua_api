using Microsoft.EntityFrameworkCore;

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
        private const int DefaultPageSize = 20;
        private const int DefaultMaxPageSize = 500;

        public static async Task<PagedQueryResult<T>> ToPagedItemsAsync<T>(
            this IQueryable<T> query,
            PagedRequest request,
            bool useSeekCountForSearch = true,
            int maxPageSize = DefaultMaxPageSize,
            CancellationToken cancellationToken = default)
        {
            request ??= new PagedRequest();

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
            bool useSeekCountForSearch = true,
            int maxPageSize = DefaultMaxPageSize,
            CancellationToken cancellationToken = default)
        {
            var normalizedPageNumber = NormalizePageNumber(pageNumber);
            var normalizedPageSize = NormalizePageSize(pageSize, maxPageSize);
            var skip = (normalizedPageNumber - 1) * normalizedPageSize;

            if (useSeekCountForSearch && hasSearch)
            {
                var items = await query
                    .Skip(skip)
                    .Take(normalizedPageSize + 1)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var hasNextPage = items.Count > normalizedPageSize;
                if (hasNextPage)
                {
                    items.RemoveAt(items.Count - 1);
                }

                return new PagedQueryResult<T>
                {
                    Items = items,
                    TotalCount = skip + items.Count + (hasNextPage ? 1 : 0),
                    PageNumber = normalizedPageNumber,
                    PageSize = normalizedPageSize
                };
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var pageItems = await query
                .Skip(skip)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedQueryResult<T>
            {
                Items = pageItems,
                TotalCount = totalCount,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
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

        private static int NormalizePageNumber(int pageNumber)
        {
            return pageNumber < 1 ? 1 : pageNumber;
        }

        private static int NormalizePageSize(int pageSize, int maxPageSize)
        {
            if (pageSize < 1)
            {
                return DefaultPageSize;
            }

            if (maxPageSize < 1)
            {
                return pageSize;
            }

            return Math.Min(pageSize, maxPageSize);
        }
    }
}
