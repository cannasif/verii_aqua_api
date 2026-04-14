using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FeedingDistribution : BaseEntity
    {
        public long FeedingLineId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public decimal FeedGram { get; set; }

        public FeedingLine? FeedingLine { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
