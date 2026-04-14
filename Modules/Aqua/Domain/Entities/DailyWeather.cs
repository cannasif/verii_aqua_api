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
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public WeatherType? WeatherType { get; set; }
        public WeatherSeverity? WeatherSeverity { get; set; }
    }
}
