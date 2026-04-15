namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ProjectCageDailyKpiSnapshot : BaseEntity
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long FishBatchId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public int InitialCount { get; set; }
        public int LiveCount { get; set; }
        public int DeadCountPeriod { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassKg { get; set; }
        public decimal FeedKgPeriod { get; set; }
        public decimal BiomassGainKgPeriod { get; set; }
        public decimal SurvivalPct { get; set; }
        public decimal MortalityPctPeriod { get; set; }
        public decimal Fcr { get; set; }
        public decimal Adg { get; set; }
        public decimal Sgr { get; set; }
        public decimal CapacityUsagePct { get; set; }
        public decimal ForecastBiomassKg30Days { get; set; }
        public decimal HarvestReadinessScore { get; set; }
        public decimal DataQualityScore { get; set; }
        public string? FormulaNote { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
