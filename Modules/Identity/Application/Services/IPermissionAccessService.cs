
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IPermissionAccessService
    {
        Task<ApiResponse<MyPermissionsDto>> GetMyPermissionsAsync();
    }
}
