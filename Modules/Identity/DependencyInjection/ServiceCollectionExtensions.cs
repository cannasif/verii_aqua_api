
namespace aqua_api.Modules.Identity.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserAuthorityService, UserAuthorityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPermissionAccessService, PermissionAccessService>();
        services.AddScoped<IPermissionDefinitionService, PermissionDefinitionService>();
        services.AddScoped<IPermissionGroupService, PermissionGroupService>();
        services.AddScoped<IUserPermissionGroupService, UserPermissionGroupService>();
        services.AddScoped<IUserDetailService, UserDetailService>();

        return services;
    }
}
