
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IPermissionGroupService
    {
        Task<ApiResponse<PagedResponse<PermissionGroupDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<PermissionGroupDto>> GetByIdAsync(long id);
        Task<ApiResponse<PermissionGroupDto>> CreateAsync(CreatePermissionGroupDto dto);
        Task<ApiResponse<PermissionGroupDto>> UpdateAsync(long id, UpdatePermissionGroupDto dto);
        Task<ApiResponse<PermissionGroupDto>> SetPermissionsAsync(long id, SetPermissionGroupPermissionsDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
