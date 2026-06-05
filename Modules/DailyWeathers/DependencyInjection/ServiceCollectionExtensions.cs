namespace aqua_api.Modules.DailyWeathers.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDailyWeathersModule(this IServiceCollection services)
    {
        services.AddScoped<IDailyWeatherService, DailyWeatherService>();
        services.AddScoped<IDailyEnvironmentalEntryService, DailyEnvironmentalEntryService>();

        return services;
    }
}
