using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class NetOperationTypeDto
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
