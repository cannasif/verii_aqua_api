namespace aqua_api.Modules.Weather.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherModule(this IServiceCollection services)
    {
        services.AddScoped<IWeatherSeverityService, WeatherSeverityService>();
        services.AddScoped<IWeatherTypeService, WeatherTypeService>();

        return services;
    }
}
