using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class ShipmentMappingProfile : Profile
    {
        public ShipmentMappingProfile()
        {
            CreateMap<Shipment, ShipmentDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateShipmentDto, Shipment>();
            CreateMap<UpdateShipmentDto, Shipment>();
        }
    }
}
