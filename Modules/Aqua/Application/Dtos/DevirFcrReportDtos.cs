namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class DevirFcrReportRequestDto
    {
        public List<long> ProjectIds { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class DevirFcrReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<DevirFcrReportRowDto> Rows { get; set; } = new();
        public DevirFcrReportTotalDto Totals { get; set; } = new();
    }

    public class DevirFcrReportRowDto
    {
        public long ProjectId { get; set; }
        public string ProjectCode { get; set; } = "-";
        public string ProjectName { get; set; } = "-";
        public int OpeningFishCount { get; set; }
        public int ShipmentFishCount { get; set; }
        public int MortalityFishCount { get; set; }
        public decimal? MortalityPct { get; set; }
        public int EndingFishCount { get; set; }
        public decimal EndingAverageGram { get; set; }
        public decimal OpeningBiomassKg { get; set; }
        public decimal EndingBiomassKg { get; set; }
        public decimal ShippedBiomassKg { get; set; }
        public decimal MortalityBiomassKg { get; set; }
        public decimal TotalFeedKg { get; set; }
        public decimal ProducedBiomassKg { get; set; }
        public decimal? Fcr { get; set; }
    }

    public class DevirFcrReportTotalDto : DevirFcrReportRowDto
    {
        public DevirFcrReportTotalDto()
        {
            ProjectCode = "TOPLAM";
            ProjectName = string.Empty;
        }
    }
}
