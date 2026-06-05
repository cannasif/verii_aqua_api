namespace aqua_api.Modules.Mortalities.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMortalitiesModule(this IServiceCollection services)
    {
        services.AddScoped<IMortalityService, MortalityService>();
        services.AddScoped<IMortalityLineService, MortalityLineService>();

        return services;
    }
}
