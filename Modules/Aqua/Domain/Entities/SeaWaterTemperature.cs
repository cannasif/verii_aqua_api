using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class SeaWaterTemperature : BaseEntity
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public DateTime RecordDate { get; set; }
        public decimal? WaterTemperatureCelsius { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
    }
}
