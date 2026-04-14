using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class StockConvertLine : BaseEntity
    {
        public long StockConvertId { get; set; }
        public long FromFishBatchId { get; set; }
        public long ToFishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal NewAverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public StockConvert? StockConvert { get; set; }
        public FishBatch? FromFishBatch { get; set; }
        public FishBatch? ToFishBatch { get; set; }
        public ProjectCage? FromProjectCage { get; set; }
        public ProjectCage? ToProjectCage { get; set; }
    }
}
