using System;

namespace aqua_api.Modules.GoodsReceipts.Application.Dtos
{
    public class GoodsReceiptFishDistributionDto
    {
        public long Id { get; set; }
        public long GoodsReceiptLineId { get; set; }
        public string? StockCode { get; set; }
        public string? StockName { get; set; }
        public long ProjectCageId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public int FishCount { get; set; }
    }

    public class CreateGoodsReceiptFishDistributionDto
    {
        public long GoodsReceiptLineId { get; set; }
        public long ProjectCageId { get; set; }
        public long FishBatchId { get; set; }
        public int FishCount { get; set; }
    }

    public class UpdateGoodsReceiptFishDistributionDto : CreateGoodsReceiptFishDistributionDto
    {
    }
}
