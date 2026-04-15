namespace aqua_api.Modules.Warehouse.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services)
    {
        services.AddScoped<IWarehouseService, WarehouseService>();
        return services;
    }
}
