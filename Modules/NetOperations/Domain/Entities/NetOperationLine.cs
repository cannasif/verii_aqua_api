using System;
using System.Collections.Generic;

namespace aqua_api.Modules.NetOperations.Domain.Entities
{
    public class NetOperationLine : BaseEntity
    {
        public long NetOperationId { get; set; }
        public long ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public string? Note { get; set; }

        public NetOperation? NetOperation { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
