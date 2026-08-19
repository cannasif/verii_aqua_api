using AutoMapper;

namespace aqua_api.Modules.Shipments.Application.Mappings
{
    public class ShipmentLineMappingProfile : Profile
    {
        public ShipmentLineMappingProfile()
        {
            CreateMap<ShipmentLine, ShipmentLineDto>()
                .ForMember(
                    destination => destination.TotalKg,
                    options => options.MapFrom(source => source.TotalKg ?? source.BiomassGram / 1000m));
            CreateMap<CreateShipmentLineDto, ShipmentLine>();
            CreateMap<UpdateShipmentLineDto, ShipmentLine>();
        }
    }
}
