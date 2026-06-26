using aqua_api.Modules.Aqua.DependencyInjection;
using aqua_api.Modules.AquaReports.DependencyInjection;
using aqua_api.Modules.AquaSettings.DependencyInjection;
using aqua_api.Modules.BatchBalances.DependencyInjection;
using aqua_api.Modules.Budget.DependencyInjection;
using aqua_api.Modules.Cages.DependencyInjection;
using aqua_api.Modules.CurrentDirection.DependencyInjection;
using aqua_api.Modules.DailyWeathers.DependencyInjection;
using aqua_api.Modules.Feedings.DependencyInjection;
using aqua_api.Modules.FishBatches.DependencyInjection;
using aqua_api.Modules.GoodsReceipts.DependencyInjection;
using aqua_api.Modules.Identity.DependencyInjection;
using aqua_api.Modules.Integrations.DependencyInjection;
using aqua_api.Modules.KpiReport.DependencyInjection;
using aqua_api.Modules.Mortalities.DependencyInjection;
using aqua_api.Modules.NetInventory.DependencyInjection;
using aqua_api.Modules.NetOperations.DependencyInjection;
using aqua_api.Modules.OpeningImports.DependencyInjection;
using aqua_api.Modules.ProjectKpis.DependencyInjection;
using aqua_api.Modules.ProjectMerges.DependencyInjection;
using aqua_api.Modules.Projects.DependencyInjection;
using aqua_api.Modules.SeaWaterTemperature.DependencyInjection;
using aqua_api.Modules.Shipments.DependencyInjection;
using aqua_api.Modules.Stock.DependencyInjection;
using aqua_api.Modules.StockConverts.DependencyInjection;
using aqua_api.Modules.System.DependencyInjection;
using aqua_api.Modules.Transfers.DependencyInjection;
using aqua_api.Modules.Warehouse.DependencyInjection;
using aqua_api.Modules.Weather.DependencyInjection;
using aqua_api.Modules.Weighings.DependencyInjection;
using aqua_api.Modules.WindDirection.DependencyInjection;
using aqua_api.Shared.Infrastructure.DependencyInjection;

namespace aqua_api.Shared.Host.WebApi.Extensions;

public static class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAquaApplicationModules(this IServiceCollection services)
    {
        services.AddAquaSharedInfrastructure();
        services.AddIdentityModule();
        services.AddStockModule();
        services.AddWarehouseModule();
        services.AddAquaSettingsModule();
        services.AddProjectModule();
        services.AddCageModule();
        services.AddFishBatchModule();
        services.AddWeatherModule();
        services.AddDailyWeathersModule();
        services.AddFeedingsModule();
        services.AddGoodsReceiptsModule();
        services.AddMortalitiesModule();
        services.AddNetOperationsModule();
        services.AddNetInventoryModule();
        services.AddStockConvertsModule();
        services.AddTransfersModule();
        services.AddShipmentsModule();
        services.AddWeighingsModule();
        services.AddBatchBalancesModule();
        services.AddBudgetModule();
        services.AddOpeningImportsModule();
        services.AddProjectMergesModule();
        services.AddProjectKpisModule();
        services.AddAquaReportsModule();
        services.AddAquaModule();
        services.AddKpiReportModule();
        services.AddSeaWaterTemperatureModule();
        services.AddCurrentDirectionModule();
        services.AddWindDirectionModule();
        services.AddIntegrationsModule();
        services.AddSystemModule();

        return services;
    }
}
