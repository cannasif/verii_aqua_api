using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WeighingLineDto
    {
        public long Id { get; set; }
        public long WeighingId { get; set; }
        public long FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public long ProjectCageId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public int MeasuredCount { get; set; }
        public decimal MeasuredAverageGram { get; set; }
        public decimal MeasuredBiomassGram { get; set; }
    }

    public class CreateWeighingLineDto
    {
        public long WeighingId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int MeasuredCount { get; set; }
        public decimal MeasuredAverageGram { get; set; }
        public decimal MeasuredBiomassGram { get; set; }
    }

    public class UpdateWeighingLineDto : CreateWeighingLineDto
    {
    }

    public class CreateWeighingLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime WeighingDate { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int MeasuredCount { get; set; }
        public decimal MeasuredAverageGram { get; set; }
        public decimal MeasuredBiomassGram { get; set; }
    }
}
