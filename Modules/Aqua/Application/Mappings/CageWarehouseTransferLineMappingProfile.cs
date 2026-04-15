using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class CageWarehouseTransferLineMappingProfile : Profile
    {
        public CageWarehouseTransferLineMappingProfile()
        {
            CreateMap<CageWarehouseTransferLine, CageWarehouseTransferLineDto>();
            CreateMap<CreateCageWarehouseTransferLineDto, CageWarehouseTransferLine>();
            CreateMap<UpdateCageWarehouseTransferLineDto, CageWarehouseTransferLine>();
        }
    }
}
