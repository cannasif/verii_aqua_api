using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class MortalityDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public DateTime MortalityDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateMortalityDto
    {
        public long ProjectId { get; set; }
        public DateTime MortalityDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateMortalityDto : CreateMortalityDto
    {
    }
}
