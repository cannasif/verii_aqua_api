using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
