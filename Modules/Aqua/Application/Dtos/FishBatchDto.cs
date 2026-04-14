using System;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class FishBatchDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public long FishStockId { get; set; }
        public decimal CurrentAverageGram { get; set; }
        public DateTime StartDate { get; set; }
        public long? SourceGoodsReceiptLineId { get; set; }
        public StockEntity? FishStock { get; set; }
    }

    public class CreateFishBatchDto
    {
        public long ProjectId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public long FishStockId { get; set; }
        public decimal CurrentAverageGram { get; set; }
        public DateTime StartDate { get; set; }
        public long? SourceGoodsReceiptLineId { get; set; }
        public StockEntity? FishStock { get; set; }
    }

    public class UpdateFishBatchDto : CreateFishBatchDto
    {
    }
}
