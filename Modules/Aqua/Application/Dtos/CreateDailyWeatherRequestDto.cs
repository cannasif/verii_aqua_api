using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class CreateDailyWeatherRequestDto
    {
        public long ProjectId { get; set; }
        public DateTime WeatherDate { get; set; }
        public long WeatherTypeId { get; set; }
        public long WeatherSeverityId { get; set; }
        public decimal? TemperatureC { get; set; }
        public decimal? WindKnot { get; set; }
        public string? Note { get; set; }
    }
}
