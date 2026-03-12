using aqua_api.Interfaces;
using aqua_api.Services;

namespace aqua_api.Helpers;

public static class LocalizationBootstrap
{
    private static readonly Lazy<ILocalizationService> _localizationService = new(() =>
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        return new LocalizationService(loggerFactory.CreateLogger<LocalizationService>());
    });

    public static string GetString(string key, params object[] arguments)
    {
        return arguments.Length == 0
            ? _localizationService.Value.GetLocalizedString(key)
            : _localizationService.Value.GetLocalizedString(key, arguments);
    }
}
