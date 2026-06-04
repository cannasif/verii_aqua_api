namespace aqua_api.Modules.CurrentDirection.Domain.Entities
{
    public class CurrentDirectionMatch : BaseEntity
    {
        public long ProjectId { get; set; }
        public Project? Project { get; set; }

        public long ProjectCageId { get; set; }
        public ProjectCage? ProjectCage { get; set; }

        public long CurrentDirectionId { get; set; }
        public CurrentDirection? CurrentDirection { get; set; }

        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }
    }
}
