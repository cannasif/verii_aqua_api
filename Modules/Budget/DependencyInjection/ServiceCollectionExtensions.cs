namespace aqua_api.Modules.Budget.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBudgetModule(this IServiceCollection services)
    {
        services.AddScoped<IBudgetWaterTemperatureService, BudgetWaterTemperatureService>();
        services.AddScoped<IBudgetFishGrowthProfileService, BudgetFishGrowthProfileService>();
        services.AddScoped<IBudgetCalibrationDefinitionService, BudgetCalibrationDefinitionService>();
        services.AddScoped<IBudgetFeedConsumptionRateService, BudgetFeedConsumptionRateService>();
        services.AddScoped<IBudgetAdjustmentRateDefinitionService, BudgetAdjustmentRateDefinitionService>();
        return services;
    }
}
