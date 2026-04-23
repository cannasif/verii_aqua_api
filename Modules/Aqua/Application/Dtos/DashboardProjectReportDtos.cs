namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class DashboardProjectsRequestDto
    {
        public List<long> ProjectIds { get; set; } = new();
    }

    public class DashboardProjectsResponseDto
    {
        public List<DashboardProjectSummaryDto> Projects { get; set; } = new();
        public bool YesterdayEntryMissing { get; set; }
        public DateTime? YesterdayDate { get; set; }
    }

    public class DashboardProjectSummaryDto
    {
        public long ProjectId { get; set; }
        public string ProjectCode { get; set; } = "-";
        public string ProjectName { get; set; } = "-";
        public decimal MeasurementAverageGram { get; set; }
        public int CageFish { get; set; }
        public int TotalShipmentCount { get; set; }
        public decimal TotalShipmentBiomassGram { get; set; }
        public int WarehouseFish { get; set; }
        public int TotalSystemFish { get; set; }
        public int TotalDeadCount { get; set; }
        public decimal TotalDeadBiomassGram { get; set; }
        public int ActiveCageCount { get; set; }
        public decimal? Fcr { get; set; }
        public decimal CageBiomassGram { get; set; }
        public decimal WarehouseBiomassGram { get; set; }
        public decimal TotalSystemBiomassGram { get; set; }
        public List<DashboardCageSummaryDto> Cages { get; set; } = new();
    }

    public class DashboardCageSummaryDto
    {
        public long ProjectCageId { get; set; }
        public string CageLabel { get; set; } = string.Empty;
        public decimal MeasurementAverageGram { get; set; }
        public int InitialFishCount { get; set; }
        public decimal InitialBiomassGram { get; set; }
        public int CurrentFishCount { get; set; }
        public int TotalShipmentCount { get; set; }
        public decimal TotalShipmentBiomassGram { get; set; }
        public int TotalDeadCount { get; set; }
        public decimal TotalDeadBiomassGram { get; set; }
        public decimal TotalFeedGram { get; set; }
        public decimal CurrentBiomassGram { get; set; }
        public decimal? Fcr { get; set; }
    }

    public class DashboardProjectDetailDto
    {
        public List<DashboardProjectDetailCageDto> Cages { get; set; } = new();
    }

    public class DashboardProjectDetailCageDto
    {
        public long ProjectCageId { get; set; }
        public string CageLabel { get; set; } = string.Empty;
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
        public List<DashboardCageDailyRowDto> DailyRows { get; set; } = new();
    }

    public class DashboardCageDailyRowDto
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
}
