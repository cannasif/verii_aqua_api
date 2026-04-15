using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class ProjectCageDailyKpiSnapshotMappingProfile : Profile
    {
        public ProjectCageDailyKpiSnapshotMappingProfile()
        {
            CreateMap<ProjectCageDailyKpiSnapshot, ProjectCageDailyKpiSnapshotDto>();
        }
    }
}
