IF OBJECT_ID(N'dbo.fn_MalKabulVeSevkiyatListesi', N'IF') IS NULL
BEGIN
    EXEC(N'
CREATE FUNCTION dbo.fn_MalKabulVeSevkiyatListesi
(
    @BaslangicTarihi DATE = ''2026-04-01''
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        TH.STHAR_TARIH AS [Tarih],
        TH.FISNO AS [Fiş No],
        TH.DEPO_KODU AS [Kafes Kodu],
        TH.PROJE_KODU AS [Proje Kodu],
        TH.STOK_KODU AS [Stok Kodu],
        SB.STOK_ADI AS [Stok Adı],
        TH.STHAR_GCMIK AS [Miktar],
        TH.STHAR_HTUR AS [Hareket Türü],
        TH.STHAR_GCKOD AS [G/C Kodu],
        SB.GRUP_KODU AS [Grup Kodu],
        CASE
            WHEN TH.STHAR_HTUR = ''J'' AND TH.STHAR_GCKOD = ''G'' AND SB.GRUP_KODU = ''YEM'' THEN N''Mal Kabul (Yem Girişi)''
            WHEN TH.STHAR_HTUR = ''J'' AND TH.STHAR_GCKOD = ''G'' AND TH.STOK_KODU LIKE ''L%'' THEN N''Mal Kabul (Balık Girişi)''
            WHEN TH.STHAR_HTUR = ''J'' AND TH.STHAR_GCKOD = ''G'' THEN N''Mal Kabul (Diğer Giriş)''
            WHEN TH.STHAR_HTUR = ''J'' AND TH.STHAR_GCKOD = ''C'' THEN N''Satış Sevkiyat''
            ELSE N''Diğer Hareket''
        END AS [İşlem Türü]
    FROM
        V3RIICO..TBLSTHAR TH WITH (NOLOCK)
    INNER JOIN
        V3RIICO..TBLSTSABIT SB WITH (NOLOCK) ON TH.STOK_KODU = SB.STOK_KODU
    WHERE
        TH.STHAR_TARIH >= @BaslangicTarihi
        AND TH.STHAR_HTUR = ''J''
        AND TH.STHAR_GCKOD IN (''G'', ''C'')
);
')
END
GO

ALTER FUNCTION dbo.fn_MalKabulVeSevkiyatListesi
(
    @BaslangicTarihi DATE = '2026-04-01'
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        TH.STHAR_TARIH AS [Tarih],
        TH.FISNO AS [Fiş No],
        TH.DEPO_KODU AS [Kafes Kodu],
        TH.PROJE_KODU AS [Proje Kodu],
        TH.STOK_KODU AS [Stok Kodu],
        SB.STOK_ADI AS [Stok Adı],
        TH.STHAR_GCMIK AS [Miktar],
        TH.STHAR_HTUR AS [Hareket Türü],
        TH.STHAR_GCKOD AS [G/C Kodu],
        SB.GRUP_KODU AS [Grup Kodu],
        CASE
            WHEN TH.STHAR_HTUR = 'J' AND TH.STHAR_GCKOD = 'G' AND SB.GRUP_KODU = 'YEM' THEN N'Mal Kabul (Yem Girişi)'
            WHEN TH.STHAR_HTUR = 'J' AND TH.STHAR_GCKOD = 'G' AND TH.STOK_KODU LIKE 'L%' THEN N'Mal Kabul (Balık Girişi)'
            WHEN TH.STHAR_HTUR = 'J' AND TH.STHAR_GCKOD = 'G' THEN N'Mal Kabul (Diğer Giriş)'
            WHEN TH.STHAR_HTUR = 'J' AND TH.STHAR_GCKOD = 'C' THEN N'Satış Sevkiyat'
            ELSE N'Diğer Hareket'
        END AS [İşlem Türü]
    FROM
        V3RIICO..TBLSTHAR TH WITH (NOLOCK)
    INNER JOIN
        V3RIICO..TBLSTSABIT SB WITH (NOLOCK) ON TH.STOK_KODU = SB.STOK_KODU
    WHERE
        TH.STHAR_TARIH >= @BaslangicTarihi
        AND TH.STHAR_HTUR = 'J'
        AND TH.STHAR_GCKOD IN ('G', 'C')
);
GO
