namespace aqua_api.Modules.CurrentDirection.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrentDirectionModule(this IServiceCollection services)
        {
            services.AddScoped<ICurrentDirectionService, CurrentDirectionService>();
            services.AddScoped<ICurrentDirectionMatchService, CurrentDirectionMatchService>();
            return services;
        }
    }
}
