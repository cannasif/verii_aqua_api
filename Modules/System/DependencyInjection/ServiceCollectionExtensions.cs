using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Modules.System.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemModule(this IServiceCollection services)
    {
        services.AddScoped<IStockSyncJob, StockSyncJob>();
        services.AddScoped<IWarehouseSyncJob, WarehouseSyncJob>();
        services.AddScoped<IMailJob, MailJob>();
        services.AddScoped<IHangfireDeadLetterJob, HangfireDeadLetterJob>();

        return services;
    }
}
