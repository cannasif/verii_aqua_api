namespace aqua_api.Modules.NetInventory.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<INetInventoryMovementService, NetInventoryMovementService>();
        return services;
    }
}
