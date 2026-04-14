namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class CariDto
    {
        public short SubeKodu { get; set; }
        public short IsletmeKodu { get; set; }
        public string CariKod { get; set; } = string.Empty;
        public string? CariIsim { get; set; }
        public string? CariTel { get; set; }
        public string? CariIl { get; set; }
        public string? CariAdres { get; set; }
        public string? CariIlce { get; set; }
        public string? UlkeKodu { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? VergiNumarasi { get; set; }
        public string? VergiDairesi { get; set; }
        public string? TcknNumber { get; set; }
    }
}
