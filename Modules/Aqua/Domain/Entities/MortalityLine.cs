using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class MortalityLine : BaseEntity
    {
        public long MortalityId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int DeadCount { get; set; }

        public Mortality? Mortality { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
