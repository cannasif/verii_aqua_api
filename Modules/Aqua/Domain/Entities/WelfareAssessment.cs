using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class WelfareAssessment : BaseEntity
    {
        public long ProjectId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public decimal WelfareScore { get; set; }
        public decimal? StockingDensityKgM3 { get; set; }
        public decimal? AppetiteScore { get; set; }
        public decimal? BehaviorScore { get; set; }
        public decimal? GillScore { get; set; }
        public decimal? SkinScore { get; set; }
        public decimal? FinScore { get; set; }
        public string? AssessedBy { get; set; }
        public string? Observation { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
    }
}
