
namespace aqua_api.Modules.Stock.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStockModule(this IServiceCollection services)
    {
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockDetailService, StockDetailService>();
        services.AddScoped<IStockImageService, StockImageService>();
        services.AddScoped<IStockRelationService, StockRelationService>();

        return services;
    }
}
