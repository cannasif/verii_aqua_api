using AutoMapper;

namespace aqua_api.Modules.GoodsReceipts.Application.Mappings
{
    public class GoodsReceiptFishDistributionMappingProfile : Profile
    {
        public GoodsReceiptFishDistributionMappingProfile()
        {
            CreateMap<GoodsReceiptFishDistribution, GoodsReceiptFishDistributionDto>();
            CreateMap<CreateGoodsReceiptFishDistributionDto, GoodsReceiptFishDistribution>();
            CreateMap<UpdateGoodsReceiptFishDistributionDto, GoodsReceiptFishDistribution>();
        }
    }
}
