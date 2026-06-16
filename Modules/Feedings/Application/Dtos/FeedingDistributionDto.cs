using System;

namespace aqua_api.Modules.Feedings.Application.Dtos
{
    public class FeedingDistributionDto
    {
        public long Id { get; set; }
        public long FeedingLineId { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public long ProjectCageId { get; set; }
        public long? ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public decimal FeedGram { get; set; }
        public bool IsERPIntegrated { get; set; }
        public string? ERPReferenceNumber { get; set; }
        public DateTime? ERPIntegrationDate { get; set; }
        public string? ERPIntegrationStatus { get; set; }
        public string? ERPErrorMessage { get; set; }
    }

    public class CreateFeedingDistributionDto
    {
        public long FeedingLineId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public decimal FeedGram { get; set; }
    }

    public class UpdateFeedingDistributionDto : CreateFeedingDistributionDto
    {
    }
}
