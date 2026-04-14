
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWeatherTypeService
    {
        Task<ApiResponse<WeatherTypeDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WeatherTypeDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WeatherTypeDto>> CreateAsync(CreateWeatherTypeDto dto);
        Task<ApiResponse<WeatherTypeDto>> UpdateAsync(long id, UpdateWeatherTypeDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
