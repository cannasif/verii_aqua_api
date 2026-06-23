namespace aqua_api.Modules.KpiReport.Application.Dtos;

public class KpiReportProjectOptionDto
{
    public long Id { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public DateTime StartDate { get; set; }
}

public class ProjectFeedFishSummaryRequestDto
{
    public List<long> ProjectIds { get; set; } = new();
}

public class ProjectFeedFishSummaryReportDto
{
    public List<ProjectFeedFishSummaryRowDto> Rows { get; set; } = new();
    public ProjectFeedFishSummaryTotalDto Totals { get; set; } = new();
}

public class ProjectFeedFishSummaryRowDto
{
    public long ProjectId { get; set; }
    public string ProjectCode { get; set; } = "-";
    public string ProjectName { get; set; } = "-";
    public int CageFish { get; set; }
    public int WarehouseFish { get; set; }
    public int TotalFish { get; set; }
    public decimal CageBiomassKg { get; set; }
    public decimal WarehouseBiomassKg { get; set; }
    public decimal TotalBiomassKg { get; set; }
    public decimal TotalFeedKg { get; set; }
    public int ActiveCageCount { get; set; }
}

public class ProjectFeedFishSummaryTotalDto
{
    public int CageFish { get; set; }
    public int WarehouseFish { get; set; }
    public int TotalFish { get; set; }
    public decimal CageBiomassKg { get; set; }
    public decimal WarehouseBiomassKg { get; set; }
    public decimal TotalBiomassKg { get; set; }
    public decimal TotalFeedKg { get; set; }
    public int ActiveCageCount { get; set; }
}

public class RawKpiReportDto
{
    public long ProjectId { get; set; }
    public string ProjectCode { get; set; } = "-";
    public string ProjectName { get; set; } = "-";
    public int DaysInSea { get; set; }
    public int StockedFish { get; set; }
    public int LiveFish { get; set; }
    public int WarehouseFish { get; set; }
    public int TotalSystemFish { get; set; }
    public int DeadFish { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal CurrentAverageGram { get; set; }
    public decimal CurrentBiomassKg { get; set; }
    public decimal WarehouseBiomassKg { get; set; }
    public decimal TotalSystemBiomassKg { get; set; }
    public decimal TotalFeedKg { get; set; }
    public decimal BiomassGainKg { get; set; }
    public decimal? SurvivalPct { get; set; }
    public decimal? MortalityPct { get; set; }
    public decimal? AdgGramPerDay { get; set; }
    public decimal? SgrPctPerDay { get; set; }
    public decimal? Fcr { get; set; }
    public decimal? DensityPct { get; set; }
    public decimal ForecastBiomassKg30d { get; set; }
    public List<RawKpiRowDto> Rows { get; set; } = new();
    public List<KpiMetricDefinitionDto> MetricDefinitions { get; set; } = new();
}

public class RawKpiRowDto
{
    public long ProjectCageId { get; set; }
    public string CageLabel { get; set; } = "-";
    public int DaysInSea { get; set; }
    public int StockedFish { get; set; }
    public int LiveFish { get; set; }
    public int DeadFish { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal CurrentAverageGram { get; set; }
    public decimal CurrentBiomassKg { get; set; }
    public decimal TotalFeedKg { get; set; }
    public decimal BiomassGainKg { get; set; }
    public decimal? SurvivalPct { get; set; }
    public decimal? MortalityPct { get; set; }
    public decimal? AdgGramPerDay { get; set; }
    public decimal? SgrPctPerDay { get; set; }
    public decimal? Fcr { get; set; }
    public decimal? DensityPct { get; set; }
    public decimal ForecastBiomassKg30d { get; set; }
}

public class KpiMetricDefinitionDto
{
    public string Key { get; set; } = string.Empty;
    public string LabelKey { get; set; } = string.Empty;
    public string DescriptionKey { get; set; } = string.Empty;
    public string FormulaKey { get; set; } = string.Empty;
}

public class ProjectDetailProjectDto
{
    public long Id { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public byte Status { get; set; }
}

public class ProjectDetailReportDto
{
    public ProjectDetailProjectDto Project { get; set; } = new();
    public List<ProjectDetailCageReportDto> Cages { get; set; } = new();
    public List<ProjectDetailCageHistoryItemDto> CageHistory { get; set; } = new();
    public ProjectDetailWarehouseSummaryDto WarehouseSummary { get; set; } = new();
}

public class ProjectDetailCageHistoryItemDto
{
    public long ProjectCageId { get; set; }
    public string CageLabel { get; set; } = "-";
    public DateTime? AssignedDate { get; set; }
    public DateTime? ReleasedDate { get; set; }
}

public class ProjectDetailWarehouseSummaryDto
{
    public int ActiveWarehouseCount { get; set; }
    public int WarehouseFishCount { get; set; }
    public decimal WarehouseBiomassGram { get; set; }
    public int TotalSystemFishCount { get; set; }
    public decimal TotalSystemBiomassGram { get; set; }
}

public class ProjectDetailCageReportDto
{
    public long ProjectCageId { get; set; }
    public string CageLabel { get; set; } = "-";
    public int InitialFishCount { get; set; }
    public decimal InitialAverageGram { get; set; }
    public decimal InitialBiomassGram { get; set; }
    public int CurrentFishCount { get; set; }
    public decimal CurrentAverageGram { get; set; }
    public decimal CurrentBiomassGram { get; set; }
    public int TotalDeadCount { get; set; }
    public decimal TotalFeedGram { get; set; }
    public int TotalCountDelta { get; set; }
    public decimal TotalBiomassDelta { get; set; }
    public List<string> MissingFeedingDays { get; set; } = new();
    public List<ProjectDetailCageDailyRowDto> DailyRows { get; set; } = new();
}

public class ProjectDetailCageDailyRowDto
{
    public string Date { get; set; } = string.Empty;
    public decimal FeedGram { get; set; }
    public int FeedStockCount { get; set; }
    public List<string> FeedDetails { get; set; } = new();
    public int DeadCount { get; set; }
    public decimal DeadBiomassGram { get; set; }
    public int CountDelta { get; set; }
    public decimal BiomassDelta { get; set; }
    public string Weather { get; set; } = "-";
    public int NetOperationCount { get; set; }
    public List<string> NetOperationDetails { get; set; } = new();
    public int TransferCount { get; set; }
    public List<string> TransferDetails { get; set; } = new();
    public int WeighingCount { get; set; }
    public List<string> WeighingDetails { get; set; } = new();
    public int StockConvertCount { get; set; }
    public List<string> StockConvertDetails { get; set; } = new();
    public int ShipmentCount { get; set; }
    public List<string> ShipmentDetails { get; set; } = new();
    public int ShipmentFishCount { get; set; }
    public decimal ShipmentBiomassGram { get; set; }
    public bool Fed { get; set; }
}

public class BusinessKpiReportDto
{
    public long ProjectId { get; set; }
    public string ProjectCode { get; set; } = "-";
    public string ProjectName { get; set; } = "-";
    public decimal EstimatedFeedCost { get; set; }
    public decimal? FeedCostPerCurrentKg { get; set; }
    public decimal ProjectedHarvestBiomassKg { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public decimal ProjectedGrossMargin { get; set; }
    public decimal? ProjectedMarginPct { get; set; }
    public decimal TargetWeightProgressPct { get; set; }
    public int? DaysToTarget { get; set; }
    public string? EstimatedHarvestDate { get; set; }
    public decimal ForecastConfidencePct { get; set; }
    public decimal HarvestReadinessPct { get; set; }
    public BusinessKpiAssumptionsDto Assumptions { get; set; } = new();
    public List<BusinessKpiRowDto> Rows { get; set; } = new();
    public List<KpiMetricDefinitionDto> MetricDefinitions { get; set; } = new();
}

public class BusinessKpiAssumptionsDto
{
    public int ForecastDays { get; set; }
    public decimal TargetHarvestGram { get; set; }
    public decimal FeedCostPerKg { get; set; }
    public decimal SalePricePerKg { get; set; }
}

public class BusinessKpiRowDto
{
    public long ProjectCageId { get; set; }
    public string CageLabel { get; set; } = "-";
    public decimal TargetWeightProgressPct { get; set; }
    public int? DaysToTarget { get; set; }
    public string? EstimatedHarvestDate { get; set; }
    public decimal ForecastConfidencePct { get; set; }
    public decimal HarvestReadinessPct { get; set; }
    public decimal EstimatedFeedCost { get; set; }
    public decimal? FeedCostPerCurrentKg { get; set; }
    public decimal ProjectedHarvestBiomassKg { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public decimal ProjectedGrossMargin { get; set; }
    public decimal? ProjectedMarginPct { get; set; }
}
