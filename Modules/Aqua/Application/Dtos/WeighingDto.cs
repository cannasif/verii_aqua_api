using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class WeighingDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string WeighingNo { get; set; } = string.Empty;
        public DateTime WeighingDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateWeighingDto
    {
        public long ProjectId { get; set; }
        public string WeighingNo { get; set; } = string.Empty;
        public DateTime WeighingDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateWeighingDto : CreateWeighingDto
    {
    }
}
