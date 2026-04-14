using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class BatchMovementDto
    {
        public long Id { get; set; }
        public long FishBatchId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FromProjectCageId { get; set; }
        public long? ToProjectCageId { get; set; }
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
    }

    public class CreateBatchMovementDto
    {
        public long FishBatchId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FromProjectCageId { get; set; }
        public long? ToProjectCageId { get; set; }
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
    }

    public class UpdateBatchMovementDto : CreateBatchMovementDto
    {
    }
}
