using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class Weighing : BaseEntity
    {
        public long ProjectId { get; set; }
        public string WeighingNo { get; set; } = string.Empty;
        public DateTime WeighingDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<WeighingLine> Lines { get; set; } = new List<WeighingLine>();
    }
}
