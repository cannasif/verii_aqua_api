using System.Security.Claims;

namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IUserService
    {
        Task<ApiResponse<long>> GetCurrentUserIdAsync();
        Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(PagedRequest request);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(long id);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto);
        Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserDto dto);
        Task<ApiResponse<object>> DeleteUserAsync(long id);
        Task<ApiResponse<UserDto>> GetUserProfileAsync(string userId);
    }
}
