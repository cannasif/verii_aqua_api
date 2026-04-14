
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IUserDetailService
    {
        Task<ApiResponse<UserDetailDto>> GetByIdAsync(long id);
        Task<ApiResponse<UserDetailDto>> GetByUserIdAsync(long userId);
        Task<ApiResponse<IEnumerable<UserDetailDto>>> GetAllAsync();
        Task<ApiResponse<PagedResponse<UserDetailDto>>> GetPagedAsync(PagedRequest request);
        Task<ApiResponse<UserDetailDto>> CreateAsync(CreateUserDetailDto dto);
        Task<ApiResponse<UserDetailDto>> UpdateAsync(long id, UpdateUserDetailDto dto);
        Task<ApiResponse<bool>> DeleteAsync(long id);
        Task<ApiResponse<UserDetailDto>> UploadProfilePictureAsync(long userId, Microsoft.AspNetCore.Http.IFormFile file);
    }
}
