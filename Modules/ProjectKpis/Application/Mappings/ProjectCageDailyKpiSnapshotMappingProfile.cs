using AutoMapper;

namespace aqua_api.Modules.ProjectKpis.Application.Mappings
{
    public class ProjectCageDailyKpiSnapshotMappingProfile : Profile
    {
        public ProjectCageDailyKpiSnapshotMappingProfile()
        {
            CreateMap<ProjectCageDailyKpiSnapshot, ProjectCageDailyKpiSnapshotDto>();
        }
    }
}
