namespace aqua_api.Modules.Integrations.Application.Services
{
    public interface INetsisAuthTokenService
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        Task<NetsisTokenResultDto> NetsisGetTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
    }
}
