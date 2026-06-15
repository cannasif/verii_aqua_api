namespace aqua_api.Modules.Integrations.Application.Services
{
    /// <summary>
    /// Raw Netsis REST transport abstraction. Bearer token attachment stays at the infrastructure boundary.
    /// </summary>
    public interface INetsisRestClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
