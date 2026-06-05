using AutoMapper;

namespace aqua_api.Modules.Feedings.Application.Mappings
{
    public class FeedingDistributionMappingProfile : Profile
    {
        public FeedingDistributionMappingProfile()
        {
            CreateMap<FeedingDistribution, FeedingDistributionDto>();
            CreateMap<CreateFeedingDistributionDto, FeedingDistribution>();
            CreateMap<UpdateFeedingDistributionDto, FeedingDistribution>();
        }
    }
}
