using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class StockConvert : BaseEntity
    {
        public long ProjectId { get; set; }
        public string ConvertNo { get; set; } = string.Empty;
        public DateTime ConvertDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ICollection<StockConvertLine> Lines { get; set; } = new List<StockConvertLine>();
    }
}
