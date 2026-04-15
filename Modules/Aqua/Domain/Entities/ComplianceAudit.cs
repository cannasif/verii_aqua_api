using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ComplianceAudit : BaseEntity
    {
        public long ProjectId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public DateTime AuditDate { get; set; }
        public string StandardCode { get; set; } = string.Empty;
        public string? ChecklistCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public int FindingCount { get; set; }
        public string? AuditorName { get; set; }
        public string? Summary { get; set; }
        public DateTime? NextAuditDate { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
        public ICollection<ComplianceCorrectiveAction> CorrectiveActions { get; set; } = new List<ComplianceCorrectiveAction>();
    }
}
