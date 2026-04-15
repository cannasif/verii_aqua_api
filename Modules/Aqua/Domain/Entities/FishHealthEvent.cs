using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishHealthEvent : BaseEntity
    {
        public long ProjectId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? AffectedFishCount { get; set; }
        public decimal? AffectedRatioPct { get; set; }
        public int? MortalityCount { get; set; }
        public bool IsConfirmed { get; set; }
        public bool RequiresVeterinaryReview { get; set; }
        public string? VeterinarianName { get; set; }
        public string? Observation { get; set; }
        public string? RecommendedAction { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
