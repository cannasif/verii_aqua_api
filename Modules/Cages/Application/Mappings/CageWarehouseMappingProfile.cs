using AutoMapper;

namespace aqua_api.Modules.Cages.Application.Mappings
{
    public class CageWarehouseMappingProfile : Profile
    {
        public CageWarehouseMappingProfile()
        {
            CreateMap<CageWarehouseMapping, CageWarehouseMappingDto>()
                .ForMember(dest => dest.CageCode, opt => opt.MapFrom(src => src.Cage != null ? src.Cage.CageCode : null))
                .ForMember(dest => dest.CageName, opt => opt.MapFrom(src => src.Cage != null ? src.Cage.CageName : null))
                .ForMember(dest => dest.ErpWarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.ErpWarehouseCode : (short?)null))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.WarehouseName : null));
            CreateMap<CreateCageWarehouseMappingDto, CageWarehouseMapping>();
            CreateMap<UpdateCageWarehouseMappingDto, CageWarehouseMapping>();
        }
    }
}
