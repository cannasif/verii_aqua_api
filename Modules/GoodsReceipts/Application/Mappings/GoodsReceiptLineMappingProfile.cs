using AutoMapper;

namespace aqua_api.Modules.GoodsReceipts.Application.Mappings
{
    public class GoodsReceiptLineMappingProfile : Profile
    {
        public GoodsReceiptLineMappingProfile()
        {
            CreateMap<GoodsReceiptLine, GoodsReceiptLineDto>();
            CreateMap<CreateGoodsReceiptLineDto, GoodsReceiptLine>();
            CreateMap<UpdateGoodsReceiptLineDto, GoodsReceiptLine>();
        }
    }
}
