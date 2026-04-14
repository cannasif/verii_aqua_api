using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class FeedingDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string FeedingNo { get; set; } = string.Empty;
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
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
