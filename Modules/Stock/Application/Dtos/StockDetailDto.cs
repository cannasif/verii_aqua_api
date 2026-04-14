using System;
using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Stock.Application.Dtos
{
    public class StockDetailGetDto : BaseEntityDto
    {
        public long StockId { get; set; }
        public string? StockName { get; set; }
        public string HtmlDescription { get; set; } = string.Empty;
        public string? TechnicalSpecsJson { get; set; }
    }

    public class StockDetailCreateDto
    {
        [Required]
        public long StockId { get; set; }

        [Required]
        public string HtmlDescription { get; set; } = string.Empty;

        public string? TechnicalSpecsJson { get; set; }
    }

    public class StockDetailUpdateDto
    {
        [Required]
        public long StockId { get; set; }

        [Required]
        public string HtmlDescription { get; set; } = string.Empty;

        public string? TechnicalSpecsJson { get; set; }
    }
}
