
namespace aqua_api.Modules.Identity.Application.Services
{
    public interface IJwtService
    {
        ApiResponse<string> GenerateToken(User user);
    }
}
