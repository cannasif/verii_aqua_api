using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
