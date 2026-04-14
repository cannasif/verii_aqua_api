using System;
using System.Collections.Generic;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class GoodsReceiptLine : BaseEntity
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

        public GoodsReceipt? GoodsReceipt { get; set; }
        public StockEntity? Stock { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ICollection<GoodsReceiptFishDistribution> FishDistributions { get; set; } = new List<GoodsReceiptFishDistribution>();
    }
}
