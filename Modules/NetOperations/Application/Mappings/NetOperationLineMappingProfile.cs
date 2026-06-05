using AutoMapper;

namespace aqua_api.Modules.NetOperations.Application.Mappings
{
    public class NetOperationLineMappingProfile : Profile
    {
        public NetOperationLineMappingProfile()
        {
            CreateMap<NetOperationLine, NetOperationLineDto>();
            CreateMap<CreateNetOperationLineDto, NetOperationLine>();
            CreateMap<UpdateNetOperationLineDto, NetOperationLine>();
        }
    }
}
