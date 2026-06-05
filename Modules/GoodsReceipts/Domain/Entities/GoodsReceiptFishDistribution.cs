using System;
using System.Collections.Generic;

namespace aqua_api.Modules.GoodsReceipts.Domain.Entities
{
    public class GoodsReceiptFishDistribution : BaseEntity
    {
        public long GoodsReceiptLineId { get; set; }
        public long ProjectCageId { get; set; }
        public long FishBatchId { get; set; }
        public int FishCount { get; set; }

        public GoodsReceiptLine? GoodsReceiptLine { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
