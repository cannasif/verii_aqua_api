using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Shipments.Domain.Entities
{
    public class Shipment : BaseEntity, IErpPostableHeader
    {
        public long ProjectId { get; set; }
        public string ShipmentNo { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public long? TargetWarehouseId { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }
        public bool IsERPIntegrated { get; set; }
        public string? ERPReferenceNumber { get; set; }
        public DateTime? ERPIntegrationDate { get; set; }
        public string? ERPIntegrationStatus { get; set; }
        public string? ERPErrorMessage { get; set; }
        public int? CountTriedBy { get; set; } = 0;

        public Project? Project { get; set; }
        public ICollection<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
    }
}
