namespace aqua_api.Modules.BudgetPlanning.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBudgetPlanningModule(this IServiceCollection services)
    {
        services.AddScoped<IBudgetPlanningService, BudgetPlanningService>();
        return services;
    }
}
