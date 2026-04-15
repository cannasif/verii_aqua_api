using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WarehouseTransferDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateWarehouseTransferDto
    {
        public long ProjectId { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateWarehouseTransferDto : CreateWarehouseTransferDto
    {
    }
}
