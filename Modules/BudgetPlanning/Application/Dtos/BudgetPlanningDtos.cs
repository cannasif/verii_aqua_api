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
    public decimal TotalSalesKg { get; set; }
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
    public decimal SalesKg { get; set; }
    public int? SalesCount { get; set; }
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
    public decimal PlannedSalesKg { get; set; }
    public decimal PlannedSalesTon { get; set; }
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
    public decimal SalesTon { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Description { get; set; }
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
    public long CalibrationDefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UnitPriceEuro { get; set; }
    public string? Description { get; set; }
}

public class GenerateBudgetPlanFishPricesDto
{
    public decimal DefaultUnitPriceEuro { get; set; }
    public List<long> CalibrationDefinitionIds { get; set; } = new();
}

public class BudgetPlanFishPriceDto : UpsertBudgetPlanFishPriceDto
{
    public long Id { get; set; }
    public long BudgetPlanId { get; set; }
    public string CalibrationCode { get; set; } = string.Empty;
    public string CalibrationInfo { get; set; } = string.Empty;
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
    public decimal MonthlyGrowthGram { get; set; }
    public decimal ClosingAverageGram { get; set; }
    public decimal SalesKg { get; set; }
    public int SalesCount { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
    public decimal FeedKg { get; set; }
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
    public decimal SalesKg { get; set; }
    public decimal FeedKg { get; set; }
    public decimal MortalityKg { get; set; }
    public int MortalityCount { get; set; }
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
