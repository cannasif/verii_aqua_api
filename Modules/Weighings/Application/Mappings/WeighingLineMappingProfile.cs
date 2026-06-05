using AutoMapper;

namespace aqua_api.Modules.Weighings.Application.Mappings
{
    public class WeighingLineMappingProfile : Profile
    {
        public WeighingLineMappingProfile()
        {
            CreateMap<WeighingLine, WeighingLineDto>();
            CreateMap<CreateWeighingLineDto, WeighingLine>();
            CreateMap<UpdateWeighingLineDto, WeighingLine>();
        }
    }
}
