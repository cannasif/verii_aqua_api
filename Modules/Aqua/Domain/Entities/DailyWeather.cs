using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class DailyWeather : BaseEntity
    {
        public long ProjectId { get; set; }
        public DateTime WeatherDate { get; set; }
        public long WeatherTypeId { get; set; }
        public long WeatherSeverityId { get; set; }
        public decimal? TemperatureC { get; set; }
        public decimal? WindKnot { get; set; }
        public decimal? WaterTemperatureSurfaceC { get; set; }
        public decimal? WaterTemperatureDepthC { get; set; }
        public decimal? DissolvedOxygenMgL { get; set; }
        public decimal? SalinityPpt { get; set; }
        public decimal? Ph { get; set; }
        public decimal? CurrentSpeedKn { get; set; }
        public decimal? WaveHeightM { get; set; }
        public decimal? TurbidityNtu { get; set; }
        public decimal? AmmoniaMgL { get; set; }
        public decimal? NitriteMgL { get; set; }
        public decimal? AlgalBloomIndex { get; set; }
        public decimal? SensorHealthScore { get; set; }
        public DateTime? SensorRecordedAt { get; set; }
        public string? DataSource { get; set; }
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public WeatherType? WeatherType { get; set; }
        public WeatherSeverity? WeatherSeverity { get; set; }
    }
}
