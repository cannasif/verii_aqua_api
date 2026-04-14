
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWeatherSeverityService
    {
        Task<ApiResponse<WeatherSeverityDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WeatherSeverityDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WeatherSeverityDto>> CreateAsync(CreateWeatherSeverityDto dto);
        Task<ApiResponse<WeatherSeverityDto>> UpdateAsync(long id, UpdateWeatherSeverityDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
