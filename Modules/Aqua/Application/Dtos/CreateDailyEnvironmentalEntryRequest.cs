using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class CreateDailyEnvironmentalEntryRequest
    {
        public long ProjectId { get; set; }
        public long ProjectCageId { get; set; }
        public DateTime Date { get; set; }
        public long TypeId { get; set; }
        public long SeverityId { get; set; }
        public decimal? WaterTemperatureCelsius { get; set; }
        public string? Description { get; set; }
        public long WindDirectionId { get; set; }
        public long CurrentDirectionId { get; set; }
    }

    public class DailyEnvironmentalEntryResultDto
    {
        public long DailyWeatherId { get; set; }
        public long SeaWaterTemperatureId { get; set; }
        public long WindDirectionMatchId { get; set; }
        public long CurrentDirectionMatchId { get; set; }
    }
}
