using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class SeaWaterTemperatureDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long ProjectCageId { get; set; }
        public long? CageId { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public DateTime RecordDate { get; set; }
        public decimal? WaterTemperatureCelsius { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class CreateSeaWaterTemperatureDto
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public DateTime RecordDate { get; set; }
        public decimal? WaterTemperatureCelsius { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class UpdateSeaWaterTemperatureDto : CreateSeaWaterTemperatureDto
    {
    }
}
