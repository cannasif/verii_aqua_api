using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class BatchWarehouseBalance : BaseEntity
    {
        public long ProjectId { get; set; }
        public long FishBatchId { get; set; }
        public long WarehouseId { get; set; }
        public int LiveCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public DateTime AsOfDate { get; set; }

        public Project? Project { get; set; }
        public FishBatch? FishBatch { get; set; }
        public aqua_api.Modules.Warehouse.Domain.Entities.Warehouse? Warehouse { get; set; }
    }
}
