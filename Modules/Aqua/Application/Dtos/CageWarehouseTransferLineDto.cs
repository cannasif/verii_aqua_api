using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class CageWarehouseTransferLineDto
    {
        public long Id { get; set; }
        public long CageWarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public long FromProjectCageId { get; set; }
        public string? FromProjectCode { get; set; }
        public string? FromCageCode { get; set; }
        public long ToWarehouseId { get; set; }
        public short? ToWarehouseCode { get; set; }
        public string? ToWarehouseName { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateCageWarehouseTransferLineDto
    {
        public long CageWarehouseTransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateCageWarehouseTransferLineDto : CreateCageWarehouseTransferLineDto
    {
    }

    public class CreateCageWarehouseTransferLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime TransferDate { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToWarehouseId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }
}
