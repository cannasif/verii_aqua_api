
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
        services.AddScoped<IBatchCageBalanceService, BatchCageBalanceService>();
        services.AddScoped<IBatchMovementService, BatchMovementService>();
        services.AddScoped<IAquaSettingsService, AquaSettingsService>();
        services.AddScoped<ICageService, CageService>();
        services.AddScoped<IDailyWeatherService, DailyWeatherService>();
        services.AddScoped<IFeedingService, FeedingService>();
        services.AddScoped<IFeedingDistributionService, FeedingDistributionService>();
        services.AddScoped<IFeedingLineService, FeedingLineService>();
        services.AddScoped<IFishBatchService, FishBatchService>();
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<IGoodsReceiptFishDistributionService, GoodsReceiptFishDistributionService>();
        services.AddScoped<IGoodsReceiptLineService, GoodsReceiptLineService>();
        services.AddScoped<IMortalityService, MortalityService>();
        services.AddScoped<IMortalityLineService, MortalityLineService>();
        services.AddScoped<INetOperationService, NetOperationService>();
        services.AddScoped<INetOperationLineService, NetOperationLineService>();
        services.AddScoped<INetOperationTypeService, NetOperationTypeService>();
        services.AddScoped<IProjectMergeService, ProjectMergeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectCageService, ProjectCageService>();
        services.AddScoped<IStockConvertService, StockConvertService>();
        services.AddScoped<IStockConvertLineService, StockConvertLineService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransferLineService, TransferLineService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IShipmentLineService, ShipmentLineService>();
        services.AddScoped<IWeatherSeverityService, WeatherSeverityService>();
        services.AddScoped<IWeatherTypeService, WeatherTypeService>();
        services.AddScoped<IWeighingService, WeighingService>();
        services.AddScoped<IWeighingLineService, WeighingLineService>();

        return services;
    }
}
