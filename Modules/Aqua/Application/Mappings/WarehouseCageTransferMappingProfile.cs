using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class WarehouseCageTransferMappingProfile : Profile
    {
        public WarehouseCageTransferMappingProfile()
        {
            CreateMap<WarehouseCageTransfer, WarehouseCageTransferDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateWarehouseCageTransferDto, WarehouseCageTransfer>();
            CreateMap<UpdateWarehouseCageTransferDto, WarehouseCageTransfer>();
        }
    }
}
