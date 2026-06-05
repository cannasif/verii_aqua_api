using AutoMapper;

namespace aqua_api.Modules.BatchBalances.Application.Mappings
{
    public class BatchWarehouseBalanceMappingProfile : Profile
    {
        public BatchWarehouseBalanceMappingProfile()
        {
            CreateMap<BatchWarehouseBalance, BatchWarehouseBalanceDto>();
            CreateMap<CreateBatchWarehouseBalanceDto, BatchWarehouseBalance>();
            CreateMap<UpdateBatchWarehouseBalanceDto, BatchWarehouseBalance>();
        }
    }
}
