using System;

namespace aqua_api.Modules.Stock.Application.Dtos
{
    public class StockRelationDto : BaseEntityDto
    {
        public long StockId { get; set; }
        public string? StockName { get; set; }
        public long RelatedStockId { get; set; }
        public string? RelatedStockCode { get; set; }
        public string? RelatedStockName { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
        public bool IsMandatory { get; set; }
    }

    public class StockRelationCreateDto
    {
        public long StockId { get; set; }
        public long RelatedStockId { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
        public bool IsMandatory { get; set; } = true;
    }

    public class StockRelationUpdateDto
    {
        public long StockId { get; set; }
        public long RelatedStockId { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
        public bool IsMandatory { get; set; }
    }
}
