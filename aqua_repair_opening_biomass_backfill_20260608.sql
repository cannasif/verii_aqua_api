/*
    Aqua repair script - opening biomass backfill
    Date: 2026-06-08

    Purpose:
    Some legacy opening count records may have LiveCount/SignedCount and AverageGram
    but zero BiomassGram/SignedBiomassGram. Devir/FCR and dashboard FCR need biomass
    to calculate "Kütle Üretim KG" and FCR.

    Formula:
    BiomassGram = Count * AverageGram

    Safe scope:
    - Active rows only (IsDeleted = 0)
    - Positive count only
    - Positive average only
    - Zero/NULL biomass only
    - Opening/stocking positive movements only for ledger backfill

    Run the PREVIEW section first. If counts look correct, run the REPAIR section.
*/

SET NOCOUNT ON;

/* =========================
   PREVIEW
   ========================= */

SELECT
    'RII_BatchCageBalance' AS TargetTable,
    COUNT(*) AS RowsToRepair,
    SUM(CAST(LiveCount AS decimal(18, 3)) * AverageGram) / 1000.0 AS BiomassKgToBackfill
FROM dbo.RII_BatchCageBalance
WHERE IsDeleted = 0
  AND LiveCount > 0
  AND AverageGram > 0
  AND ISNULL(BiomassGram, 0) = 0;

SELECT
    'RII_BatchWarehouseBalance' AS TargetTable,
    COUNT(*) AS RowsToRepair,
    SUM(CAST(LiveCount AS decimal(18, 3)) * AverageGram) / 1000.0 AS BiomassKgToBackfill
FROM dbo.RII_BatchWarehouseBalance
WHERE IsDeleted = 0
  AND LiveCount > 0
  AND AverageGram > 0
  AND ISNULL(BiomassGram, 0) = 0;

SELECT
    'RII_BatchMovement' AS TargetTable,
    COUNT(*) AS RowsToRepair,
    SUM(CAST(SignedCount AS decimal(18, 3)) * COALESCE(NULLIF(ToAverageGram, 0), NULLIF(FromAverageGram, 0), fb.CurrentAverageGram)) / 1000.0 AS BiomassKgToBackfill
FROM dbo.RII_BatchMovement bm
INNER JOIN dbo.RII_FishBatch fb ON fb.Id = bm.FishBatchId AND fb.IsDeleted = 0
WHERE bm.IsDeleted = 0
  AND bm.SignedCount > 0
  AND bm.MovementType IN (5, 9) -- Stocking, OpeningImport
  AND ISNULL(bm.SignedBiomassGram, 0) = 0
  AND COALESCE(NULLIF(bm.ToAverageGram, 0), NULLIF(bm.FromAverageGram, 0), fb.CurrentAverageGram) > 0;

/* =========================
   REPAIR
   ========================= */

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE b
    SET
        BiomassGram = ROUND(CAST(b.LiveCount AS decimal(18, 3)) * b.AverageGram, 3),
        UpdatedDate = GETDATE()
    FROM dbo.RII_BatchCageBalance b
    WHERE b.IsDeleted = 0
      AND b.LiveCount > 0
      AND b.AverageGram > 0
      AND ISNULL(b.BiomassGram, 0) = 0;

    DECLARE @BatchCageBalanceUpdated int = @@ROWCOUNT;

    UPDATE b
    SET
        BiomassGram = ROUND(CAST(b.LiveCount AS decimal(18, 3)) * b.AverageGram, 3),
        UpdatedDate = GETDATE()
    FROM dbo.RII_BatchWarehouseBalance b
    WHERE b.IsDeleted = 0
      AND b.LiveCount > 0
      AND b.AverageGram > 0
      AND ISNULL(b.BiomassGram, 0) = 0;

    DECLARE @BatchWarehouseBalanceUpdated int = @@ROWCOUNT;

    UPDATE bm
    SET
        SignedBiomassGram = ROUND(
            CAST(bm.SignedCount AS decimal(18, 3)) *
            COALESCE(NULLIF(bm.ToAverageGram, 0), NULLIF(bm.FromAverageGram, 0), fb.CurrentAverageGram),
            3
        ),
        ToAverageGram = COALESCE(NULLIF(bm.ToAverageGram, 0), NULLIF(bm.FromAverageGram, 0), fb.CurrentAverageGram),
        FromAverageGram = COALESCE(NULLIF(bm.FromAverageGram, 0), NULLIF(bm.ToAverageGram, 0), fb.CurrentAverageGram),
        UpdatedDate = GETDATE()
    FROM dbo.RII_BatchMovement bm
    INNER JOIN dbo.RII_FishBatch fb ON fb.Id = bm.FishBatchId AND fb.IsDeleted = 0
    WHERE bm.IsDeleted = 0
      AND bm.SignedCount > 0
      AND bm.MovementType IN (5, 9) -- Stocking, OpeningImport
      AND ISNULL(bm.SignedBiomassGram, 0) = 0
      AND COALESCE(NULLIF(bm.ToAverageGram, 0), NULLIF(bm.FromAverageGram, 0), fb.CurrentAverageGram) > 0;

    DECLARE @BatchMovementUpdated int = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @BatchCageBalanceUpdated AS BatchCageBalanceUpdated,
        @BatchWarehouseBalanceUpdated AS BatchWarehouseBalanceUpdated,
        @BatchMovementUpdated AS BatchMovementUpdated;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

/* =========================
   POST CHECK
   ========================= */

SELECT
    'RII_BatchCageBalance' AS TargetTable,
    COUNT(*) AS RemainingZeroBiomassRows
FROM dbo.RII_BatchCageBalance
WHERE IsDeleted = 0
  AND LiveCount > 0
  AND AverageGram > 0
  AND ISNULL(BiomassGram, 0) = 0;

SELECT
    'RII_BatchWarehouseBalance' AS TargetTable,
    COUNT(*) AS RemainingZeroBiomassRows
FROM dbo.RII_BatchWarehouseBalance
WHERE IsDeleted = 0
  AND LiveCount > 0
  AND AverageGram > 0
  AND ISNULL(BiomassGram, 0) = 0;

SELECT
    'RII_BatchMovement' AS TargetTable,
    COUNT(*) AS RemainingZeroBiomassRows
FROM dbo.RII_BatchMovement bm
INNER JOIN dbo.RII_FishBatch fb ON fb.Id = bm.FishBatchId AND fb.IsDeleted = 0
WHERE bm.IsDeleted = 0
  AND bm.SignedCount > 0
  AND bm.MovementType IN (5, 9)
  AND ISNULL(bm.SignedBiomassGram, 0) = 0
  AND COALESCE(NULLIF(bm.ToAverageGram, 0), NULLIF(bm.FromAverageGram, 0), fb.CurrentAverageGram) > 0;
