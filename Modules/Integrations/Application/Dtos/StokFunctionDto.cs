namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class StokFunctionDto
    {
        public short SubeKodu { get; set; }
        public short IsletmeKodu { get; set; }
        public string StokKodu { get; set; } = string.Empty;
        public string? OlcuBr1 { get; set; }
        public string? UreticiKodu { get; set; }
        public string? StokAdi { get; set; }
        public string? GrupKodu { get; set; }
        public string? GrupIsim { get; set; }
        public string? Kod1 { get; set; }
        public string? Kod1Adi { get; set; }
        public string? Kod2 { get; set; }
        public string? Kod2Adi { get; set; }
        public string? Kod3 { get; set; }
        public string? Kod3Adi { get; set; }
        public string? Kod4 { get; set; }
        public string? Kod4Adi { get; set; }
        public string? Kod5 { get; set; }
        public string? Kod5Adi { get; set; }
        public string? IngIsim { get; set; }
    }
}
