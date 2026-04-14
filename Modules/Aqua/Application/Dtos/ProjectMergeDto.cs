using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class ProjectMergeDto
    {
        public long Id { get; set; }
        public long TargetProjectId { get; set; }
        public string TargetProjectCode { get; set; } = string.Empty;
        public string TargetProjectName { get; set; } = string.Empty;
        public DateTime MergeDate { get; set; }
        public string? Description { get; set; }
        public ProjectMergeSourceState SourceProjectStateAfterMerge { get; set; }
        public List<ProjectMergeSourceDto> SourceProjects { get; set; } = new();
        public List<ProjectMergeCageDto> Cages { get; set; } = new();
    }

    public class ProjectMergeSourceDto
    {
        public long Id { get; set; }
        public long SourceProjectId { get; set; }
        public string SourceProjectCode { get; set; } = string.Empty;
        public string SourceProjectName { get; set; } = string.Empty;
    }

    public class ProjectMergeCageDto
    {
        public long Id { get; set; }
        public long SourceProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long CageId { get; set; }
        public string CageCode { get; set; } = string.Empty;
        public string CageName { get; set; } = string.Empty;
    }

    public class CreateProjectMergeDto
    {
        public long TargetProjectId { get; set; }
        public DateTime MergeDate { get; set; }
        public string? Description { get; set; }
        public ProjectMergeSourceState SourceProjectStateAfterMerge { get; set; } = ProjectMergeSourceState.Archived;
        public List<long> SourceProjectIds { get; set; } = new();
    }
}
