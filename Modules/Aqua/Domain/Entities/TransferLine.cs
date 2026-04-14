using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class TransferLine : BaseEntity
    {
        public long TransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }

        public Transfer? Transfer { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ProjectCage? FromProjectCage { get; set; }
        public ProjectCage? ToProjectCage { get; set; }
    }
}
