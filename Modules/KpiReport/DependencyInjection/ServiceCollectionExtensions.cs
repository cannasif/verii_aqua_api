using aqua_api.Modules.KpiReport.Application.Services;

namespace aqua_api.Modules.KpiReport.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKpiReportModule(this IServiceCollection services)
    {
        services.AddScoped<IKpiReportService, KpiReportService>();
        return services;
    }
}
