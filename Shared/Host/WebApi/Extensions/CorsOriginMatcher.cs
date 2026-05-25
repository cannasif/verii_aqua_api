namespace aqua_api.Shared.Host.WebApi.Extensions;

internal static class CorsOriginMatcher
{
    public static string[] NormalizeAllowedOrigins(IEnumerable<string> allowedOrigins)
    {
        return allowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool IsAllowed(string? origin, IReadOnlyCollection<string> allowedOrigins, HostString requestHost)
    {
        if (!TryCreateOriginUri(origin, out var originUri))
        {
            return false;
        }

        if (IsSameHost(originUri, requestHost))
        {
            return true;
        }

        foreach (var allowedOrigin in allowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(allowedOrigin))
            {
                continue;
            }

            var normalizedAllowedOrigin = allowedOrigin.Trim().TrimEnd('/');
            if (IsExactMatch(originUri, normalizedAllowedOrigin))
            {
                return true;
            }

            if (IsWildcardSubdomainMatch(originUri, normalizedAllowedOrigin))
            {
                return true;
            }

            if (IsWildcardHostMatch(originUri, normalizedAllowedOrigin))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryCreateOriginUri(string? origin, out Uri originUri)
    {
        originUri = null!;
        if (string.IsNullOrWhiteSpace(origin) ||
            !Uri.TryCreate(origin.Trim().TrimEnd('/'), UriKind.Absolute, out var parsedUri))
        {
            return false;
        }

        if (parsedUri.Scheme is not "http" and not "https")
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(parsedUri.Query) ||
            !string.IsNullOrWhiteSpace(parsedUri.Fragment) ||
            (parsedUri.AbsolutePath != "/" && !string.IsNullOrWhiteSpace(parsedUri.AbsolutePath)))
        {
            return false;
        }

        originUri = parsedUri;
        return true;
    }

    private static bool IsSameHost(Uri originUri, HostString requestHost)
    {
        if (!requestHost.HasValue || string.IsNullOrWhiteSpace(requestHost.Host))
        {
            return false;
        }

        return string.Equals(originUri.Host, requestHost.Host, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExactMatch(Uri originUri, string allowedOrigin)
    {
        if (!Uri.TryCreate(allowedOrigin, UriKind.Absolute, out var allowedUri))
        {
            return false;
        }

        return string.Equals(originUri.Scheme, allowedUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(originUri.Host, allowedUri.Host, StringComparison.OrdinalIgnoreCase) &&
               GetEffectivePort(originUri) == GetEffectivePort(allowedUri);
    }

    private static bool IsWildcardSubdomainMatch(Uri originUri, string allowedOrigin)
    {
        if (!allowedOrigin.Contains("*.", StringComparison.Ordinal) ||
            !Uri.TryCreate(allowedOrigin.Replace("*.", "wildcard.", StringComparison.Ordinal), UriKind.Absolute, out var allowedUri))
        {
            return false;
        }

        if (!allowedUri.Host.StartsWith("wildcard.", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(originUri.Scheme, allowedUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
            GetEffectivePort(originUri) != GetEffectivePort(allowedUri))
        {
            return false;
        }

        var allowedHostSuffix = allowedUri.Host["wildcard.".Length..];
        return !string.Equals(originUri.Host, allowedHostSuffix, StringComparison.OrdinalIgnoreCase) &&
               originUri.Host.EndsWith($".{allowedHostSuffix}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWildcardHostMatch(Uri originUri, string allowedOrigin)
    {
        if (!allowedOrigin.Contains("://*:", StringComparison.Ordinal) ||
            !Uri.TryCreate(allowedOrigin.Replace("://*:", "://wildcard-host:", StringComparison.Ordinal), UriKind.Absolute, out var allowedUri))
        {
            return false;
        }

        return string.Equals(originUri.Scheme, allowedUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
               GetEffectivePort(originUri) == GetEffectivePort(allowedUri);
    }

    private static int GetEffectivePort(Uri uri)
    {
        if (!uri.IsDefaultPort)
        {
            return uri.Port;
        }

        return uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
    }
}
