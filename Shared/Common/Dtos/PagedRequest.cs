using System.Collections.Generic;
using aqua_api.Shared.Host.WebApi.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Shared.Common.Dtos
{
    public class Filter
    {
        public string Column { get; set; } = string.Empty;
        public string Operator { get; set; } = "Equals";
        public string Value { get; set; } = string.Empty;
    }

    [ModelBinder(BinderType = typeof(PagedRequestModelBinder))]
    public class PagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "Id";
        public string? SortDirection { get; set; } = "desc";
        public List<Filter>? Filters { get; set; } = new();
        /// <summary>
        /// "and" veya "or" — filtrelerin nasıl birleştirileceğini belirler. Varsayılan: "and"
        /// </summary>
        public string FilterLogic { get; set; } = "and";
    }
}
