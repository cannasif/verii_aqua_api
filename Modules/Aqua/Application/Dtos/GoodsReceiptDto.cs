using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class GoodsReceiptDto
    {
        public long Id { get; set; }
        public long? ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public DocumentStatus Status { get; set; }
        public long? SupplierId { get; set; }
        public long? WarehouseId { get; set; }
        public short? WarehouseCode { get; set; }
        public string? WarehouseName { get; set; }
        public string? Note { get; set; }
    }

    public class CreateGoodsReceiptDto
    {
        public long? ProjectId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public DocumentStatus Status { get; set; }
        public long? SupplierId { get; set; }
        public long? WarehouseId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateGoodsReceiptDto : CreateGoodsReceiptDto
    {
    }
}
