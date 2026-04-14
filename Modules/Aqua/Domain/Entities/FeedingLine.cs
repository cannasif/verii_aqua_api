using System;
using System.Collections.Generic;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FeedingLine : BaseEntity
    {
        public long FeedingId { get; set; }
        public long StockId { get; set; }
        public decimal QtyUnit { get; set; }
        public decimal GramPerUnit { get; set; }
        public decimal TotalGram { get; set; }

        public Feeding? Feeding { get; set; }
        public StockEntity? Stock { get; set; }
        public ICollection<FeedingDistribution> Distributions { get; set; } = new List<FeedingDistribution>();
    }
}
