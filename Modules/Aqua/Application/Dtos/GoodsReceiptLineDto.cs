using System;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class GoodsReceiptLineDto
    {
        public long Id { get; set; }
        public long GoodsReceiptId { get; set; }
        public GoodsReceiptItemType ItemType { get; set; }
        public long StockId { get; set; }
        public decimal? QtyUnit { get; set; }
        public decimal? GramPerUnit { get; set; }
        public decimal? TotalGram { get; set; }
        public int? FishCount { get; set; }
        public decimal? FishAverageGram { get; set; }
        public decimal? FishTotalGram { get; set; }
        public long? FishBatchId { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? LocalUnitPrice { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? LocalLineAmount { get; set; }
        public StockEntity? Stock { get; set; }
    }

    public class CreateGoodsReceiptLineDto
    {
        public long GoodsReceiptId { get; set; }
        public GoodsReceiptItemType ItemType { get; set; }
        public long StockId { get; set; }
        public decimal? QtyUnit { get; set; }
        public decimal? GramPerUnit { get; set; }
        public decimal? TotalGram { get; set; }
        public int? FishCount { get; set; }
        public decimal? FishAverageGram { get; set; }
        public decimal? FishTotalGram { get; set; }
        public long? FishBatchId { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? LocalUnitPrice { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? LocalLineAmount { get; set; }
        public StockEntity? Stock { get; set; }
    }

    public class UpdateGoodsReceiptLineDto : CreateGoodsReceiptLineDto
    {
    }
}
