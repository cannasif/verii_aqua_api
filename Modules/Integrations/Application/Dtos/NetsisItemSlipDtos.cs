using System.Text.Json.Serialization;

namespace aqua_api.Modules.Integrations.Application.Dtos;

public enum NetsisItemSlipDocumentType
{
    SalesInvoice = 0,
    PurchaseInvoice = 1,
    SalesDispatch = 2,
    PurchaseDispatch = 3,
    WarehouseTransferIn = 8,
    WarehouseTransferOut = 9,
    VendorOrder = 6,
    CustomerOrder = 7,
    PurchaseDemand = 12,
    PurchaseQuotation = 13,
    SalesDemand = 14,
    SalesQuotation = 15
}

public enum NetsisItemSlipInvoiceType
{
    DomesticClosed = 1,
    DomesticOpen = 2,
    Miscellaneous = 3,
    Return = 4,
    ImportExport = 6
}

public sealed class NetsisItemSlipCreateDto
{
    [JsonPropertyName("SIPDEPOKODKULLAN")]
    public int SipDepoKodKullan { get; set; }

    [JsonPropertyName("Seri")]
    public string? Seri { get; set; }

    [JsonPropertyName("FatUst")]
    public NetsisItemSlipHeaderDto FatUst { get; set; } = new();

    [JsonPropertyName("KayitliNumaraOtomatikGuncellensin")]
    public bool KayitliNumaraOtomatikGuncellensin { get; set; } = true;

    [JsonPropertyName("SeriliHesapla")]
    public bool SeriliHesapla { get; set; } = true;

    [JsonPropertyName("Kalems")]
    public List<NetsisItemSlipLineDto> Kalems { get; set; } = [];
}

public sealed class NetsisItemSlipHeaderDto
{
    [JsonPropertyName("Seri")]
    public string? Seri { get; set; }

    [JsonPropertyName("FATIRS_NO")]
    public string? FatirsNo { get; set; }

    [JsonPropertyName("Sube_Kodu")]
    public int? SubeKodu { get; set; }

    [JsonPropertyName("CariKod")]
    public string? CariKod { get; set; }

    [JsonPropertyName("Tarih")]
    public string? Tarih { get; set; }

    [JsonPropertyName("FIYATTARIHI")]
    public string? FiyatTarihi { get; set; }

    [JsonPropertyName("SIPARIS_TEST")]
    public string? SiparisTeslimTarihi { get; set; }

    [JsonPropertyName("Tip")]
    public NetsisItemSlipDocumentType? Tip { get; set; }

    [JsonPropertyName("TIPI")]
    public NetsisItemSlipInvoiceType? Tipi { get; set; }

    [JsonPropertyName("SIPDEPOKODKULLAN")]
    public int? SipDepoKodKullan { get; set; }

    [JsonPropertyName("DEPO_KODU")]
    public int? DepoKodu { get; set; }

    [JsonPropertyName("KDV_DAHILMI")]
    public bool? KdvDahilMi { get; set; }

    [JsonPropertyName("Proje_Kodu")]
    public string? ProjeKodu { get; set; }

