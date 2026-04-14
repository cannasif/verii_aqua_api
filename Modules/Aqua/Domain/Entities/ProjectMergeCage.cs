namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class ProjectMergeCage : BaseEntity
    {
        public long ProjectMergeId { get; set; }
        public long SourceProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long CageId { get; set; }
        public string CageCode { get; set; } = string.Empty;
        public string CageName { get; set; } = string.Empty;

        public ProjectMerge? ProjectMerge { get; set; }
        public Project? SourceProject { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public Cage? Cage { get; set; }
    }
}
