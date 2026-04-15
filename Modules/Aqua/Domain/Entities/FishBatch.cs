using System;
using System.Collections.Generic;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishBatch : BaseEntity
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

        public Project? Project { get; set; }
        public StockEntity? FishStock { get; set; }
        public GoodsReceiptLine? SourceGoodsReceiptLine { get; set; }

        public ICollection<BatchCageBalance> BatchCageBalances { get; set; } = new List<BatchCageBalance>();
        public ICollection<GoodsReceiptLine> GoodsReceiptLines { get; set; } = new List<GoodsReceiptLine>();
        public ICollection<GoodsReceiptFishDistribution> GoodsReceiptFishDistributions { get; set; } = new List<GoodsReceiptFishDistribution>();
        public ICollection<FeedingDistribution> FeedingDistributions { get; set; } = new List<FeedingDistribution>();
        public ICollection<MortalityLine> MortalityLines { get; set; } = new List<MortalityLine>();
        public ICollection<TransferLine> TransferLines { get; set; } = new List<TransferLine>();
        public ICollection<ShipmentLine> ShipmentLines { get; set; } = new List<ShipmentLine>();
        public ICollection<WeighingLine> WeighingLines { get; set; } = new List<WeighingLine>();
        public ICollection<StockConvertLine> StockConvertFromLines { get; set; } = new List<StockConvertLine>();
        public ICollection<StockConvertLine> StockConvertToLines { get; set; } = new List<StockConvertLine>();
        public ICollection<BatchMovement> BatchMovements { get; set; } = new List<BatchMovement>();
        public ICollection<FishHealthEvent> FishHealthEvents { get; set; } = new List<FishHealthEvent>();
        public ICollection<FishTreatment> FishTreatments { get; set; } = new List<FishTreatment>();
        public ICollection<FishLabSample> FishLabSamples { get; set; } = new List<FishLabSample>();
        public ICollection<WelfareAssessment> WelfareAssessments { get; set; } = new List<WelfareAssessment>();
        public ICollection<ComplianceAudit> ComplianceAudits { get; set; } = new List<ComplianceAudit>();
    }
}
