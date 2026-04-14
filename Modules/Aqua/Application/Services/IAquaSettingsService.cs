
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IAquaSettingsService
    {
        Task<ApiResponse<AquaSettingsDto>> GetAsync();
        Task<ApiResponse<AquaSettingsDto>> UpdateAsync(UpdateAquaSettingsDto dto, long userId);
    }
}
