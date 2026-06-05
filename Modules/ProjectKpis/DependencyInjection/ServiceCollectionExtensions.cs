namespace aqua_api.Modules.ProjectKpis.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectKpisModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectCageDailyKpiService, ProjectCageDailyKpiService>();

        return services;
    }
}
