using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class FishBatchMappingProfile : Profile
    {
        public FishBatchMappingProfile()
        {
            CreateMap<FishBatch, FishBatchDto>();
            CreateMap<CreateFishBatchDto, FishBatch>();
            CreateMap<UpdateFishBatchDto, FishBatch>();
        }
    }
}
