using AutoMapper;

namespace aqua_api.Modules.Weather.Application.Mappings
{
    public class WeatherSeverityMappingProfile : Profile
    {
        public WeatherSeverityMappingProfile()
        {
            CreateMap<WeatherSeverity, WeatherSeverityDto>();
            CreateMap<CreateWeatherSeverityDto, WeatherSeverity>();
            CreateMap<UpdateWeatherSeverityDto, WeatherSeverity>();
        }
    }
}
