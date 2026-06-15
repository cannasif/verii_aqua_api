namespace aqua_api.Modules.Integrations.Infrastructure.Options
{
    public class NetsisAuthOptions
    {
        public string TokenUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
}
