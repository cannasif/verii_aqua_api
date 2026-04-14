using Microsoft.AspNetCore.Localization;
using System;
using System.Linq;

namespace aqua_api.Shared.Host.WebApi.Localization
{
    /// <summary>
    /// Custom culture provider that reads language from "x-language" header.
    /// It normalizes short and mixed forms (e.g., tr, tr-tr, en, en-TR) into supported cultures.
    /// </summary>
    public class CustomHeaderRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.TryGetValue("x-language", out var headerValue))
            {
                return Task.FromResult<ProviderCultureResult?>(null);
            }

            var raw = headerValue.ToString().Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Task.FromResult<ProviderCultureResult?>(null);
            }

            var normalized = NormalizeLanguage(raw);
            if (normalized == null)
            {
                return Task.FromResult<ProviderCultureResult?>(null);
            }

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(normalized, normalized));
        }

        private static string? NormalizeLanguage(string raw)
        {
            var lower = raw.ToLowerInvariant();
            var primary = lower.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return (lower, primary) switch
            {
                ("tr", _) or ("tr-tr", _) or (_, "tr") => "tr-TR",
                ("en", _) or ("en-us", _) or (_, "en") => "en-US",
                ("de", _) or ("de-de", _) or (_, "de") => "de-DE",
                ("fr", _) or ("fr-fr", _) or (_, "fr") => "fr-FR",
                ("es", _) or ("es-es", _) or (_, "es") => "es-ES",
                ("it", _) or ("it-it", _) or (_, "it") => "it-IT",
                _ => null
            };
        }
    }
}
