namespace aqua_api.Modules.KpiReport.Application.Dtos;

public class KpiReportProjectOptionDto
{
    public long Id { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
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
