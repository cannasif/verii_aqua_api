namespace aqua_api.Modules.Transfers.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransfersModule(this IServiceCollection services)
    {
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransferLineService, TransferLineService>();
        services.AddScoped<IWarehouseTransferService, WarehouseTransferService>();
        services.AddScoped<IWarehouseTransferLineService, WarehouseTransferLineService>();
        services.AddScoped<ICageWarehouseTransferService, CageWarehouseTransferService>();
        services.AddScoped<ICageWarehouseTransferLineService, CageWarehouseTransferLineService>();
        services.AddScoped<IWarehouseCageTransferService, WarehouseCageTransferService>();
        services.AddScoped<IWarehouseCageTransferLineService, WarehouseCageTransferLineService>();

        return services;
    }
}
