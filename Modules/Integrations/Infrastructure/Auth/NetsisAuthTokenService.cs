using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using aqua_api.Modules.Integrations.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace aqua_api.Modules.Integrations.Infrastructure.Auth
{
    public class NetsisAuthTokenService : INetsisAuthTokenService
    {
        private const string CacheKeyPrefix = "netsis:rest:token";
        private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<NetsisOptions> _netsisOptions;
        private readonly ILogger<NetsisAuthTokenService> _logger;

        public NetsisAuthTokenService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            IOptions<NetsisOptions> netsisOptions,
            ILogger<NetsisAuthTokenService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _netsisOptions = netsisOptions;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var token = await NetsisGetTokenAsync(false, cancellationToken).ConfigureAwait(false);
            return token.AccessToken;
        }

        public async Task<NetsisTokenResultDto> NetsisGetTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            var options = _netsisOptions.Value;
            ValidateOptions(options);

            var branchCode = ResolveRequestBranchCode(options);
            var cacheKey = BuildCacheKey(branchCode);

            if (!forceRefresh && TryGetValidCachedToken(cacheKey, options, out var cachedToken))
            {
                return cachedToken!;
            }

            await TokenSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!forceRefresh && TryGetValidCachedToken(cacheKey, options, out cachedToken))
                {
                    return cachedToken!;
                }

                var existingToken = _memoryCache.Get<NetsisTokenCacheEntry>(cacheKey);
                if (!forceRefresh && existingToken != null && CanRefresh(existingToken, options))
                {
                    try
                    {
                        var refreshedToken = await RefreshAccessTokenAsync(existingToken.RefreshToken!, options, branchCode, cancellationToken)
                            .ConfigureAwait(false);
                        CacheToken(cacheKey, refreshedToken, options);
                        return refreshedToken;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Netsis refresh token request failed. Falling back to password grant.");
                    }
                }

                var freshToken = await RequestPasswordTokenAsync(options, branchCode, cancellationToken).ConfigureAwait(false);
                CacheToken(cacheKey, freshToken, options);
                return freshToken;
            }
            finally
            {
                TokenSemaphore.Release();
            }
        }

        private bool TryGetValidCachedToken(string cacheKey, NetsisOptions options, out NetsisTokenResultDto? token)
        {
            token = null;
            var cached = _memoryCache.Get<NetsisTokenCacheEntry>(cacheKey);
            if (cached == null || string.IsNullOrWhiteSpace(cached.AccessToken))
            {
                return false;
            }

            if (cached.AccessTokenExpiresAtUtc <= DateTime.UtcNow.AddSeconds(options.Rest.TokenExpirySkewSeconds))
            {
                return false;
            }

            token = cached.ToResultDto("memory");
            return true;
        }

        private static bool CanRefresh(NetsisTokenCacheEntry cached, NetsisOptions options)
        {
            if (string.IsNullOrWhiteSpace(cached.RefreshToken))
            {
                return false;
            }

            return !cached.RefreshTokenExpiresAtUtc.HasValue
                || cached.RefreshTokenExpiresAtUtc.Value > DateTime.UtcNow.AddSeconds(options.Rest.TokenExpirySkewSeconds);
        }

        private async Task<NetsisTokenResultDto> RequestPasswordTokenAsync(
            NetsisOptions options,
            string? branchCode,
            CancellationToken cancellationToken)
        {
            var attempts = BuildPasswordLoginAttempts(options, branchCode);
            var failures = new List<string>();

            foreach (var attempt in attempts)
            {
                try
                {
                    return await SendTokenRequestAsync(attempt, options, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    failures.Add(ex.Message);
                    _logger.LogWarning(ex, "Netsis login attempt failed. Source: {Source}", attempt.Source);
                }
            }

            throw new NetsisIntegrationValidationException(
                $"Netsis bağlantısı kurulamadı, token alınamadı. ERP bağlantı bilgilerini, şube kodunu ve Netsis kullanıcı bilgilerini kontrol edin. Detay: {string.Join(" | ", failures)}");
        }

        private async Task<NetsisTokenResultDto> RefreshAccessTokenAsync(
            string refreshToken,
            NetsisOptions options,
            string? branchCode,
            CancellationToken cancellationToken)
        {
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["branchcode"] = branchCode?.Trim() ?? string.Empty,
                ["password"] = options.Rest.Password,
                ["username"] = options.Rest.Username,
                ["dbname"] = options.Rest.DbName,
                ["dbuser"] = options.Rest.DbUser,
                ["dbpassword"] = options.Rest.DbPassword?.Trim() ?? string.Empty,
                ["dbtype"] = ResolveOAuthDbType(options.Rest.DbType),
                ["refresh_token"] = refreshToken,
            };

            var attempt = BuildFormAttempt("refresh", ResolvePrimaryLoginPath(options), formData, formData.GetValueOrDefault("branchcode"));
            return await SendTokenRequestAsync(attempt, options, cancellationToken).ConfigureAwait(false);
        }

        private async Task<NetsisTokenResultDto> SendTokenRequestAsync(
            NetsisTokenRequestAttempt attempt,
            NetsisOptions options,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, attempt.LoginPath)
            {
                Content = attempt.CreateContent()
            };

            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var requestUri = request.RequestUri?.IsAbsoluteUri == true
                ? request.RequestUri.ToString()
                : new Uri(_httpClient.BaseAddress ?? new Uri(options.Rest.BaseUrl.TrimEnd('/') + "/"), attempt.LoginPath).ToString();
            var requestContext = BuildRequestContext(requestUri, attempt.Payload, attempt.Source);

            if (!response.IsSuccessStatusCode)
            {
                throw BuildNetsisTokenException(response.StatusCode, responseBody, requestContext);
            }

            var tokenResponse = ReadTokenResponse(responseBody);
            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new NetsisIntegrationValidationException(
                    $"Netsis token response is invalid or access_token is missing. {requestContext}");
            }

            var now = DateTime.UtcNow;
            var expiresInSeconds = tokenResponse.ExpiresIn > 0
                ? tokenResponse.ExpiresIn
                : options.Rest.DefaultTokenLifetimeMinutes * 60;

            var accessTokenExpiresAtUtc = now.AddSeconds(expiresInSeconds);
            var refreshTokenExpiresAtUtc = tokenResponse.RefreshExpiresIn > 0
                ? now.AddSeconds(tokenResponse.RefreshExpiresIn)
                : (DateTime?)null;

            return new NetsisTokenResultDto
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = string.IsNullOrWhiteSpace(tokenResponse.TokenType) ? "Bearer" : tokenResponse.TokenType,
                ExpiresInSeconds = expiresInSeconds,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
                BranchCode = attempt.BranchCode,
                Source = attempt.Source,
            };
        }

        private void CacheToken(string cacheKey, NetsisTokenResultDto token, NetsisOptions options)
        {
            var entry = new NetsisTokenCacheEntry
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                TokenType = token.TokenType,
                AccessTokenExpiresAtUtc = token.AccessTokenExpiresAtUtc,
                RefreshTokenExpiresAtUtc = token.RefreshTokenExpiresAtUtc,
                BranchCode = token.BranchCode,
            };

            var absoluteExpiration = token.RefreshTokenExpiresAtUtc
                ?? token.AccessTokenExpiresAtUtc.AddMinutes(options.Rest.DefaultTokenLifetimeMinutes);

            _memoryCache.Set(cacheKey, entry, new MemoryCacheEntryOptions { AbsoluteExpiration = absoluteExpiration });
        }

        private static IReadOnlyList<NetsisTokenRequestAttempt> BuildPasswordLoginAttempts(NetsisOptions options, string? branchCode)
        {
            var jLoginPayload = BuildNetOpenXJLoginPayload(options, branchCode);
            var jLoginForm = jLoginPayload.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
            var oauthForm = BuildPasswordGrantForm(options, branchCode, ResolveOAuthDbType(options.Rest.DbType));
            var legacyOAuthForm = BuildPasswordGrantForm(options, branchCode, NormalizeDbTypeText(options.Rest.DbType));
            var loginPaths = BuildLoginPaths(options);
            var attempts = new List<NetsisTokenRequestAttempt>();

            foreach (var loginPath in loginPaths)
            {
                attempts.Add(BuildFormAttempt($"oauth-password-form:{loginPath}", loginPath, oauthForm, branchCode));
                attempts.Add(BuildFormAttempt($"oauth-password-form-legacy-dbtype:{loginPath}", loginPath, legacyOAuthForm, branchCode));
            }

            foreach (var loginPath in loginPaths)
            {
                attempts.Add(BuildJsonAttempt($"netopenx-jlogin-json:{loginPath}", loginPath, jLoginPayload, branchCode));
                attempts.Add(BuildFormAttempt($"netopenx-jlogin-form:{loginPath}", loginPath, jLoginForm, branchCode));
            }

            return attempts;
        }

        private static Dictionary<string, object?> BuildNetOpenXJLoginPayload(NetsisOptions options, string? branchCode)
        {
            return new Dictionary<string, object?>
            {
                ["BranchCode"] = ToBranchCodeValue(branchCode),
                ["NetsisUser"] = options.Rest.Username,
                ["NetsisPassword"] = options.Rest.Password,
                ["DbName"] = options.Rest.DbName,
                ["DbUser"] = options.Rest.DbUser,
                ["DbPassword"] = options.Rest.DbPassword?.Trim() ?? string.Empty,
                ["DbType"] = ResolveNetOpenXDbType(options.Rest.DbType),
            };
        }

        private static Dictionary<string, string> BuildPasswordGrantForm(NetsisOptions options, string? branchCode, string dbType)
        {
            return new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["branchcode"] = branchCode?.Trim() ?? string.Empty,
                ["password"] = options.Rest.Password,
                ["username"] = options.Rest.Username,
                ["dbname"] = options.Rest.DbName,
                ["dbuser"] = options.Rest.DbUser,
                ["dbpassword"] = options.Rest.DbPassword?.Trim() ?? string.Empty,
                ["dbtype"] = dbType,
            };
        }

        private static NetsisTokenRequestAttempt BuildJsonAttempt(
            string source,
            string loginPath,
            IReadOnlyDictionary<string, object?> payload,
            string? branchCode)
        {
            var stringPayload = payload.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);

            return new NetsisTokenRequestAttempt(
                source,
                loginPath,
                branchCode,
                stringPayload,
                () => new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
        }

        private static NetsisTokenRequestAttempt BuildFormAttempt(
            string source,
            string loginPath,
            IReadOnlyDictionary<string, string> formData,
            string? branchCode)
        {
            return new NetsisTokenRequestAttempt(source, loginPath, branchCode, formData, () => new FormUrlEncodedContent(formData));
        }

        private static object? ToBranchCodeValue(string? branchCode)
        {
            return int.TryParse(branchCode, out var numericBranchCode)
                ? numericBranchCode
                : branchCode;
        }

        private static string ResolveNetOpenXDbType(string? dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType))
            {
                return "vtMSSQL";
            }

            var normalized = dbType.Trim();
            return normalized.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
                ? "vtMSSQL"
                : normalized;
        }

        private static string ResolveOAuthDbType(string? dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType))
            {
                return "0";
            }

            var normalized = dbType.Trim();
            return normalized.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("vtMSSQL", StringComparison.OrdinalIgnoreCase)
                    ? "0"
                    : normalized;
        }

        private static string NormalizeDbTypeText(string? dbType)
        {
            return string.IsNullOrWhiteSpace(dbType) ? "MSSQL" : dbType.Trim();
        }

        private static string ResolveLoginPath(NetsisOptions options)
        {
            return string.IsNullOrWhiteSpace(options.Rest.LoginPath) ? "/api/v2/token" : options.Rest.LoginPath;
        }

        private static string ResolvePrimaryLoginPath(NetsisOptions options)
        {
            return BuildLoginPaths(options)[0];
        }

        private static IReadOnlyList<string> BuildLoginPaths(NetsisOptions options)
        {
            var configuredPath = NormalizeLoginPath(ResolveLoginPath(options));
            var paths = new List<string> { "/token" };

            if (!paths.Contains(configuredPath, StringComparer.OrdinalIgnoreCase))
            {
                paths.Add(configuredPath);
            }

            if (!paths.Contains("/api/v2/token", StringComparer.OrdinalIgnoreCase))
            {
                paths.Add("/api/v2/token");
            }

            return paths;
        }

        private static string NormalizeLoginPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/token";
            }

            var trimmed = path.Trim();
            return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
        }

        private string? ResolveRequestBranchCode(NetsisOptions options)
        {
            var context = _httpContextAccessor.HttpContext;
            var branchCode = context?.Items["BranchCode"]?.ToString();

            if (string.IsNullOrWhiteSpace(branchCode))
            {
                branchCode = context?.Request.Headers["X-Branch-Code"].FirstOrDefault()
                    ?? context?.Request.Headers["Branch-Code"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(branchCode))
            {
                branchCode = options.Rest.BranchCode;
            }

            return string.IsNullOrWhiteSpace(branchCode) ? null : branchCode.Trim();
        }

        private static string BuildCacheKey(string? branchCode)
        {
            return string.IsNullOrWhiteSpace(branchCode)
                ? $"{CacheKeyPrefix}:default"
                : $"{CacheKeyPrefix}:branch:{branchCode.Trim()}";
        }

        private static Exception BuildNetsisTokenException(HttpStatusCode statusCode, string responseBody, string requestContext)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<NetsisErrorResponse>(responseBody, JsonOptions);
                if (!string.IsNullOrWhiteSpace(payload?.ErrorDescription))
                {
                    return new NetsisIntegrationValidationException(
                        $"Netsis token request failed ({(int)statusCode}). {payload.ErrorDescription} {requestContext}");
                }

                if (!string.IsNullOrWhiteSpace(payload?.Error))
                {
                    return new NetsisIntegrationValidationException(
                        $"Netsis token request failed ({(int)statusCode}). {payload.Error} {requestContext}");
                }
            }
            catch
            {
                // Ignore parse failures and fall back to raw body.
            }

            return new NetsisIntegrationValidationException(
                $"Netsis token request failed ({(int)statusCode}). Body: {responseBody} {requestContext}");
        }

        private static string BuildRequestContext(string requestUri, IReadOnlyDictionary<string, string> formData, string source)
        {
            var safePayload = formData.ToDictionary(kvp => kvp.Key, kvp => MaskSensitiveValue(kvp.Key, kvp.Value));
            return $"Request: POST {requestUri}. Source: {source}. Payload: {JsonSerializer.Serialize(safePayload)}";
        }

        private static string MaskSensitiveValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return key.ToLowerInvariant() switch
            {
                "password" => "***",
                "dbpassword" => "***",
                "refresh_token" => "***",
                _ => value
            };
        }

        private static NetsisTokenResponse? ReadTokenResponse(string responseBody)
        {
            NetsisTokenResponse? typed = null;
            try
            {
                typed = JsonSerializer.Deserialize<NetsisTokenResponse>(responseBody, JsonOptions);
            }
            catch
            {
                // Some NetOpenX builds return a non-object body on token success/failure.
            }

            if (!string.IsNullOrWhiteSpace(typed?.AccessToken))
            {
                return typed;
            }

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                var root = UnwrapData(document.RootElement);
                if (root.ValueKind == JsonValueKind.String)
                {
                    return new NetsisTokenResponse { AccessToken = root.GetString() };
                }

                return new NetsisTokenResponse
                {
                    AccessToken = GetString(root, "access_token", "accessToken", "AccessToken", "token", "Token", "value", "Value"),
                    RefreshToken = GetString(root, "refresh_token", "refreshToken", "RefreshToken"),
                    TokenType = GetString(root, "token_type", "tokenType", "TokenType"),
                    ExpiresIn = GetInt(root, "expires_in", "expiresIn", "ExpiresIn"),
                    RefreshExpiresIn = GetInt(root, "refresh_expires_in", "refreshExpiresIn", "RefreshExpiresIn")
                };
            }
            catch
            {
                return typed;
            }
        }

        private static JsonElement UnwrapData(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var propertyName in new[] { "data", "Data", "result", "Result" })
                {
                    if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Object)
                    {
                        return value;
                    }
                }
            }

            return root;
        }

        private static string? GetString(JsonElement root, params string[] names)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var value))
                {
                    return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
                }
            }

            return null;
        }

        private static int GetInt(JsonElement root, params string[] names)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                return 0;
            }

            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numericValue))
                {
                    return numericValue;
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var stringValue))
                {
                    return stringValue;
                }
            }

            return 0;
        }

        private static void ValidateOptions(NetsisOptions options)
        {
            if (!options.Enabled)
            {
                throw new NetsisIntegrationValidationException("Netsis entegrasyonu kapalı. Sunucu entegrasyon ayarlarını kontrol edin.");
            }

            if (string.IsNullOrWhiteSpace(options.Rest.BaseUrl))
            {
                throw new NetsisIntegrationValidationException("Netsis Rest BaseUrl tanımlı değil. Sunucu entegrasyon ayarlarını kontrol edin.");
            }

            if (string.IsNullOrWhiteSpace(options.Rest.Username))
            {
                throw new NetsisIntegrationValidationException("Netsis Rest kullanıcı adı tanımlı değil. Sunucu entegrasyon ayarlarını kontrol edin.");
            }

            if (string.IsNullOrWhiteSpace(options.Rest.Password))
            {
                throw new NetsisIntegrationValidationException("Netsis Rest şifresi tanımlı değil. Sunucu entegrasyon ayarlarını kontrol edin.");
            }
        }

        private sealed class NetsisTokenCacheEntry
        {
            public string AccessToken { get; set; } = string.Empty;
            public string? RefreshToken { get; set; }
            public string TokenType { get; set; } = "Bearer";
            public DateTime AccessTokenExpiresAtUtc { get; set; }
            public DateTime? RefreshTokenExpiresAtUtc { get; set; }
            public string? BranchCode { get; set; }

            public NetsisTokenResultDto ToResultDto(string source)
            {
                return new NetsisTokenResultDto
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    TokenType = TokenType,
                    ExpiresInSeconds = (int)Math.Max(0, (AccessTokenExpiresAtUtc - DateTime.UtcNow).TotalSeconds),
                    AccessTokenExpiresAtUtc = AccessTokenExpiresAtUtc,
                    RefreshTokenExpiresAtUtc = RefreshTokenExpiresAtUtc,
                    BranchCode = BranchCode,
                    Source = source
                };
            }
        }

        private sealed record NetsisTokenRequestAttempt(
            string Source,
            string LoginPath,
            string? BranchCode,
            IReadOnlyDictionary<string, string> Payload,
            Func<HttpContent> CreateContent);

        private sealed class NetsisTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("token_type")]
            public string? TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_expires_in")]
            public int RefreshExpiresIn { get; set; }
        }

        private sealed class NetsisErrorResponse
        {
            [JsonPropertyName("error")]
            public string? Error { get; set; }

            [JsonPropertyName("error_description")]
            public string? ErrorDescription { get; set; }
        }
    }
}
