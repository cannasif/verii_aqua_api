using System;

namespace aqua_api.Modules.NetOperations.Application.Dtos
{
    public class NetOperationTypeDto : AuditDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CreateNetOperationTypeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateNetOperationTypeDto : CreateNetOperationTypeDto
    {
    }
}
