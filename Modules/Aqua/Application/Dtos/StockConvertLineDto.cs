using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class StockConvertLineDto
    {
        public long Id { get; set; }
        public long StockConvertId { get; set; }
        public long FromFishBatchId { get; set; }
        public string? FromBatchCode { get; set; }
        public long ToFishBatchId { get; set; }
        public string? ToBatchCode { get; set; }
        public long FromProjectCageId { get; set; }
        public string? FromProjectCode { get; set; }
        public string? FromProjectName { get; set; }
        public string? FromCageCode { get; set; }
        public string? FromCageName { get; set; }
        public long ToProjectCageId { get; set; }
        public string? ToProjectCode { get; set; }
        public string? ToProjectName { get; set; }
        public string? ToCageCode { get; set; }
        public string? ToCageName { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal NewAverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateStockConvertLineDto
    {
        public long StockConvertId { get; set; }
        public long FromFishBatchId { get; set; }
        public long ToFishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal NewAverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateStockConvertLineDto : CreateStockConvertLineDto
    {
    }

    public class CreateStockConvertLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime ConvertDate { get; set; }
        public long FromFishBatchId { get; set; }
        public long ToFishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal NewAverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }
}
