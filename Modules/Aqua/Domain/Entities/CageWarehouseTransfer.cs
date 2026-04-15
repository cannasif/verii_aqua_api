using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class CageWarehouseTransfer : BaseEntity
    {
        public long ProjectId { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<CageWarehouseTransferLine> Lines { get; set; } = new List<CageWarehouseTransferLine>();
    }
}
