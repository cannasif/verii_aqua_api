using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class GoodsReceipt : BaseEntity
    {
        public long? ProjectId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public long? SupplierId { get; set; }
        public long? WarehouseId { get; set; }
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
    }
}
