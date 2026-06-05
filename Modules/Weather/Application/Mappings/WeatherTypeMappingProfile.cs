using AutoMapper;

namespace aqua_api.Modules.Weather.Application.Mappings
{
    public class WeatherTypeMappingProfile : Profile
    {
        public WeatherTypeMappingProfile()
        {
            CreateMap<WeatherType, WeatherTypeDto>();
            CreateMap<CreateWeatherTypeDto, WeatherType>();
            CreateMap<UpdateWeatherTypeDto, WeatherType>();
        }
    }
}
