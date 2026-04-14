using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
