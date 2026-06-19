using System;

namespace aqua_api.Modules.Feedings.Application.Dtos
{
    public class FeedingDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public string? BatchCode { get; set; }
        public string? StockCode { get; set; }
        public string? StockName { get; set; }
        public decimal TotalFeedGram { get; set; }
        public string FeedingNo { get; set; } = string.Empty;
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
        public bool IsERPIntegrated { get; set; }
        public string? ERPReferenceNumber { get; set; }
        public DateTime? ERPIntegrationDate { get; set; }
        public string? ERPIntegrationStatus { get; set; }
        public string? ERPErrorMessage { get; set; }
        public int? CountTriedBy { get; set; }
    }

    public class CreateFeedingDto
    {
        public long ProjectId { get; set; }
        public string FeedingNo { get; set; } = string.Empty;
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateFeedingDto : CreateFeedingDto
    {
    }
}
