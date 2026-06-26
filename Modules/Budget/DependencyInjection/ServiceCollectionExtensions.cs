namespace aqua_api.Modules.Budget.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBudgetModule(this IServiceCollection services)
    {
        services.AddScoped<IBudgetWaterTemperatureService, BudgetWaterTemperatureService>();
        return services;
    }
}
