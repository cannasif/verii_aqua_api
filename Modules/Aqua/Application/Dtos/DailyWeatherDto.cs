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
        public decimal? TemperatureC { get; set; }
        public decimal? WindKnot { get; set; }
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
        public string? Note { get; set; }
    }

    public class UpdateDailyWeatherDto : CreateDailyWeatherDto
    {
    }
}
