using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
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
