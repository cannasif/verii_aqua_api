using System;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class FishLabResult : BaseEntity
    {
        public long FishLabSampleId { get; set; }
        public DateTime ResultDate { get; set; }
        public string ResultType { get; set; } = string.Empty;
        public string? PathogenName { get; set; }
        public string? ResultValue { get; set; }
        public string? Unit { get; set; }
        public bool IsPositive { get; set; }
        public string? Interpretation { get; set; }
        public string? RecommendedAction { get; set; }

        public FishLabSample? FishLabSample { get; set; }
    }
}
