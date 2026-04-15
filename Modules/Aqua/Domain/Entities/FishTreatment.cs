using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishTreatment : BaseEntity
    {
        public long ProjectId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public long? FishHealthEventId { get; set; }
        public DateTime TreatmentDate { get; set; }
        public string TreatmentType { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string? ActiveIngredient { get; set; }
        public decimal? DoseValue { get; set; }
        public string? DoseUnit { get; set; }
        public int? DurationDays { get; set; }
        public DateTime? WithdrawalEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? VeterinarianName { get; set; }
        public string? TreatmentReason { get; set; }
        public string? Note { get; set; }

        public Project? Project { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public FishBatch? FishBatch { get; set; }
        public FishHealthEvent? FishHealthEvent { get; set; }
    }
}
