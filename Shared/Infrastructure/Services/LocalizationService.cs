using System.Globalization;
using System.Reflection;
using System.Resources;

namespace aqua_api.Shared.Infrastructure.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ILogger<LocalizationService> _logger;
        private readonly IReadOnlyList<ResourceManager> _resourceManagers;
        private static readonly string[] ResourceBaseNames =
        {
            "aqua_api.Shared.Localization.Messages",
            "aqua_api.Modules.Identity.Localization.Messages",
            "aqua_api.Modules.Stock.Localization.Messages",
            "aqua_api.Modules.Integrations.Localization.Messages",
            "aqua_api.Modules.System.Localization.Messages",
            "aqua_api.Modules.Aqua.Localization.Messages"
        };

        public LocalizationService(ILogger<LocalizationService> logger)
        {
            _logger = logger;

            var assembly = Assembly.GetExecutingAssembly();
            _resourceManagers = ResourceBaseNames
                .Select(resourceBaseName => new ResourceManager(resourceBaseName, assembly))
                .ToArray();
        }

        public string GetLocalizedString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            try
            {
                var culture = NormalizeCulture(CultureInfo.CurrentUICulture);
                var localizedString = TryGetLocalizedString(key, culture);

                return string.IsNullOrWhiteSpace(localizedString) ? key : localizedString;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Localization lookup failed for key: {Key}", key);
                return key;
            }
        }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            var localizedString = GetLocalizedString(key);

            try
            {
                return string.Format(localizedString, arguments);
            }
            catch
            {
                return localizedString;
            }
        }

        private string? TryGetLocalizedString(string key, CultureInfo culture)
        {
            var fallbackCulture = new CultureInfo("en-US");

            foreach (var resourceManager in _resourceManagers)
            {
                try
                {
                    var localizedString = resourceManager.GetString(key, culture);
                    if (!string.IsNullOrWhiteSpace(localizedString))
                    {
                        return localizedString;
                    }

                    if (!culture.Name.Equals(fallbackCulture.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        localizedString = resourceManager.GetString(key, fallbackCulture);
                        if (!string.IsNullOrWhiteSpace(localizedString))
                        {
                            return localizedString;
                        }
                    }
                }
                catch (MissingManifestResourceException ex)
                {
                    _logger.LogDebug(ex, "Localization resource manifest not found for one resource set.");
                }
            }

            return null;
        }

        private static CultureInfo NormalizeCulture(CultureInfo culture)
        {
            var name = culture?.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return new CultureInfo("tr-TR");
            }

            var normalized = name.ToLowerInvariant() switch
            {
                "tr" or "tr-tr" => "tr-TR",
                "en" or "en-us" or "en-tr" => "en-US",
                "de" or "de-de" => "de-DE",
                "fr" or "fr-fr" => "fr-FR",
                "es" or "es-es" => "es-ES",
                "it" or "it-it" => "it-IT",
                _ => name
            };

            try
            {
                return new CultureInfo(normalized);
            }
            catch
            {
                return new CultureInfo("tr-TR");
            }
        }
    }
}
