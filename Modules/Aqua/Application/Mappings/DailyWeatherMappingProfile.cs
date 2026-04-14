using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class DailyWeatherMappingProfile : Profile
    {
        public DailyWeatherMappingProfile()
        {
            CreateMap<DailyWeather, DailyWeatherDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null))
                .ForMember(dest => dest.WeatherTypeCode, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Code : null))
                .ForMember(dest => dest.WeatherTypeName, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Name : null))
                .ForMember(dest => dest.WeatherSeverityCode, opt => opt.MapFrom(src => src.WeatherSeverity != null ? src.WeatherSeverity.Code : null))
                .ForMember(dest => dest.WeatherSeverityName, opt => opt.MapFrom(src => src.WeatherSeverity != null ? src.WeatherSeverity.Name : null));
            CreateMap<CreateDailyWeatherDto, DailyWeather>();
            CreateMap<UpdateDailyWeatherDto, DailyWeather>();
        }
    }
}
