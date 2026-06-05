namespace aqua_api.Modules.ProjectMerges.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectMergesModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectMergeService, ProjectMergeService>();

        return services;
    }
}
