using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishLabSample : BaseEntity
    {
        public long ProjectId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public long? FishHealthEventId { get; set; }
        public DateTime SampleDate { get; set; }
        public string SampleCode { get; set; } = string.Empty;
        public string SampleType { get; set; } = string.Empty;
        public string? LaboratoryName { get; set; }
        public string? RequestedBy { get; set; }
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
        public FishHealthEvent? FishHealthEvent { get; set; }
        public ICollection<FishLabResult> Results { get; set; } = new List<FishLabResult>();
    }
}
