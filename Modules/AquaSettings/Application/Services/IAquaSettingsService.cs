
namespace aqua_api.Modules.AquaSettings.Application.Services
{
    public interface IAquaSettingsService
    {
        Task<ApiResponse<AquaSettingsDto>> GetAsync();
        Task<ApiResponse<AquaSettingsDto>> UpdateAsync(UpdateAquaSettingsDto dto, long userId);
    }
}
