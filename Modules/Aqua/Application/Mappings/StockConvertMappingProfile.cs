using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class StockConvertMappingProfile : Profile
    {
        public StockConvertMappingProfile()
        {
            CreateMap<StockConvert, StockConvertDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateStockConvertDto, StockConvert>();
            CreateMap<UpdateStockConvertDto, StockConvert>();
        }
    }
}
