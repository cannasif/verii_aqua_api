using AutoMapper;

namespace aqua_api.Modules.Cages.Application.Mappings
{
    public class CageMappingProfile : Profile
    {
        public CageMappingProfile()
        {
            CreateMap<Cage, CageDto>();
            CreateMap<CreateCageDto, Cage>();
            CreateMap<UpdateCageDto, Cage>();
        }
    }
}
