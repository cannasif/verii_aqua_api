using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class StockConvertLineMappingProfile : Profile
    {
        public StockConvertLineMappingProfile()
        {
            CreateMap<StockConvertLine, StockConvertLineDto>();
            CreateMap<CreateStockConvertLineDto, StockConvertLine>();
            CreateMap<UpdateStockConvertLineDto, StockConvertLine>();
        }
    }
}
