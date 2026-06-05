namespace aqua_api.Modules.Weighings.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeighingsModule(this IServiceCollection services)
    {
        services.AddScoped<IWeighingService, WeighingService>();
        services.AddScoped<IWeighingLineService, WeighingLineService>();

        return services;
    }
}
