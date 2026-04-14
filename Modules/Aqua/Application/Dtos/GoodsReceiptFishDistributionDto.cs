using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class GoodsReceiptFishDistributionDto
    {
        public long Id { get; set; }
        public long GoodsReceiptLineId { get; set; }
        public long ProjectCageId { get; set; }
        public long FishBatchId { get; set; }
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
