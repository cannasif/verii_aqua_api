using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class FeedingMappingProfile : Profile
    {
        public FeedingMappingProfile()
        {
            CreateMap<Feeding, FeedingDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateFeedingDto, Feeding>();
            CreateMap<UpdateFeedingDto, Feeding>();
        }
    }
}
