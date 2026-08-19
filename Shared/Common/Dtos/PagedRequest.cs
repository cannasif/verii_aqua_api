using System.Collections.Generic;
using System.Text.Json.Serialization;
using aqua_api.Shared.Host.WebApi.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Shared.Common.Dtos
{
    public class Filter
    {
        public string Column { get; set; } = string.Empty;
        public string Operator { get; set; } = "equals";
        public string? Value { get; set; }
    }

    [ModelBinder(BinderType = typeof(PagedRequestModelBinder))]
    public class PagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public List<string> SearchFields { get; set; } = new();
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public List<Filter>? Filters { get; set; } = new();
        /// <summary>
        /// "and" veya "or" — filtrelerin nasıl birleştirileceğini belirler. Varsayılan: "and"
        /// </summary>
        public string FilterLogic { get; set; } = "and";

        [JsonIgnore]
        public bool SearchFieldsSpecified { get; private set; }

        internal bool SearchApplied { get; private set; }

        internal void MarkSearchFieldsSpecified() => SearchFieldsSpecified = true;

        internal void MarkSearchApplied() => SearchApplied = true;
    }
}
