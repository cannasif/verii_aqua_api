using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class NetOperationLineDto
    {
        public long Id { get; set; }
        public long NetOperationId { get; set; }
        public long ProjectCageId { get; set; }
        public long? FishBatchId { get; set; }
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
}
