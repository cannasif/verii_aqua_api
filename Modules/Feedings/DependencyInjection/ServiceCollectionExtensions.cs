namespace aqua_api.Modules.Feedings.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeedingsModule(this IServiceCollection services)
    {
        services.AddScoped<IFeedingService, FeedingService>();
        services.AddScoped<IFeedingLineService, FeedingLineService>();
        services.AddScoped<IFeedingDistributionService, FeedingDistributionService>();

        return services;
    }
}
