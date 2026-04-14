using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class NetOperationDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long OperationTypeId { get; set; }
        public string? OperationTypeCode { get; set; }
        public string? OperationTypeName { get; set; }
        public string OperationNo { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateNetOperationDto
    {
        public long ProjectId { get; set; }
        public long OperationTypeId { get; set; }
        public string OperationNo { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateNetOperationDto : CreateNetOperationDto
    {
    }
}
