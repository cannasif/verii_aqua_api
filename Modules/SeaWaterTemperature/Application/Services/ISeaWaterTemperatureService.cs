namespace aqua_api.Modules.SeaWaterTemperature.Application.Services
{
    public interface ISeaWaterTemperatureService
    {
        Task<ApiResponse<SeaWaterTemperatureDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<SeaWaterTemperatureDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<SeaWaterTemperatureDto>> CreateAsync(CreateSeaWaterTemperatureDto dto);
        Task<ApiResponse<SeaWaterTemperatureDto>> UpdateAsync(long id, UpdateSeaWaterTemperatureDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
