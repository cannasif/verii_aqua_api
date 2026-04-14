using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
