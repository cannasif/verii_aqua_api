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
    }
}
