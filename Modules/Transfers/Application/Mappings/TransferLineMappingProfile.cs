using AutoMapper;

namespace aqua_api.Modules.Transfers.Application.Mappings
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
