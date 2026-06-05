
namespace aqua_api.Modules.Aqua.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAquaModule(this IServiceCollection services)
    {
        services.AddScoped<IBalanceLedgerManager, BalanceLedgerManager>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IMortalityRepository, MortalityRepository>();
        services.AddScoped<IWeighingRepository, WeighingRepository>();
        services.AddScoped<IStockConvertRepository, StockConvertRepository>();
        services.AddScoped<INetOperationRepository, NetOperationRepository>();
        services.AddScoped<IDailyWeatherRepository, DailyWeatherRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        return services;
    }
}
