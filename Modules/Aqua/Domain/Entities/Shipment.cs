using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class Shipment : BaseEntity
    {
        public long ProjectId { get; set; }
        public string ShipmentNo { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public long? TargetWarehouseId { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
    }
}
