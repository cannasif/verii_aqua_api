using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WeatherTypeDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CreateWeatherTypeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateWeatherTypeDto : CreateWeatherTypeDto
    {
    }
}
