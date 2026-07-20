using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Application.Dtos;

public class BudgetPlanDto
{
    public long Id { get; set; }
    public string BudgetNo { get; set; } = string.Empty;
    public string BudgetCode { get; set; } = string.Empty;
    public string BudgetName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int StartMonth { get; set; }
    public int EndYear { get; set; }
    public int EndMonth { get; set; }
    public BudgetPlanStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public int FishBatchCount { get; set; }
    public decimal TotalInitialBiomassKg { get; set; }
    public decimal TotalSalesTon { get; set; }
    public decimal TotalFeedKg { get; set; }
    public decimal TotalMortalityKg { get; set; }
}

public class CreateBudgetPlanDto
{
    public string? BudgetCode { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int StartMonth { get; set; }
    public int EndYear { get; set; }
    public int EndMonth { get; set; }
    public string? Description { get; set; }
}

public class CopyBudgetPlanDto
{
    public string? BudgetCode { get; set; }
    public string? BudgetName { get; set; }
    public int? StartYear { get; set; }
    public int? StartMonth { get; set; }
    public int? EndYear { get; set; }
    public int? EndMonth { get; set; }
    public bool IncludeCalculatedResults { get; set; } = true;
    public bool ResetToDraft { get; set; }
    public string? Description { get; set; }
}

public class BudgetPlanProjectDto
{
    public long Id { get; set; }
    public long BudgetPlanId { get; set; }
    public BudgetPlanSourceType SourceType { get; set; }
    public long? SourceProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
}

public class BudgetPlanFishBatchDto
{
    public long Id { get; set; }
    public long BudgetPlanId { get; set; }
    public long BudgetPlanProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public BudgetPlanSourceType SourceType { get; set; }
    public long? SourceFishBatchId { get; set; }
    public long FishStockId { get; set; }
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public int InitialLiveCount { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal InitialBiomassKg { get; set; }
    public decimal? InitialUnitCost { get; set; }
    public decimal? InitialSmmAmount { get; set; }
    public int GrowthStartYear { get; set; }
    public int GrowthStartMonth { get; set; }
    public string? Note { get; set; }
}

public class CreateBudgetPlanFishBatchAdjustmentDto
{
    public long BudgetPlanFishBatchId { get; set; }
    public BudgetPlanFishBatchAdjustmentType AdjustmentType { get; set; }
    public int LiveCount { get; set; }
    public decimal? AverageGram { get; set; }
    public string? Description { get; set; }
}

public class BudgetPlanFishBatchAdjustmentDto : CreateBudgetPlanFishBatchAdjustmentDto
{
    public long Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public decimal BiomassKg { get; set; }
}

public class AddActualFishBatchesToBudgetDto
{
    public List<long> FishBatchIds { get; set; } = new();
    public int? GrowthStartYear { get; set; }
    public int? GrowthStartMonth { get; set; }
}

public class AddVirtualFishBatchDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public long FishStockId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public int InitialLiveCount { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal? InitialUnitCost { get; set; }
    public decimal? InitialSmmAmount { get; set; }
    public int GrowthStartYear { get; set; }
    public int GrowthStartMonth { get; set; }
    public string? Note { get; set; }
}

public class BudgetAvailableFishBatchDto
{
    public long FishBatchId { get; set; }
    public long ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public long FishStockId { get; set; }
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public int LiveCount { get; set; }
    public decimal AverageGram { get; set; }
    public decimal BiomassKg { get; set; }
    public DateTime AsOfDate { get; set; }
}

public class UpsertBudgetPlanSalesLineDto
{
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public decimal SalesTon { get; set; }
    public int? SalesCount { get; set; }
    public string CurrencyCode { get; set; } = "EUR";
    public decimal? UnitPrice { get; set; }
    public string? Description { get; set; }
}

public class BudgetPlanSalesLineDto : UpsertBudgetPlanSalesLineDto
{
    public long Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public decimal SalesAmount { get; set; }
    public decimal? SourceUnitPrice { get; set; }
    public decimal SalesAmountCurrency { get; set; }
    public decimal? UnitPriceEuro { get; set; }
    public decimal SalesAmountEuro { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? SalesAmountTry { get; set; }
}

public class BudgetSalesPlanningRowDto
{
    public long BudgetPlanFishBatchId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal AverageGram { get; set; }
    public decimal AverageKg { get; set; }
    public int AvailableCount { get; set; }
    public decimal AvailableKg { get; set; }
    public decimal AvailableTon { get; set; }
    public decimal PlannedSalesTon { get; set; }
    public decimal DomesticSalesTon { get; set; }
    public decimal ForeignSalesTon { get; set; }
    public int PlannedSalesCount { get; set; }
    public decimal RemainingKg { get; set; }
    public decimal RemainingTon { get; set; }
    public int RemainingCount { get; set; }
}

public class UpsertBudgetPlanSalesTonDto
{
    public long BudgetPlanFishBatchId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public decimal SalesTon { get; set; }
    public string CurrencyCode { get; set; } = "EUR";
    public decimal? UnitPrice { get; set; }
    public string? Description { get; set; }
}

public class ImportBudgetPlanSalesTonsDto
{
    public List<UpsertBudgetPlanSalesTonDto> Lines { get; set; } = new();
}

public class GenerateBudgetPlanExchangeRatesDto
{
    public List<string> CurrencyCodes { get; set; } = new();
    public string RateType { get; set; } = "Budget";
    public decimal DefaultExchangeRate { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? SourceReference { get; set; }
}

public class UpsertBudgetPlanExchangeRateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string RateType { get; set; } = "Budget";
    public decimal ExchangeRate { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? SourceReference { get; set; }
    public bool IsManualOverride { get; set; } = true;
    public string? Description { get; set; }
}

public class BudgetPlanExchangeRateDto : UpsertBudgetPlanExchangeRateDto
{
    public long Id { get; set; }
    public long BudgetPlanId { get; set; }
}

public class UpsertBudgetPlanFishPriceDto
{
    public long? FishStockId { get; set; }
    public long CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetFishPriceType PriceType { get; set; } = BudgetFishPriceType.Sales;
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public string CurrencyCode { get; set; } = "EUR";
    public decimal UnitPrice { get; set; }
    public decimal? UnitPriceEuro { get; set; }
    public decimal IncreaseRatePercent { get; set; }
    public int IncreasePeriodMonths { get; set; } = 1;
    public string? Description { get; set; }
}

public class BudgetPlanSalesDistributionDto
{
    public long Id { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public BudgetMarketType MarketType { get; set; }
    public string? CalibrationCode { get; set; }
    public decimal SalesTon { get; set; }
    public decimal SalesKg { get; set; }
    public int SalesCount { get; set; }
    public decimal UnitGram { get; set; }
    public string CurrencyCode { get; set; } = "EUR";
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceEuro { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountEuro { get; set; }
    public decimal? AmountTry { get; set; }
}

public class GenerateBudgetPlanFishPricesDto
{
    public BudgetFishPriceType PriceType { get; set; } = BudgetFishPriceType.Sales;
    public BudgetMarketType MarketType { get; set; } = BudgetMarketType.Domestic;
    public string CurrencyCode { get; set; } = "EUR";
    public decimal DefaultUnitPrice { get; set; }
    public decimal? DefaultUnitPriceEuro { get; set; }
    public decimal IncreaseRatePercent { get; set; }
    public int IncreasePeriodMonths { get; set; } = 1;
    public List<long> FishStockIds { get; set; } = new();
    public List<long> CalibrationDefinitionIds { get; set; } = new();
}

public class BudgetPlanFishPriceDto : UpsertBudgetPlanFishPriceDto
{
    public long Id { get; set; }
    public long BudgetPlanId { get; set; }
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public string CalibrationCode { get; set; } = string.Empty;
    public string CalibrationInfo { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public decimal? ExchangeRate { get; set; }
    public decimal? UnitPriceTry { get; set; }
}

public class BudgetPlanFeedingLineDto
{
    public long Id { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public long? FeedStockId { get; set; }
    public string? FeedStockCode { get; set; }
    public string? FeedStockName { get; set; }
    public decimal FeedAmountRate { get; set; }
    public decimal MortalityReductionPercent { get; set; }
    public decimal MortalityReductionKg { get; set; }
    public decimal FeedKg { get; set; }
}

public class BudgetPlanMortalityLineDto
{
    public long Id { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal MortalityRatePercent { get; set; }
    public int MortalityCount { get; set; }
    public decimal MortalityKg { get; set; }
}

public class BudgetPlanMonthlyProjectionDto
{
    public long Id { get; set; }
    public long BudgetPlanFishBatchId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int MonthIndex { get; set; }
    public int OpeningLiveCount { get; set; }
    public decimal OpeningAverageGram { get; set; }
    public decimal OpeningBiomassKg { get; set; }
    public decimal RawMonthlyGrowthGram { get; set; }
    public decimal GrowthQualityPercent { get; set; }
    public decimal MonthlyGrowthGram { get; set; }
    public decimal ClosingAverageGram { get; set; }
    public decimal SalesTon { get; set; }
    public int SalesCount { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public decimal FeedKg { get; set; }
    public decimal FeedMortalityReductionPercent { get; set; }
    public decimal FeedMortalityReductionKg { get; set; }
    public int ClosingLiveCount { get; set; }
    public decimal ClosingBiomassKg { get; set; }
    public string? CalibrationCode { get; set; }
    public decimal? WaterTemperatureCelsius { get; set; }
}

public class BudgetKpiSummaryDto
{
    public long BudgetPlanId { get; set; }
    public string BudgetNo { get; set; } = string.Empty;
    public string BudgetCode { get; set; } = string.Empty;
    public decimal InitialBiomassKg { get; set; }
    public decimal FinalBiomassKg { get; set; }
    public decimal SalesTon { get; set; }
    public decimal FeedKg { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public int InitialLiveCount { get; set; }
    public int SalesCount { get; set; }
    public int FinalLiveCount { get; set; }
    public decimal ProducedBiomassKg { get; set; }
    public decimal Fcr { get; set; }
    public decimal MortalityRatePercent { get; set; }
}

public class CreateBudgetMortalityRateDefinitionDto
{
    public long? FishStockId { get; set; }
    public long? CalibrationDefinitionId { get; set; }
    public int? GrowthMonthNo { get; set; }
    public decimal MortalityRatePercent { get; set; }
    public string? Description { get; set; }
}

public class BudgetMortalityRateDefinitionDto : CreateBudgetMortalityRateDefinitionDto
{
    public long Id { get; set; }
    public string? FishStockCode { get; set; }
    public string? FishStockName { get; set; }
    public string? CalibrationCode { get; set; }
}
