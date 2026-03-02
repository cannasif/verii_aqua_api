# BatchMovement Teknik Dokümanı (Aqua)

Bu doküman, `RII_BatchMovement` tablosunun iş mantığını, tablo bağlantılarını, hangi operasyonlarda nasıl kayıt üretildiğini ve SQL tarafında nasıl okunması gerektiğini açıklar.

Kapsam: mevcut backend kodu (`verii_aqua_api`) ve aktif EF model snapshot'ına göredir.

## 1) BatchMovement Nedir?

`RII_BatchMovement`, sistemdeki **balık batch hareketlerinin ledger (hareket defteri)** tablosudur.

Amaç:
- Her post edilen operasyonun batch bazında izini tutmak.
- `RII_BatchCageBalance` anlık bakiyesinin nasıl oluştuğunu geçmişe dönük açıklamak.
- Raporlarda zaman akışı, hareket tipi, kaynak/hedef kafes, kaynak/hedef stok, gram değişimi ve işlem referansını sağlamak.

Kısaca:
- `RII_BatchCageBalance` = **anlık durum (state)**
- `RII_BatchMovement` = **olay geçmişi (event ledger)**

## 2) Fiziksel Tablo ve Kolonlar

Tablo: `RII_BatchMovement`

Kod referansı:
- `/Users/cannasif/Documents/V3rii/verii_aqua_api/Models/Aqua/Entities/BatchMovement.cs`
- `/Users/cannasif/Documents/V3rii/verii_aqua_api/Data/Configurations/AquaConfiguration/Entities/BatchMovementConfiguration.cs`

Kolonlar:

1. `Id` (bigint, PK)
- BaseEntity'den gelir.

2. `FishBatchId` (bigint, not null)
- Hareketin bağlı olduğu batch.
- FK: `RII_FishBatch(Id)`.

3. `ProjectCageId` (bigint, null)
- Hareketin yazıldığı birincil kafes bağlamı.
- FK: `RII_ProjectCage(Id)`.
- Not: Transfer/convert gibi durumlarda `From/ToProjectCageId` da ayrı tutulur.

4. `FromProjectCageId` (bigint, null)
- Kaynak kafes.
- Uygulama seviyesinde mantıksal bağ; DB tarafında FK constraint tanımlı değil.

5. `ToProjectCageId` (bigint, null)
- Hedef kafes.
- Uygulama seviyesinde mantıksal bağ; DB tarafında FK constraint tanımlı değil.

6. `FromStockId` (bigint, null)
- Kaynak stok.
- Uygulama seviyesinde mantıksal bağ; DB tarafında FK constraint tanımlı değil.

7. `ToStockId` (bigint, null)
- Hedef stok.
- Uygulama seviyesinde mantıksal bağ; DB tarafında FK constraint tanımlı değil.

8. `FromAverageGram` (decimal(18,3), null)
- Hareket öncesi ortalama gram bilgisi.

9. `ToAverageGram` (decimal(18,3), null)
- Hareket sonrası/hedef ortalama gram bilgisi.

10. `MovementDate` (datetime2(3), not null)
- İşlemin iş tarihi.

11. `MovementType` (tinyint, not null)
- Hareket tipi enum değeri.
- Check: `CK_RII_BatchMovement_MovementType IN (0,1,2,3,4,5,6,7)`.

12. `SignedCount` (int, not null)
- İşaretli adet delta.
- Pozitif: artış, negatif: azalış, `0`: sadece bilgi/iz hareketi.

13. `SignedBiomassGram` (decimal(18,3), not null)
- İşaretli biyokütle delta.
- Pozitif: artış, negatif: azalış, `0`: sadece bilgi/iz hareketi.

14. `FeedGram` (decimal(18,3), null)
- Besleme gramı (özellikle `MovementType=Feeding` için).

15. `ActorUserId` (bigint, null)
- İşlemi yapan kullanıcı.

16. `ReferenceTable` (nvarchar(50), not null)
- Kaydın geldiği iş belgesi/tablosu.
- Örn: `RII_Transfer`, `RII_Mortality`, `RII_Weighing`, `RII_Shipment`, `RII_FeedingDistribution`.

17. `ReferenceId` (bigint, not null)
- İlgili kaydın Id'si.

18. `Note` (nvarchar(500), null)
- Operasyona özgü açıklama/bağlam.

19. Base audit kolonları
- `CreatedDate`, `UpdatedDate`, `DeletedDate`, `IsDeleted`, `CreatedBy`, `UpdatedBy`, `DeletedBy`.

Index:
- `IX_RII_BatchMovement_BatchDate (FishBatchId, MovementDate)`

Soft delete:
- Global filter: `IsDeleted = 0`.

## 3) MovementType Sözlüğü

Değerler:
- `0 = Transfer`
- `1 = Mortality`
- `2 = Weighing`
- `3 = StockConvert`
- `4 = Adjustment`
- `5 = Stocking`
- `6 = Shipment`
- `7 = Feeding`

