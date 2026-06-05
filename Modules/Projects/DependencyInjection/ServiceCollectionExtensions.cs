namespace aqua_api.Modules.Projects.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectCageService, ProjectCageService>();

        return services;
    }
}
