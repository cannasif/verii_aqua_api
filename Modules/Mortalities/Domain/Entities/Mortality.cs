using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Mortalities.Domain.Entities
{
    public class Mortality : BaseEntity, IErpPostableHeader
    {
        public long ProjectId { get; set; }
        public string? MortalityNo { get; set; }
        public DateTime MortalityDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }
        public bool IsERPIntegrated { get; set; }
        public string? ERPReferenceNumber { get; set; }
        public DateTime? ERPIntegrationDate { get; set; }
        public string? ERPIntegrationStatus { get; set; }
        public string? ERPErrorMessage { get; set; }
        public int? CountTriedBy { get; set; } = 0;

        public Project? Project { get; set; }
        public ICollection<MortalityLine> Lines { get; set; } = new List<MortalityLine>();
    }
}
