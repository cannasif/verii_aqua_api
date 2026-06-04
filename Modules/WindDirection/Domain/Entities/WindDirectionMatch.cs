using System;

namespace aqua_api.Modules.WindDirection.Domain.Entities
{
    public class WindDirectionMatch : BaseEntity
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long WindDirectionId { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public WindDirection? WindDirection { get; set; }
    }
}
