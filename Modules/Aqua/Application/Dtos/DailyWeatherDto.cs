using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class DailyWeatherDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public DateTime WeatherDate { get; set; }
        public long WeatherTypeId { get; set; }
        public string? WeatherTypeCode { get; set; }
        public string? WeatherTypeName { get; set; }
        public long WeatherSeverityId { get; set; }
        public string? WeatherSeverityCode { get; set; }
        public string? WeatherSeverityName { get; set; }
        public int? WeatherSeverityScore { get; set; }
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
        public decimal OperationalRiskScore { get; set; }
        public string OperationalRiskLevel { get; set; } = string.Empty;
        public string? OperationalRiskFormula { get; set; }
        public string? Note { get; set; }
    }

    public class CreateDailyWeatherDto
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
    }

    public class UpdateDailyWeatherDto : CreateDailyWeatherDto
    {
    }
}