## 4) BatchCageBalance ile Bağlantı Mantığı

Tablo: `RII_BatchCageBalance`

Rol:
- `(FishBatchId, ProjectCageId)` için anlık canlı adet/ortalama gram/biyokütle taşır.
- Unique aktif kayıt: `UX_RII_BatchCageBalance_BatchCage_Active`.
- Non-negative check: canlı, gram, biyokütle negatif olamaz.



`BalanceLedgerManager.ApplyDelta(...)` davranışı:
1. İlgili batch-cage balance kaydını bulur, yoksa oluşturur.
2. `deltaCount` ve `deltaBiomass` uygular.
3. Negatif bakiye kontrolü yapar.
4. `AverageGram = BiomassGram / LiveCount` (LiveCount>0 ise) yeniden hesaplar.
5. Aynı işlem için `RII_BatchMovement` satırı yazar.

Önemli:
- Yani birçok operasyonda **balance güncellemesi + movement kaydı tek yerde** yönetilir.

## 5) Hangi Operasyon Nasıl BatchMovement Üretir?

Posting endpointleri:
- `/Users/cannasif/Documents/V3rii/verii_aqua_api/Controllers/AquaController/AquaPostingController.cs`

### 5.1 Goods Receipt Post
- Servis: `GoodsReceiptService.PostAsync`
- Hareket: `Stocking`
- Etki: dağıtım satırlarına göre ilgili kafeslerde +adet/+biyokütle
- Referans: `ReferenceTable = nameof(GoodsReceipt)`


### 5.2 Transfer Post
- Servis: `TransferService.Post`
- Hareket: `Transfer`
- Etki:
  - Kaynak kafes: `-FishCount`, `-Biomass`
  - Hedef kafes: `+FishCount`, `+Biomass`
- Referans: `RII_Transfer`
- `FromProjectCageId` / `ToProjectCageId` alanları dolu gelir.


### 5.3 Mortality Post
- Servis: `MortalityService.Post`
- Hareket: `Mortality`
- Etki:
  - `SignedCount = -DeadCount`
  - `SignedBiomassGram = -(balance.AverageGram * DeadCount)`
- Referans: `RII_Mortality`


### 5.4 Weighing Post
- Servis: `WeighingService.Post`
- Hareket: `Weighing`
- Etki:
  - Şu an kodda `SignedCount=0`, `SignedBiomassGram=0`
  - Gram değişimi `FromAverageGram/ToAverageGram` alanlarında taşınır.
  - Ayrıca `FishBatch.CurrentAverageGram` güncellenir.
- Referans: `RII_Weighing`


### 5.5 Stock Convert Post
- Servis: `StockConvertService.Post`
- Hareket: `StockConvert`
- Etki:
  - Kaynak batch/cage için negatif delta
  - Hedef batch/cage için pozitif delta
  - Stok bağlamı (`FromStockId`, `ToStockId`) ve gram bağlamı (`FromAverageGram`, `ToAverageGram`) yazılır.
- Referans: `RII_StockConvert`


### 5.6 Shipment Post
- Servis: `ShipmentService.Post`
- Hareket: `Shipment`
- Etki:
  - Kaynak kafeslerde negatif delta (soğuk depoya çıkış)
- Referans: `RII_Shipment`
- Ek iş kuralı:
  - Kafes boşalırsa `ProjectCage.ReleasedDate` set edilir.
  - Projede canlı kalmazsa proje kapanışı tetiklenir.


Not (kritik):
- Kodda proje kapanışı `DocumentStatus.Cancelled` yapılıyor; `Closed` enum değeri yok.

### 5.7 Feeding Distribution Create
- Servis: `FeedingDistributionService.CreateAsync`
- Hareket: `Feeding`
- Etki:
  - `SignedCount=0`, `SignedBiomassGram=0`
  - `FeedGram` dolu yazılır.
- Kayıt sadece ilgili feeding belgesi `Posted` ise üretilir.
- Referans: `RII_FeedingDistribution`


### 5.8 Daily Weather CreateDaily
- Servis: `DailyWeatherService.CreateDaily`
- Hareket: `Adjustment`
- Etki:
  - Aktif batch-cage'ler için 0-delta satır (timeline bütünlüğü için)
- Referans: `RII_DailyWeather`


### 5.9 Net Operation Post
- Servis: `NetOperationService.Post`
- Hareket: `Adjustment`
- Etki:
  - 0-delta satır (izlenebilirlik amacıyla)
- Referans: `RII_NetOperationLine`


## 6) SQL Developer için Join Modeli

Temel ilişki akışı:
- `RII_BatchMovement.FishBatchId -> RII_FishBatch.Id`
- `RII_FishBatch.ProjectId -> RII_Project.Id`
- `RII_BatchMovement.ProjectCageId -> RII_ProjectCage.Id`
- `RII_ProjectCage.CageId -> RII_Cage.Id`

