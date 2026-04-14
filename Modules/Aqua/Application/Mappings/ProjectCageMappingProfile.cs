using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class ProjectCageMappingProfile : Profile
    {
        public ProjectCageMappingProfile()
        {
            CreateMap<ProjectCage, ProjectCageDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null))
                .ForMember(dest => dest.CageCode, opt => opt.MapFrom(src => src.Cage != null ? src.Cage.CageCode : null))
                .ForMember(dest => dest.CageName, opt => opt.MapFrom(src => src.Cage != null ? src.Cage.CageName : null));
            CreateMap<CreateProjectCageDto, ProjectCage>();
            CreateMap<UpdateProjectCageDto, ProjectCage>();
        }
    }
}
