using aqua_api.Interfaces;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace aqua_api.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ILogger<LocalizationService> _logger;
        private readonly ResourceManager _resourceManager;

        // If resources are missing, disable lookups to avoid repeated exceptions/noise.
        private static bool _resourceLookupDisabled;

        public LocalizationService(ILogger<LocalizationService> logger)
        {
            _logger = logger;

            var assembly = Assembly.GetExecutingAssembly();
            _resourceManager = new ResourceManager("aqua_api.Resources.Messages", assembly);
        }

        public string GetLocalizedString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (_resourceLookupDisabled)
            {
                return key;
            }

            try
            {
                var culture = NormalizeCulture(CultureInfo.CurrentUICulture);
                var localizedString = _resourceManager.GetString(key, culture);

                // Background jobs (e.g. Hangfire) often run with InvariantCulture; GetString returns null and we'd expose the key. Fall back to a default culture.
                if (string.IsNullOrWhiteSpace(localizedString))
                {
                    var fallbackCulture = new CultureInfo("en-US");
                    if (!culture.Name.Equals(fallbackCulture.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        localizedString = _resourceManager.GetString(key, fallbackCulture);
                    }
                }

                return string.IsNullOrWhiteSpace(localizedString) ? key : localizedString;
            }
            catch (MissingManifestResourceException ex)
            {
                _resourceLookupDisabled = true;
                _logger.LogWarning(ex, "Localization resources not found. Falling back to keys.");
                return key;
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
