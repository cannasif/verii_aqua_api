using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class BatchMovement : BaseEntity
    {
        public long FishBatchId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FromProjectCageId { get; set; }
        public long? ToProjectCageId { get; set; }
        public long? WarehouseId { get; set; }
        public long? FromWarehouseId { get; set; }
        public long? ToWarehouseId { get; set; }
        public long? FromStockId { get; set; }
        public long? ToStockId { get; set; }
        public decimal? FromAverageGram { get; set; }
        public decimal? ToAverageGram { get; set; }
        public DateTime MovementDate { get; set; }
        public BatchMovementType MovementType { get; set; }
        public int SignedCount { get; set; }
        public decimal SignedBiomassGram { get; set; }
        public decimal? FeedGram { get; set; }
        public long? ActorUserId { get; set; }
        public string ReferenceTable { get; set; } = string.Empty;
        public long ReferenceId { get; set; }
        public string? Note { get; set; }

        public FishBatch? FishBatch { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
