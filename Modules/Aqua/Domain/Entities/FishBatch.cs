using System;
using System.Collections.Generic;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishBatch : BaseEntity
    {
        public long ProjectId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public long FishStockId { get; set; }
        public decimal CurrentAverageGram { get; set; }
        public DateTime StartDate { get; set; }
        public long? SourceGoodsReceiptLineId { get; set; }

        public Project? Project { get; set; }
        public StockEntity? FishStock { get; set; }
        public GoodsReceiptLine? SourceGoodsReceiptLine { get; set; }

        public ICollection<BatchCageBalance> BatchCageBalances { get; set; } = new List<BatchCageBalance>();
        public ICollection<GoodsReceiptLine> GoodsReceiptLines { get; set; } = new List<GoodsReceiptLine>();
        public ICollection<GoodsReceiptFishDistribution> GoodsReceiptFishDistributions { get; set; } = new List<GoodsReceiptFishDistribution>();
        public ICollection<FeedingDistribution> FeedingDistributions { get; set; } = new List<FeedingDistribution>();
        public ICollection<MortalityLine> MortalityLines { get; set; } = new List<MortalityLine>();
        public ICollection<TransferLine> TransferLines { get; set; } = new List<TransferLine>();
        public ICollection<ShipmentLine> ShipmentLines { get; set; } = new List<ShipmentLine>();
        public ICollection<WeighingLine> WeighingLines { get; set; } = new List<WeighingLine>();
        public ICollection<StockConvertLine> StockConvertFromLines { get; set; } = new List<StockConvertLine>();
        public ICollection<StockConvertLine> StockConvertToLines { get; set; } = new List<StockConvertLine>();
        public ICollection<BatchMovement> BatchMovements { get; set; } = new List<BatchMovement>();
    }
}
