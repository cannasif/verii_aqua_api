namespace aqua_api.Modules.StockConverts.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStockConvertsModule(this IServiceCollection services)
    {
        services.AddScoped<IStockConvertService, StockConvertService>();
        services.AddScoped<IStockConvertLineService, StockConvertLineService>();

        return services;
    }
}
