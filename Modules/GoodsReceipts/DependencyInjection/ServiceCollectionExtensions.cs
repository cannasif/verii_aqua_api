namespace aqua_api.Modules.GoodsReceipts.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoodsReceiptsModule(this IServiceCollection services)
    {
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<IGoodsReceiptLineService, GoodsReceiptLineService>();
        services.AddScoped<IGoodsReceiptFishDistributionService, GoodsReceiptFishDistributionService>();

        return services;
    }
}
