using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class NetOperationTypeMappingProfile : Profile
    {
        public NetOperationTypeMappingProfile()
        {
            CreateMap<NetOperationType, NetOperationTypeDto>();
            CreateMap<CreateNetOperationTypeDto, NetOperationType>();
            CreateMap<UpdateNetOperationTypeDto, NetOperationType>();
        }
    }
}
