using System;

namespace aqua_api.Modules.CurrentDirection.Application.Dtos
{
    public class CurrentDirectionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateCurrentDirectionDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCurrentDirectionDto : CreateCurrentDirectionDto
    {
    }

    public class CurrentDirectionMatchDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long ProjectCageId { get; set; }
        public long? CageId { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long CurrentDirectionId { get; set; }
        public string? CurrentDirectionName { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }
    }

    public class CreateCurrentDirectionMatchDto
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long CurrentDirectionId { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateCurrentDirectionMatchDto : CreateCurrentDirectionMatchDto
    {
    }
}
