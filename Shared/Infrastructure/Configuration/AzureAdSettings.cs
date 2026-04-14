namespace aqua_api.Shared.Infrastructure.Configuration
{
    /// <summary>
    /// Azure AD app registration (service principal) for Client Credentials flow.
    /// Bind from configuration section "AzureAd".
    /// </summary>
    public class AzureAdSettings
    {
        public const string SectionName = "AzureAd";

        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
