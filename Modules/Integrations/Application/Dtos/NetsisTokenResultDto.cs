namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class NetsisTokenResultDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresInSeconds { get; set; }
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; set; }
        public string? BranchCode { get; set; }
        public string Source { get; set; } = "password";
    }
}
