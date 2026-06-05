using AutoMapper;

namespace aqua_api.Modules.Transfers.Application.Mappings
{
    public class WarehouseTransferLineMappingProfile : Profile
    {
        public WarehouseTransferLineMappingProfile()
        {
            CreateMap<WarehouseTransferLine, WarehouseTransferLineDto>();
            CreateMap<CreateWarehouseTransferLineDto, WarehouseTransferLine>();
            CreateMap<UpdateWarehouseTransferLineDto, WarehouseTransferLine>();
        }
    }
}
