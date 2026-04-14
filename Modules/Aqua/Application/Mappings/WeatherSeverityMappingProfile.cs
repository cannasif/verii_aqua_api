using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class WeatherSeverityMappingProfile : Profile
    {
        public WeatherSeverityMappingProfile()
        {
            CreateMap<WeatherSeverity, WeatherSeverityDto>()
                .ForMember(dest => dest.WeatherTypeCode, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Code : null))
                .ForMember(dest => dest.WeatherTypeName, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Name : null));
            CreateMap<CreateWeatherSeverityDto, WeatherSeverity>();
            CreateMap<UpdateWeatherSeverityDto, WeatherSeverity>();
        }
    }
}
