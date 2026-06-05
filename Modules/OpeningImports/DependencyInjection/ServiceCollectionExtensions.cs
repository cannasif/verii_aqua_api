namespace aqua_api.Modules.OpeningImports.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpeningImportsModule(this IServiceCollection services)
    {
        services.AddScoped<IOpeningImportService, OpeningImportService>();

        return services;
    }
}
