using AutoMapper;

namespace aqua_api.Modules.NetInventory.Application.Mappings;

public class NetInventoryMovementMappingProfile : Profile
{
    public NetInventoryMovementMappingProfile()
    {
        CreateMap<CreateNetInventoryMovementDto, NetInventoryMovement>();
        CreateMap<UpdateNetInventoryMovementDto, NetInventoryMovement>();
    }
}
