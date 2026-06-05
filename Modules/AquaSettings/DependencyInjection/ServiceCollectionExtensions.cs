namespace aqua_api.Modules.AquaSettings.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAquaSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<IAquaSettingsService, AquaSettingsService>();

        return services;
    }
}
