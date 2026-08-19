using System;

namespace aqua_api.Modules.Weather.Application.Dtos
{
    public class WeatherSeverityDto : AuditDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class CreateWeatherSeverityDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class UpdateWeatherSeverityDto : CreateWeatherSeverityDto
    {
    }
}
