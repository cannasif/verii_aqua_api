using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class Mortality : BaseEntity
    {
        public long ProjectId { get; set; }
        public DateTime MortalityDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<MortalityLine> Lines { get; set; } = new List<MortalityLine>();
    }
}
