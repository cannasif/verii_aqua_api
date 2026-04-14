using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class BatchCageBalance : BaseEntity
    {
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int LiveCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public DateTime AsOfDate { get; set; }

        public FishBatch? FishBatch { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
