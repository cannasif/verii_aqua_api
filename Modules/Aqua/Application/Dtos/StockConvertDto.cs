using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class StockConvertDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string ConvertNo { get; set; } = string.Empty;
        public DateTime ConvertDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateStockConvertDto
    {
        public long ProjectId { get; set; }
        public string ConvertNo { get; set; } = string.Empty;
        public DateTime ConvertDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateStockConvertDto : CreateStockConvertDto
    {
    }
}
