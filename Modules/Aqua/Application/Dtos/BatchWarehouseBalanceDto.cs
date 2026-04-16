using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class BatchWarehouseBalanceDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public long? FishStockId { get; set; }
        public string? FishStockCode { get; set; }
        public string? FishStockName { get; set; }
        public long WarehouseId { get; set; }
        public short? WarehouseCode { get; set; }
        public string? WarehouseName { get; set; }
        public int? WarehouseBranchCode { get; set; }
        public int LiveCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public DateTime AsOfDate { get; set; }
    }

    public class CreateBatchWarehouseBalanceDto
    {
        public long ProjectId { get; set; }
        public long FishBatchId { get; set; }
        public long WarehouseId { get; set; }
        public int LiveCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
        public DateTime AsOfDate { get; set; }
    }

    public class UpdateBatchWarehouseBalanceDto : CreateBatchWarehouseBalanceDto
    {
    }
}