    [JsonPropertyName("PLA_KODU")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlasiyerKodu { get; set; }

    [JsonPropertyName("Aciklama")]
    public string? Aciklama { get; set; }

    [JsonPropertyName("KOD1")]
    public string? Kod1 { get; set; }

    [JsonPropertyName("KOD2")]
    public string? Kod2 { get; set; }

    [JsonPropertyName("EKACK1")]
    public string? EkAciklama1 { get; set; }

    [JsonPropertyName("EKACK2")]
    public string? EkAciklama2 { get; set; }

    [JsonPropertyName("EKACK3")]
    public string? EkAciklama3 { get; set; }

    [JsonPropertyName("EKACK4")]
    public string? EkAciklama4 { get; set; }

    [JsonPropertyName("EKACK5")]
    public string? EkAciklama5 { get; set; }

    [JsonPropertyName("EKACK6")]
    public string? EkAciklama6 { get; set; }

    [JsonPropertyName("EKACK7")]
    public string? EkAciklama7 { get; set; }

    [JsonPropertyName("EKACK8")]
    public string? EkAciklama8 { get; set; }

    [JsonPropertyName("EKACK9")]
    public string? EkAciklama9 { get; set; }

    [JsonPropertyName("EKACK10")]
    public string? EkAciklama10 { get; set; }

    [JsonPropertyName("EKACK11")]
    public string? EkAciklama11 { get; set; }

    [JsonPropertyName("EKACK12")]
    public string? EkAciklama12 { get; set; }

    [JsonPropertyName("EKACK13")]
    public string? EkAciklama13 { get; set; }

    [JsonPropertyName("EKACK14")]
    public string? EkAciklama14 { get; set; }

    [JsonPropertyName("EKACK15")]
    public string? EkAciklama15 { get; set; }

    [JsonPropertyName("EKACK16")]
    public string? EkAciklama16 { get; set; }

    [JsonPropertyName("FarkliTeslimYeri")]
    public string? FarkliTeslimYeri { get; set; }

    [JsonPropertyName("GEN_ISK1O")]
    public decimal? GenelIskonto1Orani { get; set; }

    [JsonPropertyName("GEN_ISK1T")]
    public decimal? GenelIskonto1Tutari { get; set; }
}

public sealed class NetsisItemSlipLineDto
{
    [JsonPropertyName("StokKodu")]
    public string? StokKodu { get; set; }

    [JsonPropertyName("STra_GCMIK")]
    public decimal Miktar { get; set; }

    [JsonPropertyName("STra_NF")]
    public decimal NetFiyat { get; set; }

    [JsonPropertyName("STra_BF")]
    public decimal BrutFiyat { get; set; }

    [JsonPropertyName("STra_ACIK")]
    public string? Aciklama { get; set; }

    [JsonPropertyName("Aciklama1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama1 { get; set; }

    [JsonPropertyName("Aciklama2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama2 { get; set; }

    [JsonPropertyName("ACIKLAMA3")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama3 { get; set; }

    [JsonPropertyName("Aciklama4")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama4 { get; set; }

    [JsonPropertyName("Aciklama8")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama8 { get; set; }

    [JsonPropertyName("ACIKLAMA9")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Aciklama9 { get; set; }

    [JsonPropertyName("SatirBaziAciks")]
    public List<string?>? SatirBaziAciks { get; set; }

    [JsonPropertyName("STra_DOVTIP")]
    public string? DovizTipi { get; set; }

    [JsonPropertyName("STra_DOVFIAT")]
    public decimal? DovizFiyat { get; set; }

    [JsonPropertyName("DEPO_KODU")]
    public int? DepoKodu { get; set; }

    [JsonPropertyName("GirisDepoKodu")]
    public int? GirisDepoKodu { get; set; }

    [JsonPropertyName("CikisDepoKodu")]
    public int? CikisDepoKodu { get; set; }

    [JsonPropertyName("ReferansKodu")]
    public string? ReferansKodu { get; set; }

    [JsonPropertyName("ProjeKodu")]
    public string? ProjeKodu { get; set; }

    [JsonPropertyName("STra_KDV")]
    public decimal? KdvOrani { get; set; }

    [JsonPropertyName("STra_SatIsk")]
    public decimal? Iskonto1 { get; set; }

    [JsonPropertyName("STra_SatIsk2")]
    public decimal? Iskonto2 { get; set; }

    [JsonPropertyName("STra_SatIsk3")]
    public decimal? Iskonto3 { get; set; }
}

public sealed class NetsisItemSlipCreateResponseDto
{
    public bool IsSuccessful { get; set; }
    public bool? IsSuccessStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDesc { get; set; }
    public string? ErrorDescription { get; set; }
    public NetsisItemSlipResponseDataDto? Data { get; set; }
    public string? RawResponse { get; set; }
}

public sealed class NetsisItemSlipResponseDataDto
{
    public string? FisNo { get; set; }
    public string? KayitNo { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BelgeNo { get; set; }
}
