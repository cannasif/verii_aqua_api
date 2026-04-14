using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class BatchMovementMappingProfile : Profile
    {
        public BatchMovementMappingProfile()
        {
            CreateMap<BatchMovement, BatchMovementDto>();
            CreateMap<CreateBatchMovementDto, BatchMovement>();
            CreateMap<UpdateBatchMovementDto, BatchMovement>();
        }
    }
}
