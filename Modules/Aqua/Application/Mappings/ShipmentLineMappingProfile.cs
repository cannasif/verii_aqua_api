using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class ShipmentLineMappingProfile : Profile
    {
        public ShipmentLineMappingProfile()
        {
            CreateMap<ShipmentLine, ShipmentLineDto>();
            CreateMap<CreateShipmentLineDto, ShipmentLine>();
            CreateMap<UpdateShipmentLineDto, ShipmentLine>();
        }
    }
}
