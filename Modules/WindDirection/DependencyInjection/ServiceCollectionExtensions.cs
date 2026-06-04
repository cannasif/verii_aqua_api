namespace aqua_api.Modules.WindDirection.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindDirectionModule(this IServiceCollection services)
    {
        services.AddScoped<IWindDirectionService, WindDirectionService>();
        services.AddScoped<IWindDirectionMatchService, WindDirectionMatchService>();
        return services;
    }
}
