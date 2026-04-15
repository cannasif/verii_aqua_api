using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ProjectCage : BaseEntity
    {
        public long ProjectId { get; set; }
        public long CageId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ReleasedDate { get; set; }

        public Project? Project { get; set; }
        public Cage? Cage { get; set; }
        public ICollection<BatchCageBalance> BatchCageBalances { get; set; } = new List<BatchCageBalance>();
        public ICollection<FishHealthEvent> FishHealthEvents { get; set; } = new List<FishHealthEvent>();
        public ICollection<FishTreatment> FishTreatments { get; set; } = new List<FishTreatment>();
        public ICollection<FishLabSample> FishLabSamples { get; set; } = new List<FishLabSample>();
        public ICollection<WelfareAssessment> WelfareAssessments { get; set; } = new List<WelfareAssessment>();
        public ICollection<ComplianceAudit> ComplianceAudits { get; set; } = new List<ComplianceAudit>();
    }
}
