using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class CageDto
    {
        public long Id { get; set; }
        public string CageCode { get; set; } = string.Empty;
        public string CageName { get; set; } = string.Empty;
        public int? CapacityCount { get; set; }
        public decimal? CapacityGram { get; set; }
    }

    public class CreateCageDto
    {
        public string CageCode { get; set; } = string.Empty;
        public string CageName { get; set; } = string.Empty;
        public int? CapacityCount { get; set; }
        public decimal? CapacityGram { get; set; }
    }

    public class UpdateCageDto : CreateCageDto
    {
    }
}
