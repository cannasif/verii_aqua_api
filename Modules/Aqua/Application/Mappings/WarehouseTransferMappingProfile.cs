using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class WarehouseTransferMappingProfile : Profile
    {
        public WarehouseTransferMappingProfile()
        {
            CreateMap<WarehouseTransfer, WarehouseTransferDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateWarehouseTransferDto, WarehouseTransfer>();
            CreateMap<UpdateWarehouseTransferDto, WarehouseTransfer>();
        }
    }
}
