using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class BatchMovementDto
    {
        public long Id { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public long? ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long? FishStockId { get; set; }
        public string? FishStockCode { get; set; }
        public string? FishStockName { get; set; }
        public long? ProjectCageId { get; set; }
        public string? ProjectCageCode { get; set; }
        public string? ProjectCageName { get; set; }
        public long? FromProjectCageId { get; set; }
        public string? FromProjectCageCode { get; set; }
        public string? FromProjectCageName { get; set; }
        public long? ToProjectCageId { get; set; }
        public string? ToProjectCageCode { get; set; }
        public string? ToProjectCageName { get; set; }
        public long? FromStockId { get; set; }
        public string? FromStockCode { get; set; }
        public string? FromStockName { get; set; }
        public long? ToStockId { get; set; }
        public string? ToStockCode { get; set; }
        public string? ToStockName { get; set; }
        public decimal? FromAverageGram { get; set; }
        public decimal? ToAverageGram { get; set; }
        public DateTime MovementDate { get; set; }
        public BatchMovementType MovementType { get; set; }
        public string MovementTypeName { get; set; } = string.Empty;
        public int SignedCount { get; set; }
        public decimal SignedBiomassGram { get; set; }
        public decimal? FeedGram { get; set; }
        public long? ActorUserId { get; set; }
        public string ReferenceTable { get; set; } = string.Empty;
        public long ReferenceId { get; set; }
        public string? ReferenceDocumentNo { get; set; }
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
