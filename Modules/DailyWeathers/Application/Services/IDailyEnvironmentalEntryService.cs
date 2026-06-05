namespace aqua_api.Modules.DailyWeathers.Application.Services
{
    public interface IDailyEnvironmentalEntryService
    {
        Task<ApiResponse<DailyEnvironmentalEntryResultDto>> CreateAsync(CreateDailyEnvironmentalEntryRequest request, long userId);
    }
}
