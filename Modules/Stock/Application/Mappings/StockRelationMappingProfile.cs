using AutoMapper;

namespace aqua_api.Modules.Stock.Application.Mappings
{
    public class StockRelationMappingProfile : Profile
    {
        public StockRelationMappingProfile()
        {
            // StockRelation mappings
            CreateMap<StockRelation, StockRelationDto>()
                .ForMember(dest => dest.StockName, opt => opt.MapFrom(src => src.Stock != null ? src.Stock.StockName : null))
                .ForMember(dest => dest.RelatedStockCode, opt => opt.MapFrom(src => src.RelatedStock != null ? src.RelatedStock.ErpStockCode : null))
                .ForMember(dest => dest.RelatedStockName, opt => opt.MapFrom(src => src.RelatedStock != null ? src.RelatedStock.StockName : null))
                .ForMember(dest => dest.CreatedByFullUser, opt => opt.MapFrom(src => src.CreatedByUser != null ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}".Trim() : null))
                .ForMember(dest => dest.UpdatedByFullUser, opt => opt.MapFrom(src => src.UpdatedByUser != null ? $"{src.UpdatedByUser.FirstName} {src.UpdatedByUser.LastName}".Trim() : null))
                .ForMember(dest => dest.DeletedByFullUser, opt => opt.MapFrom(src => src.DeletedByUser != null ? $"{src.DeletedByUser.FirstName} {src.DeletedByUser.LastName}".Trim() : null));

            CreateMap<StockRelationCreateDto, StockRelation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedStock, opt => opt.Ignore());

            CreateMap<StockRelationUpdateDto, StockRelation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedStock, opt => opt.Ignore());

            CreateMap<StockRelationDto, StockRelation>();
        }
    }
}
