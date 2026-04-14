namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class OnHandQuantityDto
    {
        public int DepoKodu { get; set; }
        public string? StokKodu { get; set; }
        public string? ProjeKodu { get; set; }
        public string? SeriNo { get; set; }
        public string? HucreKodu { get; set; }
        public string? Kaynak { get; set; }
        public decimal? Bakiye { get; set; }
        public string? StokAdi { get; set; }
        public string? DepoIsmi { get; set; }
    }
}
