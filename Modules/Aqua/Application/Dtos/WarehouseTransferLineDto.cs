using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WarehouseTransferLineDto
    {
        public long Id { get; set; }
        public long WarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public short? FromWarehouseCode { get; set; }
        public string? FromWarehouseName { get; set; }
        public long ToWarehouseId { get; set; }
        public short? ToWarehouseCode { get; set; }
        public string? ToWarehouseName { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateWarehouseTransferLineDto
    {
        public long WarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateWarehouseTransferLineDto : CreateWarehouseTransferLineDto
    {
    }

    public class CreateWarehouseTransferLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime TransferDate { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }
}
