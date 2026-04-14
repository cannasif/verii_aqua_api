using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
