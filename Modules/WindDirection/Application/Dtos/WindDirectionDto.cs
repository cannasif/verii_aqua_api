using System;

namespace aqua_api.Modules.WindDirection.Application.Dtos
{
    public class WindDirectionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateWindDirectionDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateWindDirectionDto : CreateWindDirectionDto
    {
    }

    public class WindDirectionMatchDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long ProjectCageId { get; set; }
        public long? CageId { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long WindDirectionId { get; set; }
        public string? WindDirectionName { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }
    }

    public class CreateWindDirectionMatchDto
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public long WindDirectionId { get; set; }
        public DateTime RecordDate { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateWindDirectionMatchDto : CreateWindDirectionMatchDto
    {
    }
}
