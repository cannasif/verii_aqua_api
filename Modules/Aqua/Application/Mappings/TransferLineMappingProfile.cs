using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class TransferLineMappingProfile : Profile
    {
        public TransferLineMappingProfile()
        {
            CreateMap<TransferLine, TransferLineDto>();
            CreateMap<CreateTransferLineDto, TransferLine>();
            CreateMap<UpdateTransferLineDto, TransferLine>();
        }
    }
}
