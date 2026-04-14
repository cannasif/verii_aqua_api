using System.Collections.Generic;

namespace aqua_api.Shared.Infrastructure.Configuration
{
    /// <summary>
    /// Options for PDF builder: image URL allowlist (SSRF), timeouts, max size.
    /// Default: only data: images allowed. Set AllowlistedImageHosts to enable external URLs.
    /// </summary>
    public class PdfBuilderOptions
    {
        public const string SectionName = "PdfBuilder";

        /// <summary>
        /// If empty, only data: URIs are allowed for images. If non-empty, only these hosts (e.g. "https://cdn.example.com") are allowed.
        /// localhost and private IPs are always blocked.
        /// </summary>
        public List<string> AllowlistedImageHosts { get; set; } = new();

        /// <summary>
        /// Timeout in seconds for fetching external images. Default 5.
        /// </summary>
        public int ImageFetchTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Max size in bytes for a single image. Default 5MB.
        /// </summary>
        public int MaxImageSizeBytes { get; set; } = 5 * 1024 * 1024;

        /// <summary>
        /// Allowed image content types for data: URIs (e.g. image/png, image/jpeg). Empty = allow common image types.
        /// </summary>
        public List<string> AllowedImageContentTypes { get; set; } = new() { "image/png", "image/jpeg", "image/gif", "image/webp" };
    }
}
