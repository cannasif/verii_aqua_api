namespace aqua_api.Modules.FishGrowths.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFishGrowthsModule(this IServiceCollection services)
    {
        services.AddScoped<IFishGrowthLedgerReplayService, FishGrowthLedgerReplayService>();
        services.AddScoped<IFishGrowthService, FishGrowthService>();
        return services;
    }
}
