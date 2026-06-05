namespace aqua_api.Modules.BatchBalances.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBatchBalancesModule(this IServiceCollection services)
    {
        services.AddScoped<IBatchCageBalanceService, BatchCageBalanceService>();
        services.AddScoped<IBatchWarehouseBalanceService, BatchWarehouseBalanceService>();
        services.AddScoped<IBatchMovementService, BatchMovementService>();

        return services;
    }
}
