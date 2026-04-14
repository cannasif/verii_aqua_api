using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class WeatherSeverity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }

        public ICollection<DailyWeather> DailyWeathers { get; set; } = new List<DailyWeather>();
    }
}
