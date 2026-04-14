using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ProjectMerge : BaseEntity
    {
        public long TargetProjectId { get; set; }
        public string TargetProjectCode { get; set; } = string.Empty;
        public string TargetProjectName { get; set; } = string.Empty;
        public DateTime MergeDate { get; set; }
        public string? Description { get; set; }
        public ProjectMergeSourceState SourceProjectStateAfterMerge { get; set; } = ProjectMergeSourceState.Archived;

        public Project? TargetProject { get; set; }
        public ICollection<ProjectMergeSource> SourceProjects { get; set; } = new List<ProjectMergeSource>();
        public ICollection<ProjectMergeCage> Cages { get; set; } = new List<ProjectMergeCage>();
    }
}
