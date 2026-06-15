using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Feedings.Domain.Entities
{
    public class Feeding : BaseEntity, IErpPostableHeader
    {
        public long ProjectId { get; set; }
        public string FeedingNo { get; set; } = string.Empty;
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }
        public bool IsERPIntegrated { get; set; }
        public string? ERPReferenceNumber { get; set; }
        public DateTime? ERPIntegrationDate { get; set; }
        public string? ERPIntegrationStatus { get; set; }
        public string? ERPErrorMessage { get; set; }
        public int? CountTriedBy { get; set; } = 0;

        public Project? Project { get; set; }
        public ICollection<FeedingLine> Lines { get; set; } = new List<FeedingLine>();
    }
}
