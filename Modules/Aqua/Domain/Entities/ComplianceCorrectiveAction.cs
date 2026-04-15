using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ComplianceCorrectiveAction : BaseEntity
    {
        public long ComplianceAuditId { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? OwnerName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string? ClosureNote { get; set; }

        public ComplianceAudit? ComplianceAudit { get; set; }
    }
}
