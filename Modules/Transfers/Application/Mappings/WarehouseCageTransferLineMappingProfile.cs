using AutoMapper;

namespace aqua_api.Modules.Transfers.Application.Mappings
{
    public class WarehouseCageTransferLineMappingProfile : Profile
    {
        public WarehouseCageTransferLineMappingProfile()
        {
            CreateMap<WarehouseCageTransferLine, WarehouseCageTransferLineDto>();
            CreateMap<CreateWarehouseCageTransferLineDto, WarehouseCageTransferLine>();
            CreateMap<UpdateWarehouseCageTransferLineDto, WarehouseCageTransferLine>();
        }
    }
}
