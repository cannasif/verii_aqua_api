using AutoMapper;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Stock.Application.Mappings
{
    public class StockMappingProfile : Profile
    {
        public StockMappingProfile()
        {
            // Stock mappings
            CreateMap<StockEntity, StockGetDto>()
                .ForMember(dest => dest.StockDetail, opt => opt.MapFrom(src => src.StockDetail != null && !src.StockDetail.IsDeleted ? src.StockDetail : null))
                .ForMember(dest => dest.StockImages, opt => opt.MapFrom(src => src.StockImages != null ? src.StockImages.Where(i => !i.IsDeleted).ToList() : null))
                .ForMember(dest => dest.ParentRelations, opt => opt.MapFrom(src => src.ParentRelations != null ? src.ParentRelations.Where(r => !r.IsDeleted).ToList() : null))
                .ForMember(dest => dest.CreatedByFullUser, opt => opt.MapFrom(src => src.CreatedByUser != null ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}".Trim() : null))
                .ForMember(dest => dest.UpdatedByFullUser, opt => opt.MapFrom(src => src.UpdatedByUser != null ? $"{src.UpdatedByUser.FirstName} {src.UpdatedByUser.LastName}".Trim() : null))
                .ForMember(dest => dest.DeletedByFullUser, opt => opt.MapFrom(src => src.DeletedByUser != null ? $"{src.DeletedByUser.FirstName} {src.DeletedByUser.LastName}".Trim() : null));

            CreateMap<StockCreateDto, StockEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.StockDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StockImages, opt => opt.Ignore())
                .ForMember(dest => dest.ParentRelations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUser, opt => opt.Ignore());

            CreateMap<StockUpdateDto, StockEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.StockDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StockImages, opt => opt.Ignore())
                .ForMember(dest => dest.ParentRelations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUser, opt => opt.Ignore());

            // StockGetDto to StockGetWithMainImageDto mapping
            CreateMap<StockGetDto, StockGetWithMainImageDto>()
                .ForMember(dest => dest.MainImage, opt => opt.Ignore());
        }
    }
}
