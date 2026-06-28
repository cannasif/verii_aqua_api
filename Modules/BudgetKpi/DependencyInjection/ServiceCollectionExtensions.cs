namespace aqua_api.Modules.BudgetKpi.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBudgetKpiModule(this IServiceCollection services)
    {
        services.AddScoped<IBudgetKpiService, BudgetKpiService>();
        return services;
    }
}
