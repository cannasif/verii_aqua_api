using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class MortalityMappingProfile : Profile
    {
        public MortalityMappingProfile()
        {
            CreateMap<Mortality, MortalityDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateMortalityDto, Mortality>();
            CreateMap<UpdateMortalityDto, Mortality>();
        }
    }
}
