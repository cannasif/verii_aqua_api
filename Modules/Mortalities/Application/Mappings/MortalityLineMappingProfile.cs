using AutoMapper;

namespace aqua_api.Modules.Mortalities.Application.Mappings
{
    public class MortalityLineMappingProfile : Profile
    {
        public MortalityLineMappingProfile()
        {
            CreateMap<MortalityLine, MortalityLineDto>();
            CreateMap<CreateMortalityLineDto, MortalityLine>();
            CreateMap<UpdateMortalityLineDto, MortalityLine>();
        }
    }
}
