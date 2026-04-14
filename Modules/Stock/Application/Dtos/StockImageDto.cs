using System;

namespace aqua_api.Modules.Stock.Application.Dtos
{
    public class StockImageDto : BaseEntityDto
    {
        public long StockId { get; set; }
        public string? StockName { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class StockImageCreateDto
    {
        public long StockId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
    }

    public class StockImageUpdateDto
    {
        public long StockId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
