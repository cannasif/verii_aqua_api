namespace aqua_api.Shared.Infrastructure.Configuration
{
    /// <summary>
    /// Power BI API and embed configuration.
    /// Bind from configuration section "PowerBi".
    /// </summary>
    public class PowerBISettings
    {
        public const string SectionName = "PowerBi";

        public string Authority { get; set; } = string.Empty;
        public string Scope { get; set; } = "https://analysis.windows.net/powerbi/api/.default";
        public string ApiBaseUrl { get; set; } = "https://api.powerbi.com";
        public string? WorkspaceId { get; set; }
        /// <summary>Base URL for embed (e.g. https://app.powerbi.com). Used to build embedUrl when not stored in report definition.</summary>
        public string EmbedBaseUrl { get; set; } = "https://app.powerbi.com";
    }
}
