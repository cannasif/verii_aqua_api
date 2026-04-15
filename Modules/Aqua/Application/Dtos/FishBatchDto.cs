using System;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class FishBatchDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public long FishStockId { get; set; }
        public decimal CurrentAverageGram { get; set; }
        public DateTime StartDate { get; set; }
        public long? SourceGoodsReceiptLineId { get; set; }
        public long? SupplierId { get; set; }
        public string? SupplierLotCode { get; set; }
        public string? HatcheryName { get; set; }
        public string? OriginCountryCode { get; set; }
        public string? StrainCode { get; set; }
        public string? GenerationCode { get; set; }
        public string? BroodstockCode { get; set; }
        public bool IsVaccinated { get; set; }
        public DateTime? VaccinationDate { get; set; }
        public string? VaccinationNote { get; set; }
        public string? TreatmentHistoryNote { get; set; }
        public decimal? TargetHarvestAverageGram { get; set; }
        public DateTime? TargetHarvestDate { get; set; }
        public string? TargetHarvestClass { get; set; }
        public string? QualityGrade { get; set; }
        public StockEntity? FishStock { get; set; }
    }

    public class CreateFishBatchDto
    {
        public long ProjectId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public long FishStockId { get; set; }
        public decimal CurrentAverageGram { get; set; }
        public DateTime StartDate { get; set; }
        public long? SourceGoodsReceiptLineId { get; set; }
        public long? SupplierId { get; set; }
        public string? SupplierLotCode { get; set; }
        public string? HatcheryName { get; set; }
        public string? OriginCountryCode { get; set; }
        public string? StrainCode { get; set; }
        public string? GenerationCode { get; set; }
        public string? BroodstockCode { get; set; }
        public bool IsVaccinated { get; set; }
        public DateTime? VaccinationDate { get; set; }
        public string? VaccinationNote { get; set; }
        public string? TreatmentHistoryNote { get; set; }
        public decimal? TargetHarvestAverageGram { get; set; }
        public DateTime? TargetHarvestDate { get; set; }
        public string? TargetHarvestClass { get; set; }
        public string? QualityGrade { get; set; }
        public StockEntity? FishStock { get; set; }
    }

    public class UpdateFishBatchDto : CreateFishBatchDto
    {
    }
}
