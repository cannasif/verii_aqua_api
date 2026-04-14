namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class StokDto
    {
        public string? StokKodu { get; set; }
        public string? StokAdi { get; set; }
        public string? GrupKodu { get; set; }
        public string? UreticiKodu { get; set; }
        public string? OlcuBr1 { get; set; }
        public decimal? SatisFiat1 { get; set; }
        public decimal? SatisFiat2 { get; set; }
        public decimal? SatisFiat3 { get; set; }
        public decimal? SatisFiat4 { get; set; }
        public decimal? KdvOrani { get; set; }
        public short? DepoKodu { get; set; }
    }
}
