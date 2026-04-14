using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class Feeding : BaseEntity
    {
        public long ProjectId { get; set; }
        public string FeedingNo { get; set; } = string.Empty;
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<FeedingLine> Lines { get; set; } = new List<FeedingLine>();
    }
}
