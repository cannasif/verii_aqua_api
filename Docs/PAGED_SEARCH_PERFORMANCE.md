# Aqua paged search performance evidence

## Ölçülen/kanıtlanan davranış

`QueryHelperSearchTests` SQL Server provider ve `ToQueryString()` kullanarak canonical sorgunun SQL çevirisini denetler:

- String genel search `LIKE ... ESCAPE` üretir ve Türkçe/ASCII bracket sınıflarını (`[cCçÇ]`, `[iIİıîÎ]`, `[uUüÜûÛ]`) parametre tarafında taşır.
- Kolon üzerinde `LOWER`, `UPPER`, `REPLACE`, `TRANSLATE` veya runtime `COLLATE` yoktur.
- Yalnız `StockName` seçildiğinde diğer search alanları `WHERE` predicate'ine girmez.
- Integral `Id` search doğrudan equality üretir; `LIKE`, string conversion veya `nvarchar` dönüşümü üretmez.
- Noktalama pattern içinde literal kalır; `%`, `_`, `[`, `]`, `^` ve `\` escape edilir.

Count search/filter/scope uygulandıktan hemen sonra, sort/paging/projection enrichment öncesinde alınır. Items aynı filtrelenmiş `IQueryable` kökünden stabil sort ve `Skip/Take` ile çıkar. Navigation yalnız public mapping navigation path'i gerektirdiğinde sorguya girer; entity graph `Include` arama altyapısının parçası değildir.

## Alternatiflerin karşılaştırması

| Yaklaşım | Doğruluk | SQL/index etkisi | Aqua kararı |
|---|---|---|---|
| Eski `Contains` / runtime `Collate` | Collation'a bağlı; ASCII/Türkçe sonucu ortama göre değişebilir | runtime collation ve kolon dönüşümü plan/index kullanımını bozabilir | canonical genel search'te kullanılmıyor |
| Türkçe bracket `LIKE '%term%'` | İstenen Türkçe/ASCII ve case varyantlarını tek sözleşmede eşler; punctuation literal | baştaki `%` nedeniyle sargable değildir, yüksek hacimde scan beklenir | mevcut doğruluk sözleşmesi |
| Code exact/prefix, name contains hibriti | Kod aramasında hızlı ve doğal; serbest ad aramasını korur | `code =` veya `code LIKE 'term%'` uygun index kullanabilir | veri/UX ölçümü sonrası endpoint predicate factory adayı |
| Persisted normalized `SearchText` + index | Deterministik normalize metin ve prefix aramada güçlü | schema, backfill, ek disk/yazma maliyeti; `%term%` yine index seek garantilemez | yalnız onaylı migration tasarımı olarak gelecek öneri |
| SQL Server Full-Text Search | Token/linguistic aramada yüksek hacme uygun | katalog/indeks işletimi, ranking ve punctuation semantiği ayrıca tasarlanmalı | ölçüm threshold'u aşılırsa PoC önerisi |

`LIKE '%term%'` leading wildcard içerdiği için sargable değildir. Bracket pattern kolon fonksiyonunu kaldırarak önceki yaklaşıma göre daha öngörülebilir bir plan sağlar, fakat scan maliyetini ortadan kaldırmaz. Özellikle Stock, StockDetail, Netsis mirror, BatchMovement ve operational line tablolarında production-benzeri veriyle execution plan, logical reads ve elapsed time ölçülmeden “hızlandı” iddiası yapılmamalıdır.

## Güvenli ortam kısıtı

Bu çalışmada güvenli ve izole bir gerçek SQL Server integration veritabanı/credential verilmedi. Production'a veya `appsettings.json` içindeki sunucuya bağlanılmadı; migration, seed ve veri yazımı yapılmadı. Bu nedenle gerçek execution plan, satır sayısı, logical read ve wall-clock değerleri raporlanamaz. SQL translation testleri correctness ve query-shape kanıtıdır, runtime performans ölçümü değildir.

Güvenli test ortamı sağlandığında önerilen read-only ölçüm matrisi:

1. Stock, StockDetail, Netsis mirror, BatchMovement ve en büyük üç line tablosunda gerçek row count kaydedilir.
2. Aynı scope/searchFields ile eski contains/collate, bracket contains ve code exact/prefix varyantları warm/cold cache altında en az 10 kez çalıştırılır.
3. Count ve items ayrı ölçülür; logical reads, CPU, elapsed time, returned count, ID order ve execution plan hash kaydedilir.
4. `Çipura/Cipura`, `Işık/Isik`, `Ağ Kafesi/Ag Kafesi`, `PEN-KOLU` ve iki terimli örneklerde ID seti/total/order eşitliği doğrulanır.
5. Kabul edilebilir p95 ve read threshold'u aşılırsa önce code hibriti, sonra normalized column veya full-text PoC değerlendirilir.

## Schema kararı

Bu değişiklik setinde `Migrations`, `DbContext`, entity precision/schema veya `appsettings` değişikliği yoktur. Persisted search column, yeni index ve full-text katalog yalnız ayrı kullanıcı onayı, migration ve geri dönüş planıyla ele alınmalıdır.
