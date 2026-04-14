using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class NetOperation : BaseEntity
    {
        public long ProjectId { get; set; }
        public long OperationTypeId { get; set; }
        public string OperationNo { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public NetOperationType? OperationType { get; set; }
        public ICollection<NetOperationLine> Lines { get; set; } = new List<NetOperationLine>();
    }
}
