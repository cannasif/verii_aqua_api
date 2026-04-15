using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WarehouseCageTransferLineDto
    {
        public long Id { get; set; }
        public long WarehouseCageTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public short? FromWarehouseCode { get; set; }
        public string? FromWarehouseName { get; set; }
        public long ToProjectCageId { get; set; }
        public string? ToProjectCode { get; set; }
        public string? ToCageCode { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateWarehouseCageTransferLineDto
    {
        public long WarehouseCageTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateWarehouseCageTransferLineDto : CreateWarehouseCageTransferLineDto
    {
    }

    public class CreateWarehouseCageTransferLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime TransferDate { get; set; }
        public long FishBatchId { get; set; }
        public long FromWarehouseId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }
}
