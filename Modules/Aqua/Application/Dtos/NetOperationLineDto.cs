using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class NetOperationLineDto
    {
        public long Id { get; set; }
        public long NetOperationId { get; set; }
        public long ProjectCageId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long? FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public string? Note { get; set; }
    }

    public class CreateNetOperationLineDto
    {
        public long NetOperationId { get; set; }
        public long ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateNetOperationLineDto : CreateNetOperationLineDto
    {
    }

    public class CreateNetOperationLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime OperationDate { get; set; }
        public long OperationTypeId { get; set; }
        public long ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
        public string? Note { get; set; }
    }
}
