using AutoMapper;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Warehouse.Application.Mappings
{
    public class WarehouseMappingProfile : Profile
    {
        public WarehouseMappingProfile()
        {
            CreateMap<WarehouseEntity, WarehouseDto>();
        }
    }
}
