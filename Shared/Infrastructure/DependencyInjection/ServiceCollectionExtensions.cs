using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;

namespace aqua_api.Shared.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAquaSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IFileUploadService, FileUploadService>();

        return services;
    }
}
