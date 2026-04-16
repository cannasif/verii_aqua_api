using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
