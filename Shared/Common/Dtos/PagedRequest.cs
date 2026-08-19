using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace aqua_api.Shared.Common.Dtos
{
    public class Filter
    {
        public string Column { get; set; } = string.Empty;
        public string Operator { get; set; } = "equals";
        public string? Value { get; set; }
    }

    public class PagedRequest
    {
        private List<string> _searchFields = new();

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public List<string> SearchFields
        {
            get => _searchFields;
            set
            {
                _searchFields = value ?? new List<string>();
                SearchFieldsSpecified = true;
            }
        }
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

        internal void MarkSearchApplied() => SearchApplied = true;

        internal void Normalize()
        {
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
            _searchFields = _searchFields
                .Select(field => field?.Trim() ?? string.Empty)
                .ToList();
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();
            SortDirection = string.IsNullOrWhiteSpace(SortDirection) ? "desc" : SortDirection.Trim();
            FilterLogic = string.IsNullOrWhiteSpace(FilterLogic) ? "and" : FilterLogic.Trim();
            Filters = Filters?
                .Select(filter => new Filter
                {
                    Column = filter.Column?.Trim() ?? string.Empty,
                    Operator = filter.Operator?.Trim() ?? string.Empty,
                    Value = filter.Value?.Trim()
                })
                .ToList() ?? new List<Filter>();
        }
    }
}
