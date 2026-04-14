using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class GoodsReceiptMappingProfile : Profile
    {
        public GoodsReceiptMappingProfile()
        {
            CreateMap<GoodsReceipt, GoodsReceiptDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null));
            CreateMap<CreateGoodsReceiptDto, GoodsReceipt>();
            CreateMap<UpdateGoodsReceiptDto, GoodsReceipt>();
        }
    }
}
