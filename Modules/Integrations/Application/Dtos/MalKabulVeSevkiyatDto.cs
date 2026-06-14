namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class MalKabulVeSevkiyatDto
    {
        public DateTime Tarih { get; set; }
        public string? FisNo { get; set; }
        public short? KafesKodu { get; set; }
        public string? ProjeKodu { get; set; }
        public string StokKodu { get; set; } = string.Empty;
        public string? StokAdi { get; set; }
        public decimal? Miktar { get; set; }
        public string HareketTuru { get; set; } = string.Empty;
        public string GcKodu { get; set; } = string.Empty;
        public string? GrupKodu { get; set; }
        public string IslemTuru { get; set; } = string.Empty;
    }
}
