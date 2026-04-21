
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(long id);
        Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterDto registerDto);
        Task<ApiResponse<string>> LoginAsync(LoginRequest request);
        Task<ApiResponse<UserDto>> GetUserByEmailOrUsernameAsync(string emailOrUsername);
        Task<ApiResponse<LoginWithSessionResponseDto>> LoginWithSessionAsync(LoginDto loginDto);
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<IEnumerable<UserDto>>> GetActiveUsersAsync();
        Task<ApiResponse<string>> RequestPasswordResetAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ApiResponse<LoginWithSessionResponseDto>> RefreshTokenAsync(RefreshTokenDto request);
    }

    public class LoginWithSessionResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public long UserId { get; set; }
        public Guid SessionId { get; set; }
        /// <summary>true: localStorage; false: sessionStorage (frontend token saklama)</summary>
        public bool RememberMe { get; set; }
    }
}
