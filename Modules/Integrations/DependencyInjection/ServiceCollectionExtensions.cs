
namespace aqua_api.Modules.Integrations.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationsModule(this IServiceCollection services)
    {
        services.AddScoped<IErpService, ErpService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<ISmtpSettingsService, SmtpSettingsService>();

        return services;
    }
}
