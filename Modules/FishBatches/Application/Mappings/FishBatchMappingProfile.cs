using AutoMapper;

namespace aqua_api.Modules.FishBatches.Application.Mappings
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
