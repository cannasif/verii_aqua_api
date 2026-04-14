using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WeatherSeverityDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public long? WeatherTypeId { get; set; }
        public string? WeatherTypeCode { get; set; }
        public string? WeatherTypeName { get; set; }
    }

    public class CreateWeatherSeverityDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public long WeatherTypeId { get; set; }
    }

    public class UpdateWeatherSeverityDto : CreateWeatherSeverityDto
    {
    }
}
