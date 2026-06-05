using AutoMapper;

namespace aqua_api.Modules.NetOperations.Application.Mappings
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
