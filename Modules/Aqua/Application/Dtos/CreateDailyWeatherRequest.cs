namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class CreateDailyWeatherRequest
    {
        public long ProjectId { get; set; }
        public DateTime Date { get; set; }
        public long SeverityId { get; set; }
        public long TypeId { get; set; }
        public string? Description { get; set; }
    }
}
