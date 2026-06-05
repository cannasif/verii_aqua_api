namespace aqua_api.Modules.Cages.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCageModule(this IServiceCollection services)
    {
        services.AddScoped<ICageService, CageService>();
        services.AddScoped<ICageWarehouseMappingService, CageWarehouseMappingService>();

        return services;
    }
}
