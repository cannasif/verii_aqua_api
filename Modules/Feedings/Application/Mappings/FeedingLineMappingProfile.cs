using AutoMapper;

namespace aqua_api.Modules.Feedings.Application.Mappings
{
    public class FeedingLineMappingProfile : Profile
    {
        public FeedingLineMappingProfile()
        {
            CreateMap<FeedingLine, FeedingLineDto>();
            CreateMap<CreateFeedingLineDto, FeedingLine>();
            CreateMap<UpdateFeedingLineDto, FeedingLine>();
        }
    }
}
