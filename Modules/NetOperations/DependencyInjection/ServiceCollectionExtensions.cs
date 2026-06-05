namespace aqua_api.Modules.NetOperations.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetOperationsModule(this IServiceCollection services)
    {
        services.AddScoped<INetOperationService, NetOperationService>();
        services.AddScoped<INetOperationLineService, NetOperationLineService>();
        services.AddScoped<INetOperationTypeService, NetOperationTypeService>();

        return services;
    }
}
