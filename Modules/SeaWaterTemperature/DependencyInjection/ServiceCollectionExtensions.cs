namespace aqua_api.Modules.SeaWaterTemperature.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeaWaterTemperatureModule(this IServiceCollection services)
    {
        services.AddScoped<ISeaWaterTemperatureService, SeaWaterTemperatureService>();
        return services;
    }
}
