namespace aqua_api.Modules.Shipments.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShipmentsModule(this IServiceCollection services)
    {
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IShipmentLineService, ShipmentLineService>();

        return services;
    }
}
