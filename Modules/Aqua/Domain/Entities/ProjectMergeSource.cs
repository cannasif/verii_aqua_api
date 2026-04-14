namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ProjectMergeSource : BaseEntity
    {
        public long ProjectMergeId { get; set; }
        public long SourceProjectId { get; set; }
        public string SourceProjectCode { get; set; } = string.Empty;
        public string SourceProjectName { get; set; } = string.Empty;

        public ProjectMerge? ProjectMerge { get; set; }
        public Project? SourceProject { get; set; }
    }
}
