namespace aqua_api.Modules.FishBatches.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFishBatchModule(this IServiceCollection services)
    {
        services.AddScoped<IFishBatchService, FishBatchService>();

        return services;
    }
}
