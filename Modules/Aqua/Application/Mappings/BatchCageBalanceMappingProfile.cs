using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class BatchCageBalanceMappingProfile : Profile
    {
        public BatchCageBalanceMappingProfile()
        {
            CreateMap<BatchCageBalance, BatchCageBalanceDto>();
            CreateMap<CreateBatchCageBalanceDto, BatchCageBalance>();
            CreateMap<UpdateBatchCageBalanceDto, BatchCageBalance>();
        }
    }
}
