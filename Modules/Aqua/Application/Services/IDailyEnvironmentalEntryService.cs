namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IDailyEnvironmentalEntryService
    {
        Task<ApiResponse<DailyEnvironmentalEntryResultDto>> CreateAsync(CreateDailyEnvironmentalEntryRequest request, long userId);
    }
}