Doküman ilişkisi:
- `ReferenceTable + ReferenceId` ile ilgili başlık/satır tablosuna gidilir.

Önerilen pratik:
- `ReferenceTable` değerini switch/case ile yorumlayın.
- `From/To*` alanları null olabileceği için raporda null-safe gösterim kullanın.

## 7) Örnek Analitik SQL'ler

### 7.1 Bir batch için kronolojik hareket dökümü
```sql
SELECT
    bm.Id,
    bm.MovementDate,
    bm.MovementType,
    bm.SignedCount,
    bm.SignedBiomassGram,
    bm.FeedGram,
    bm.ProjectCageId,
    bm.FromProjectCageId,
    bm.ToProjectCageId,
    bm.FromStockId,
    bm.ToStockId,
    bm.FromAverageGram,
    bm.ToAverageGram,
    bm.ReferenceTable,
    bm.ReferenceId,
    bm.Note
FROM dbo.RII_BatchMovement bm
WHERE bm.IsDeleted = 0
  AND bm.FishBatchId = @FishBatchId
ORDER BY bm.MovementDate, bm.Id;
```

### 7.2 Proje bazında hareket özeti
```sql
SELECT
    p.Id AS ProjectId,
    p.ProjectCode,
    bm.MovementType,
    SUM(bm.SignedCount) AS SumCountDelta,
    SUM(bm.SignedBiomassGram) AS SumBiomassDelta,
    SUM(COALESCE(bm.FeedGram, 0)) AS SumFeedGram
FROM dbo.RII_BatchMovement bm
JOIN dbo.RII_FishBatch fb ON fb.Id = bm.FishBatchId AND fb.IsDeleted = 0
JOIN dbo.RII_Project p ON p.Id = fb.ProjectId AND p.IsDeleted = 0
WHERE bm.IsDeleted = 0
GROUP BY p.Id, p.ProjectCode, bm.MovementType
ORDER BY p.ProjectCode, bm.MovementType;
```

### 7.3 BatchCageBalance tutarlılık kontrolü (delta toplamı vs anlık)
```sql
;WITH movement_sum AS (
    SELECT
        FishBatchId,
        ProjectCageId,
        SUM(SignedCount) AS DeltaCount,
        SUM(SignedBiomassGram) AS DeltaBiomass
    FROM dbo.RII_BatchMovement
    WHERE IsDeleted = 0
      AND ProjectCageId IS NOT NULL
    GROUP BY FishBatchId, ProjectCageId
)
SELECT
    bcb.FishBatchId,
    bcb.ProjectCageId,
    bcb.LiveCount,
    bcb.BiomassGram,
    ms.DeltaCount,
    ms.DeltaBiomass,
    (bcb.LiveCount - COALESCE(ms.DeltaCount, 0)) AS CountDiff,
    (bcb.BiomassGram - COALESCE(ms.DeltaBiomass, 0)) AS BiomassDiff
FROM dbo.RII_BatchCageBalance bcb
LEFT JOIN movement_sum ms
  ON ms.FishBatchId = bcb.FishBatchId
 AND ms.ProjectCageId = bcb.ProjectCageId
WHERE bcb.IsDeleted = 0;
```

## 8) Önemli Operasyon Notları

1. Weighing kayıtları şu an biyokütle delta üretmiyor.
- `SignedBiomassGram = 0`.
- Gram değişimi yalnızca `FromAverageGram/ToAverageGram` ve batch üstünden izleniyor.

2. DailyWeather ve NetOperation, 0-delta hareket üretir.
- Timeline raporlarda gün boş görünmesin diye.

3. Feeding hareketi `FeedingDistribution` seviyesinde üretiliyor.
- `Feeding` başlık postunda doğrudan movement üretimi yok.

4. Proje kapanış statüsü:
- Shipment sonrası canlı kalmazsa kodda proje `Cancelled` durumuna çekiliyor.
- Eğer iş beklentisi “Closed” ise enum/model uyarlaması gerekir.

5. DB script vs migration farkı olabilir.
- `Database/Scripts/001_aqua_module_schema.sql` bazı ortamlarda eski check aralığı içerebilir.
- Esas referans canlı DB migration ve `AquaDbContextModelSnapshot` olmalıdır.

## 9) Kısa Özet

- `RII_BatchMovement` operasyonel gerçeğin event ledger'ıdır.
- `RII_BatchCageBalance` anlık durumdur; çoğu post işleminde `ApplyDelta` ile birlikte güncellenir.
- `MovementType + ReferenceTable/ReferenceId` üçlüsü hareketin "ne/nereden" geldiğini belirler.
- SQL raporlamada batch bazlı timeline ve proje bazlı özetler için ana kaynak tablo budur.
