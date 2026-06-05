namespace aqua_api.Modules.AquaReports.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAquaReportsModule(this IServiceCollection services)
    {
        services.AddScoped<IDashboardProjectReportService, DashboardProjectReportService>();
        services.AddScoped<IDevirFcrReportService, DevirFcrReportService>();

        return services;
    }
}
