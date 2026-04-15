using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class CageWarehouseTransferMappingProfile : Profile
    {
        public CageWarehouseTransferMappingProfile()
        {
            CreateMap<CageWarehouseTransfer, CageWarehouseTransferDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateCageWarehouseTransferDto, CageWarehouseTransfer>();
            CreateMap<UpdateCageWarehouseTransferDto, CageWarehouseTransfer>();
        }
    }
}
