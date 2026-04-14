
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IDailyWeatherService
    {
        Task<ApiResponse<DailyWeatherDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<DailyWeatherDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<DailyWeatherDto>> CreateAsync(CreateDailyWeatherDto dto);
        Task<ApiResponse<DailyWeatherDto>> UpdateAsync(long id, UpdateDailyWeatherDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<long>> CreateDaily(CreateDailyWeatherRequest request, long userId);
    }
}
