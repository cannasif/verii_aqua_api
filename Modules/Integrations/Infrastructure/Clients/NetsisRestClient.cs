using System.Net.Http.Headers;

namespace aqua_api.Modules.Integrations.Infrastructure.Clients
{
    public class NetsisRestClient : INetsisRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly INetsisAuthTokenService _tokenService;
        private readonly ILogger<NetsisRestClient> _logger;

        public NetsisRestClient(
            HttpClient httpClient,
            INetsisAuthTokenService tokenService,
            ILogger<NetsisRestClient> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request.Headers.Authorization == null)
            {
                var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            _logger.LogDebug(
                "Netsis REST request sending. Method: {Method}, Uri: {Uri}",
                request.Method.Method,
                request.RequestUri?.ToString());

            return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
