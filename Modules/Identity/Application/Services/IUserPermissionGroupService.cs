
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IUserPermissionGroupService
    {
        Task<ApiResponse<UserPermissionGroupDto>> GetByUserIdAsync(long userId);
        Task<ApiResponse<UserPermissionGroupDto>> SetUserGroupsAsync(long userId, SetUserPermissionGroupsDto dto);
    }
}
