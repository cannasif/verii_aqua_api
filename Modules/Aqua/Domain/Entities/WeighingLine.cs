using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class WeighingLine : BaseEntity
    {
        public long WeighingId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int MeasuredCount { get; set; }
        public decimal MeasuredAverageGram { get; set; }
        public decimal MeasuredBiomassGram { get; set; }

        public Weighing? Weighing { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
