using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class NetOperationMappingProfile : Profile
    {
        public NetOperationMappingProfile()
        {
            CreateMap<NetOperation, NetOperationDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null))
                .ForMember(dest => dest.OperationTypeCode, opt => opt.MapFrom(src => src.OperationType != null ? src.OperationType.Code : null))
                .ForMember(dest => dest.OperationTypeName, opt => opt.MapFrom(src => src.OperationType != null ? src.OperationType.Name : null));
            CreateMap<CreateNetOperationDto, NetOperation>();
            CreateMap<UpdateNetOperationDto, NetOperation>();
        }
    }
}
