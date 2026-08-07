/*
    Repairs every safely reconstructable legacy opening-import shipment that
    was posted as a document but was not applied to
    RII_BATCH_MOVEMENT / RII_BATCH_CAGE_BALANCE.

    Safety:
      - @Apply = 0 previews every change and rolls back.
      - @ProjectIdFilter = NULL scans every project. A project id can be supplied
        for a targeted dry run.
      - Only rows proven to originate from Opening Import are considered. Older
        ordinary shipments may reference their header instead of their line and
        must not be treated as missing.
      - An application lock prevents two copies of this repair from overlapping;
        transaction locks protect the selected operational rows.
      - The script aborts if the current balances do not match the active ledger.
      - Missing shipments are distributed over currently available cage balances.
        Active growth biomass is first reversed to its pre-growth value.
      - Later active growth records are recalculated with the same target gram.
      - Every repaired balance must match the active ledger before commit.
      - The fish-batch current gram is synchronized to the weighted active cage
        and warehouse balance so consumers cannot read a stale opening gram.
      - Rows requiring a missing historical growth or stock movement are left
        untouched and returned in the manual-review result set.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DROP TABLE IF EXISTS #BatchAverageChange;
DROP TABLE IF EXISTS #Allocation;
DROP TABLE IF EXISTS #Weight;
DROP TABLE IF EXISTS #Skipped;
DROP TABLE IF EXISTS #Missing;
DROP TABLE IF EXISTS #BeforeBalance;
DROP TABLE IF EXISTS #TargetLine;
DROP TABLE IF EXISTS #ScopeBatch;

DECLARE @Apply bit = 0; -- Set to 1 only after reviewing the dry-run result.
DECLARE @ProjectIdFilter bigint = NULL; -- NULL repairs every safe project.
DECLARE @Now datetime2(7) = SYSUTCDATETIME();

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @ApplicationLockResult int;
    EXEC @ApplicationLockResult = sys.sp_getapplock
        @Resource = N'AQUA_LEGACY_OPENING_SHIPMENT_REPAIR',
        @LockMode = N'Exclusive',
        @LockOwner = N'Transaction',
        @LockTimeout = 0;

    IF @ApplicationLockResult < 0
        THROW 51000, 'Another opening-shipment repair is already running.', 1;

    CREATE TABLE #ScopeBatch
    (
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL PRIMARY KEY
    );

    INSERT INTO #ScopeBatch (ProjectId, FishBatchId)
    SELECT shipment.ProjectId, line.FishBatchId
    FROM RII_SHIPMENT_LINE line WITH (UPDLOCK, HOLDLOCK)
    JOIN RII_SHIPMENT shipment WITH (UPDLOCK, HOLDLOCK)
      ON shipment.Id = line.ShipmentId
    JOIN RII_FISH_BATCH batch
      ON batch.Id = line.FishBatchId
    WHERE line.IsDeleted = 0
      AND shipment.IsDeleted = 0
      AND shipment.Status = 1
      AND batch.IsDeleted = 0
      AND batch.ProjectId = shipment.ProjectId
      AND (@ProjectIdFilter IS NULL OR shipment.ProjectId = @ProjectIdFilter)
      AND
      (
          line.ErpSourceMovementKey LIKE N'OPENING_IMPORT:%'
          OR shipment.Note LIKE N'Opening import summary - %'
      )
    GROUP BY shipment.ProjectId, line.FishBatchId;

    CREATE TABLE #TargetLine
    (
        ProjectId bigint NOT NULL,
        ShipmentLineId bigint NOT NULL PRIMARY KEY
    );

    INSERT INTO #TargetLine (ProjectId, ShipmentLineId)
    SELECT
        shipment.ProjectId,
        line.Id
    FROM RII_SHIPMENT_LINE line WITH (UPDLOCK, HOLDLOCK)
    JOIN RII_SHIPMENT shipment WITH (UPDLOCK, HOLDLOCK)
      ON shipment.Id = line.ShipmentId
    JOIN RII_FISH_BATCH batch
      ON batch.Id = line.FishBatchId
    OUTER APPLY
    (
        SELECT
            -SUM(CONVERT(bigint, movement.SignedCount)) FishCount,
            -SUM(movement.SignedBiomassGram) BiomassGram
        FROM RII_BATCH_MOVEMENT movement
        WHERE movement.ReferenceId = line.Id
          AND movement.ReferenceTable IN (N'RII_SHIPMENT_LINE', N'RII_ShipmentLine')
          AND movement.MovementType = 6
          AND movement.ProjectCageId IS NOT NULL
          AND movement.IsDeleted = 0
    ) represented
    WHERE line.IsDeleted = 0
      AND shipment.IsDeleted = 0
      AND shipment.Status = 1
      AND batch.IsDeleted = 0
      AND batch.ProjectId = shipment.ProjectId
      AND (@ProjectIdFilter IS NULL OR shipment.ProjectId = @ProjectIdFilter)
      AND
      (
          line.ErpSourceMovementKey LIKE N'OPENING_IMPORT:%'
          OR shipment.Note LIKE N'Opening import summary - %'
      )
      AND
      (
          CONVERT(bigint, line.FishCount) > ISNULL(represented.FishCount, 0)
          OR line.BiomassGram > ISNULL(represented.BiomassGram, 0) + 0.001
      );

    IF EXISTS
    (
        SELECT 1
        FROM #TargetLine target
        LEFT JOIN RII_PROJECT project ON project.Id = target.ProjectId
        LEFT JOIN RII_SHIPMENT_LINE line ON line.Id = target.ShipmentLineId
        LEFT JOIN RII_SHIPMENT shipment ON shipment.Id = line.ShipmentId
        LEFT JOIN RII_FISH_BATCH batch ON batch.Id = line.FishBatchId
        WHERE project.Id IS NULL
           OR line.Id IS NULL
           OR line.IsDeleted = 1
           OR shipment.IsDeleted = 1
           OR shipment.Status <> 1
           OR shipment.ProjectId <> target.ProjectId
           OR batch.ProjectId <> target.ProjectId
    )
        THROW 51001, 'Repair target validation failed. Project or shipment line changed.', 1;

    CREATE TABLE #BeforeBalance
    (
        FishBatchId bigint NOT NULL,
        ProjectCageId bigint NOT NULL,
        LiveCount int NOT NULL,
        BiomassGram decimal(38, 6) NOT NULL,
        PRIMARY KEY (FishBatchId, ProjectCageId)
    );

    INSERT INTO #BeforeBalance (FishBatchId, ProjectCageId, LiveCount, BiomassGram)
    SELECT balance.FishBatchId, balance.ProjectCageId, balance.LiveCount, balance.BiomassGram
    FROM RII_BATCH_CAGE_BALANCE balance WITH (UPDLOCK, HOLDLOCK)
    JOIN RII_SHIPMENT_LINE line ON line.FishBatchId = balance.FishBatchId
    JOIN #TargetLine target ON target.ShipmentLineId = line.Id
    WHERE balance.IsDeleted = 0
    GROUP BY balance.FishBatchId, balance.ProjectCageId, balance.LiveCount, balance.BiomassGram;

    IF EXISTS
    (
        SELECT 1
        FROM #BeforeBalance balance
        OUTER APPLY
        (
            SELECT
                ISNULL(SUM(CONVERT(bigint, movement.SignedCount)), 0) AS LedgerCount,
                ISNULL(SUM(movement.SignedBiomassGram), 0) AS LedgerBiomassGram
            FROM RII_BATCH_MOVEMENT movement
            WHERE movement.FishBatchId = balance.FishBatchId
              AND movement.ProjectCageId = balance.ProjectCageId
              AND movement.IsDeleted = 0
        ) ledger
        WHERE CONVERT(bigint, balance.LiveCount) <> ledger.LedgerCount
           OR ABS(balance.BiomassGram - ledger.LedgerBiomassGram) > 0.001
    )
        THROW 51002, 'Current cage balance does not match the active movement ledger.', 1;

    CREATE TABLE #Missing
    (
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL,
        FishStockId bigint NULL,
        ShipmentLineId bigint NOT NULL PRIMARY KEY,
        ShipmentDate datetime2 NOT NULL,
        MissingCount int NOT NULL,
        MissingBiomassGram decimal(38, 6) NOT NULL,
        RepairBiomassGram decimal(38, 6) NULL
    );

    INSERT INTO #Missing
    (
        ProjectId,
        FishBatchId,
        FishStockId,
        ShipmentLineId,
        ShipmentDate,
        MissingCount,
        MissingBiomassGram
    )
    SELECT DISTINCT
        target.ProjectId,
        line.FishBatchId,
        batch.FishStockId,
        line.Id,
        shipment.ShipmentDate,
        CONVERT(int, line.FishCount - ISNULL(represented.FishCount, 0)),
        line.BiomassGram - ISNULL(represented.BiomassGram, 0)
    FROM #TargetLine target
    JOIN RII_SHIPMENT_LINE line ON line.Id = target.ShipmentLineId
    JOIN RII_SHIPMENT shipment ON shipment.Id = line.ShipmentId
    JOIN RII_FISH_BATCH batch ON batch.Id = line.FishBatchId
    OUTER APPLY
    (
        SELECT
            -SUM(CONVERT(bigint, movement.SignedCount)) AS FishCount,
            -SUM(movement.SignedBiomassGram) AS BiomassGram
        FROM RII_BATCH_MOVEMENT movement
        WHERE movement.ReferenceId = line.Id
          AND movement.ReferenceTable IN (N'RII_SHIPMENT_LINE', N'RII_ShipmentLine')
          AND movement.MovementType = 6
          AND movement.ProjectCageId IS NOT NULL
          AND movement.IsDeleted = 0
    ) represented;

    IF EXISTS
    (
        SELECT 1
        FROM #Missing
        WHERE MissingCount < 0
           OR (MissingCount > 0 AND MissingBiomassGram < -0.001)
    )
        THROW 51003, 'A target shipment is over-represented in the movement ledger.', 1;

    DELETE FROM #Missing
    WHERE MissingCount = 0 AND MissingBiomassGram <= 0.001;

    CREATE TABLE #Skipped
    (
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL,
        ShipmentLineId bigint NOT NULL PRIMARY KEY,
        Reason nvarchar(500) NOT NULL
    );

    CREATE TABLE #Weight
    (
        ShipmentLineId bigint NOT NULL,
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL,
        FishStockId bigint NULL,
        ShipmentDate datetime2 NOT NULL,
        ProjectCageId bigint NOT NULL,
        BaseCount bigint NOT NULL,
        BaseBiomassGram decimal(38, 6) NOT NULL,
        MissingCount int NOT NULL,
        MissingBiomassGram decimal(38, 6) NOT NULL,
        TotalBaseCount bigint NOT NULL,
        TotalBaseBiomassGram decimal(38, 6) NOT NULL,
        PRIMARY KEY (ShipmentLineId, ProjectCageId)
    );

    WITH ActiveGrowthDelta AS
    (
        SELECT
            movement.FishBatchId,
            movement.ProjectCageId,
            SUM(movement.SignedBiomassGram) AS GrowthBiomassGram
        FROM RII_BATCH_MOVEMENT movement
        WHERE movement.MovementType = 10
          AND movement.ProjectCageId IS NOT NULL
          AND movement.IsDeleted = 0
        GROUP BY movement.FishBatchId, movement.ProjectCageId
    ), CageWeight AS
    (
        SELECT
            missing.ShipmentLineId,
            missing.ProjectId,
            missing.FishBatchId,
            missing.FishStockId,
            missing.ShipmentDate,
            balance.ProjectCageId,
            CONVERT(bigint, balance.LiveCount) AS BaseCount,
            balance.BiomassGram - ISNULL(growth.GrowthBiomassGram, 0) AS BaseBiomassGram,
            missing.MissingCount,
            missing.MissingBiomassGram
        FROM #Missing missing
        JOIN #BeforeBalance balance ON balance.FishBatchId = missing.FishBatchId
        LEFT JOIN ActiveGrowthDelta growth
          ON growth.FishBatchId = balance.FishBatchId
         AND growth.ProjectCageId = balance.ProjectCageId
    ), PositiveWeight AS
    (
        SELECT *
        FROM CageWeight
        WHERE BaseCount > 0 AND BaseBiomassGram > 0
    )
    INSERT INTO #Weight
    (
        ShipmentLineId,
        ProjectId,
        FishBatchId,
        FishStockId,
        ShipmentDate,
        ProjectCageId,
        BaseCount,
        BaseBiomassGram,
        MissingCount,
        MissingBiomassGram,
        TotalBaseCount,
        TotalBaseBiomassGram
    )
    SELECT
        weight.ShipmentLineId,
        weight.ProjectId,
        weight.FishBatchId,
        weight.FishStockId,
        weight.ShipmentDate,
        weight.ProjectCageId,
        weight.BaseCount,
        weight.BaseBiomassGram,
        weight.MissingCount,
        weight.MissingBiomassGram,
        SUM(weight.BaseCount) OVER (PARTITION BY weight.ShipmentLineId),
        SUM(weight.BaseBiomassGram) OVER (PARTITION BY weight.ShipmentLineId)
    FROM PositiveWeight weight;

    ;WITH LineCapacity AS
    (
        SELECT
            missing.ProjectId,
            missing.FishBatchId,
            missing.ShipmentLineId,
            missing.MissingCount,
            missing.MissingBiomassGram,
            MAX(weight.TotalBaseCount) TotalBaseCount,
            MAX(weight.TotalBaseBiomassGram) TotalBaseBiomassGram
        FROM #Missing missing
        LEFT JOIN #Weight weight ON weight.ShipmentLineId = missing.ShipmentLineId
        GROUP BY
            missing.ProjectId,
            missing.FishBatchId,
            missing.ShipmentLineId,
            missing.MissingCount,
            missing.MissingBiomassGram
    ), UnsafeBatch AS
    (
        SELECT FishBatchId
        FROM LineCapacity
        GROUP BY FishBatchId
        HAVING MAX(CASE WHEN TotalBaseCount IS NULL THEN 1 ELSE 0 END) = 1
            OR SUM(CONVERT(bigint, MissingCount)) > MAX(TotalBaseCount)
            OR SUM(MissingBiomassGram) > MAX(TotalBaseBiomassGram) + 0.001
    )
    INSERT INTO #Skipped (ProjectId, FishBatchId, ShipmentLineId, Reason)
    SELECT
        capacity.ProjectId,
        capacity.FishBatchId,
        capacity.ShipmentLineId,
        CASE
            WHEN capacity.TotalBaseCount IS NULL
                THEN N'No positive cage balance exists for this fish batch.'
            ELSE CONCAT(
                N'The combined missing shipment exceeds reconstructable stock. Missing count=',
                batchMissing.MissingCount,
                N', available count=', capacity.TotalBaseCount,
                N', missing biomass gram=', batchMissing.MissingBiomassGram,
                N', available biomass gram=', capacity.TotalBaseBiomassGram,
                N'. A missing historical growth or stock movement must be reviewed manually.')
        END
    FROM LineCapacity capacity
    JOIN UnsafeBatch unsafe ON unsafe.FishBatchId = capacity.FishBatchId
    CROSS APPLY
    (
        SELECT
            SUM(CONVERT(bigint, other.MissingCount)) MissingCount,
            SUM(other.MissingBiomassGram) MissingBiomassGram
        FROM #Missing other
        WHERE other.FishBatchId = capacity.FishBatchId
    ) batchMissing;

    DELETE weight
    FROM #Weight weight
    JOIN #Skipped skipped ON skipped.ShipmentLineId = weight.ShipmentLineId;

    DELETE missing
    FROM #Missing missing
    JOIN #Skipped skipped ON skipped.ShipmentLineId = missing.ShipmentLineId;

    DELETE target
    FROM #TargetLine target
    JOIN #Skipped skipped ON skipped.ShipmentLineId = target.ShipmentLineId;

    UPDATE missing
    SET RepairBiomassGram =
        CASE
            WHEN missing.MissingCount = weight.TotalBaseCount
                THEN weight.TotalBaseBiomassGram
            ELSE missing.MissingBiomassGram
        END
    FROM #Missing missing
    JOIN
    (
        SELECT ShipmentLineId, MAX(TotalBaseCount) TotalBaseCount,
               MAX(TotalBaseBiomassGram) TotalBaseBiomassGram
        FROM #Weight
        GROUP BY ShipmentLineId
    ) weight ON weight.ShipmentLineId = missing.ShipmentLineId;

    CREATE TABLE #Allocation
    (
        ShipmentLineId bigint NOT NULL,
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL,
        FishStockId bigint NULL,
        ShipmentDate datetime2 NOT NULL,
        ProjectCageId bigint NOT NULL,
        AllocatedCount int NOT NULL,
        AllocatedBiomassGram decimal(38, 6) NOT NULL,
        PRIMARY KEY (ShipmentLineId, ProjectCageId)
    );

    WITH RawAllocation AS
    (
        SELECT
            weight.*,
            missing.RepairBiomassGram,
            CONVERT(bigint, FLOOR(CONVERT(decimal(38, 12), weight.MissingCount) * weight.BaseCount / weight.TotalBaseCount)) CountFloor,
            CONVERT(decimal(38, 12), weight.MissingCount) * weight.BaseCount / weight.TotalBaseCount
                - FLOOR(CONVERT(decimal(38, 12), weight.MissingCount) * weight.BaseCount / weight.TotalBaseCount) CountFraction,
            ROUND(missing.RepairBiomassGram * weight.BaseBiomassGram / weight.TotalBaseBiomassGram, 3) BiomassRounded
        FROM #Weight weight
        JOIN #Missing missing ON missing.ShipmentLineId = weight.ShipmentLineId
    ), RankedAllocation AS
    (
        SELECT
            raw.*,
            ROW_NUMBER() OVER
            (
                PARTITION BY raw.ShipmentLineId
                ORDER BY raw.CountFraction DESC, raw.ProjectCageId
            ) CountRank,
            ROW_NUMBER() OVER
            (
                PARTITION BY raw.ShipmentLineId
                ORDER BY raw.BaseBiomassGram DESC, raw.ProjectCageId
            ) BiomassRank,
            SUM(raw.CountFloor) OVER (PARTITION BY raw.ShipmentLineId) TotalCountFloor,
            SUM(raw.BiomassRounded) OVER (PARTITION BY raw.ShipmentLineId) TotalBiomassRounded
        FROM RawAllocation raw
    )
    INSERT INTO #Allocation
    (
        ShipmentLineId,
        ProjectId,
        FishBatchId,
        FishStockId,
        ShipmentDate,
        ProjectCageId,
        AllocatedCount,
        AllocatedBiomassGram
    )
    SELECT
        ranked.ShipmentLineId,
        ranked.ProjectId,
        ranked.FishBatchId,
        ranked.FishStockId,
        ranked.ShipmentDate,
        ranked.ProjectCageId,
        CONVERT(int, ranked.CountFloor +
            CASE WHEN ranked.CountRank <= ranked.MissingCount - ranked.TotalCountFloor THEN 1 ELSE 0 END),
        ranked.BiomassRounded +
            CASE WHEN ranked.BiomassRank = 1 THEN ranked.RepairBiomassGram - ranked.TotalBiomassRounded ELSE 0 END
    FROM RankedAllocation ranked;

    IF EXISTS
    (
        SELECT 1
        FROM #Missing missing
        JOIN
        (
            SELECT ShipmentLineId, SUM(CONVERT(bigint, AllocatedCount)) AllocatedCount,
                   SUM(AllocatedBiomassGram) AllocatedBiomassGram
            FROM #Allocation
            GROUP BY ShipmentLineId
        ) allocation ON allocation.ShipmentLineId = missing.ShipmentLineId
        WHERE allocation.AllocatedCount <> missing.MissingCount
           OR ABS(allocation.AllocatedBiomassGram - missing.RepairBiomassGram) > 0.001
    )
        THROW 51005, 'Shipment allocation totals do not reconcile.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT
                FishBatchId,
                ProjectCageId,
                SUM(CONVERT(bigint, AllocatedCount)) AllocatedCount,
                SUM(AllocatedBiomassGram) AllocatedBiomassGram
            FROM #Allocation
            GROUP BY FishBatchId, ProjectCageId
        ) allocation
        JOIN #BeforeBalance balance
          ON balance.FishBatchId = allocation.FishBatchId
         AND balance.ProjectCageId = allocation.ProjectCageId
        WHERE allocation.AllocatedCount > balance.LiveCount
           OR allocation.AllocatedBiomassGram > balance.BiomassGram + 0.001
    )
        THROW 51006, 'Current cage balance cannot absorb the combined shipment allocation.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM #Allocation allocation
        JOIN RII_FISH_GROWTH growth
          ON growth.FishBatchId = allocation.FishBatchId
         AND growth.ProjectCageId = allocation.ProjectCageId
         AND growth.IsDeleted = 0
        WHERE growth.GrowthDate <= allocation.ShipmentDate
    )
        THROW 51007, 'An active growth predates the missing opening shipment.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM RII_FISH_GROWTH growth
        JOIN #Allocation allocation
          ON allocation.FishBatchId = growth.FishBatchId
         AND allocation.ProjectCageId = growth.ProjectCageId
        OUTER APPLY
        (
            SELECT COUNT(*) MovementCount
            FROM RII_BATCH_MOVEMENT movement
            WHERE movement.ReferenceTable = N'RII_FISH_GROWTH'
              AND movement.ReferenceId = growth.Id
              AND movement.MovementType = 10
              AND movement.IsDeleted = 0
        ) ledger
        WHERE growth.IsDeleted = 0 AND ledger.MovementCount <> 1
    )
        THROW 51008, 'An active growth record does not have exactly one active ledger movement.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM #Allocation allocation
        CROSS APPLY
        (
            SELECT MAX(growth.GrowthDate) LastGrowthDate
            FROM RII_FISH_GROWTH growth
            WHERE growth.FishBatchId = allocation.FishBatchId
              AND growth.ProjectCageId = allocation.ProjectCageId
              AND growth.IsDeleted = 0
        ) lastGrowth
        JOIN RII_BATCH_MOVEMENT movement
          ON movement.FishBatchId = allocation.FishBatchId
         AND movement.ProjectCageId = allocation.ProjectCageId
         AND movement.IsDeleted = 0
         AND movement.MovementDate > lastGrowth.LastGrowthDate
         AND movement.MovementType <> 10
         AND (movement.SignedCount <> 0 OR movement.SignedBiomassGram <> 0)
        WHERE lastGrowth.LastGrowthDate IS NOT NULL
    )
        THROW 51009, 'A dependent movement exists after an active growth; automatic replay is unsafe.', 1;

    INSERT INTO RII_BATCH_MOVEMENT
    (
        FishBatchId,
        ProjectCageId,
        MovementDate,
        MovementType,
        SignedCount,
        SignedBiomassGram,
        ReferenceTable,
        ReferenceId,
        Note,
        CreatedDate,
        IsDeleted,
        FromAverageGram,
        FromProjectCageId,
        FromStockId,
        ToStockId
    )
    SELECT
        allocation.FishBatchId,
        allocation.ProjectCageId,
        allocation.ShipmentDate,
        6,
        -allocation.AllocatedCount,
        -allocation.AllocatedBiomassGram,
        N'RII_SHIPMENT_LINE',
        allocation.ShipmentLineId,
        CONCAT(N'Legacy opening shipment ledger repair | projectId=', allocation.ProjectId,
               N' | fromCage=', allocation.ProjectCageId,
               N' | allocation=available-balance-weighted'),
        @Now,
        0,
        CASE WHEN allocation.AllocatedCount > 0
             THEN ROUND(allocation.AllocatedBiomassGram / allocation.AllocatedCount, 3)
             ELSE NULL END,
        allocation.ProjectCageId,
        allocation.FishStockId,
        allocation.FishStockId
    FROM #Allocation allocation
    WHERE allocation.AllocatedCount <> 0 OR allocation.AllocatedBiomassGram <> 0;

    DECLARE
        @FishBatchId bigint,
        @ProjectCageId bigint,
        @AllocatedCount int,
        @AllocatedBiomassGram decimal(38, 6),
        @CorrectedCount int,
        @RunningBiomassGram decimal(38, 6),
        @GrowthId bigint,
        @TargetAverageGram decimal(38, 6),
        @PreviousAverageGram decimal(38, 6),
        @NewBiomassGram decimal(38, 6);

    DECLARE allocation_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT FishBatchId, ProjectCageId, AllocatedCount, AllocatedBiomassGram
        FROM #Allocation
        ORDER BY FishBatchId, ProjectCageId, ShipmentDate, ShipmentLineId;

    OPEN allocation_cursor;
    FETCH NEXT FROM allocation_cursor
    INTO @FishBatchId, @ProjectCageId, @AllocatedCount, @AllocatedBiomassGram;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT
            @CorrectedCount = balance.LiveCount - @AllocatedCount,
            @RunningBiomassGram = balance.BiomassGram - @AllocatedBiomassGram
        FROM RII_BATCH_CAGE_BALANCE balance
        WHERE balance.FishBatchId = @FishBatchId
          AND balance.ProjectCageId = @ProjectCageId
          AND balance.IsDeleted = 0;

        IF EXISTS
        (
            SELECT 1
            FROM RII_FISH_GROWTH growth
            WHERE growth.FishBatchId = @FishBatchId
              AND growth.ProjectCageId = @ProjectCageId
              AND growth.IsDeleted = 0
        )
        BEGIN
            SELECT TOP (1)
                @RunningBiomassGram = growth.PreviousBiomassGram - @AllocatedBiomassGram
            FROM RII_FISH_GROWTH growth
            WHERE growth.FishBatchId = @FishBatchId
              AND growth.ProjectCageId = @ProjectCageId
              AND growth.IsDeleted = 0
            ORDER BY growth.GrowthDate, growth.Id;

            DECLARE growth_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT growth.Id, growth.NewAverageGram
                FROM RII_FISH_GROWTH growth
                WHERE growth.FishBatchId = @FishBatchId
                  AND growth.ProjectCageId = @ProjectCageId
                  AND growth.IsDeleted = 0
                ORDER BY growth.GrowthDate, growth.Id;

            OPEN growth_cursor;
            FETCH NEXT FROM growth_cursor INTO @GrowthId, @TargetAverageGram;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @PreviousAverageGram = CASE WHEN @CorrectedCount > 0
                    THEN ROUND(@RunningBiomassGram / @CorrectedCount, 3)
                    ELSE 0 END;
                SET @NewBiomassGram = ROUND(@CorrectedCount * @TargetAverageGram, 3);

                UPDATE RII_FISH_GROWTH
                SET FishCount = @CorrectedCount,
                    PreviousAverageGram = @PreviousAverageGram,
                    GrowthGram = @TargetAverageGram - @PreviousAverageGram,
                    PreviousBiomassGram = @RunningBiomassGram,
                    NewBiomassGram = @NewBiomassGram,
                    UpdatedDate = @Now
                WHERE Id = @GrowthId;

                UPDATE RII_BATCH_MOVEMENT
                SET SignedCount = 0,
                    SignedBiomassGram = @NewBiomassGram - @RunningBiomassGram,
                    FromAverageGram = @PreviousAverageGram,
                    ToAverageGram = @TargetAverageGram,
                    UpdatedDate = @Now
                WHERE ReferenceTable = N'RII_FISH_GROWTH'
                  AND ReferenceId = @GrowthId
                  AND MovementType = 10
                  AND IsDeleted = 0;

                SET @RunningBiomassGram = @NewBiomassGram;
                FETCH NEXT FROM growth_cursor INTO @GrowthId, @TargetAverageGram;
            END;

            CLOSE growth_cursor;
            DEALLOCATE growth_cursor;
        END;

        UPDATE RII_BATCH_CAGE_BALANCE
        SET LiveCount = @CorrectedCount,
            BiomassGram = CASE WHEN @CorrectedCount = 0 THEN 0 ELSE @RunningBiomassGram END,
            AverageGram = CASE WHEN @CorrectedCount > 0
                THEN ROUND(@RunningBiomassGram / @CorrectedCount, 3)
                ELSE 0 END,
            UpdatedDate = @Now
        WHERE FishBatchId = @FishBatchId
          AND ProjectCageId = @ProjectCageId
          AND IsDeleted = 0;

        FETCH NEXT FROM allocation_cursor
        INTO @FishBatchId, @ProjectCageId, @AllocatedCount, @AllocatedBiomassGram;
    END;

    CLOSE allocation_cursor;
    DEALLOCATE allocation_cursor;

    IF EXISTS
    (
        SELECT 1
        FROM RII_BATCH_CAGE_BALANCE balance
        JOIN #BeforeBalance beforeBalance
          ON beforeBalance.FishBatchId = balance.FishBatchId
         AND beforeBalance.ProjectCageId = balance.ProjectCageId
        OUTER APPLY
        (
            SELECT
                ISNULL(SUM(CONVERT(bigint, movement.SignedCount)), 0) LedgerCount,
                ISNULL(SUM(movement.SignedBiomassGram), 0) LedgerBiomassGram
            FROM RII_BATCH_MOVEMENT movement
            WHERE movement.FishBatchId = balance.FishBatchId
              AND movement.ProjectCageId = balance.ProjectCageId
              AND movement.IsDeleted = 0
        ) ledger
        WHERE balance.IsDeleted = 0
          AND
          (
              balance.LiveCount < 0
              OR balance.BiomassGram < 0
              OR (balance.LiveCount = 0 AND balance.BiomassGram <> 0)
              OR CONVERT(bigint, balance.LiveCount) <> ledger.LedgerCount
              OR ABS(balance.BiomassGram - ledger.LedgerBiomassGram) > 0.001
          )
    )
        THROW 51010, 'Post-repair balance and movement ledger reconciliation failed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM #TargetLine target
        JOIN RII_SHIPMENT_LINE line ON line.Id = target.ShipmentLineId
        OUTER APPLY
        (
            SELECT
                -SUM(CONVERT(bigint, movement.SignedCount)) FishCount,
                -SUM(movement.SignedBiomassGram) BiomassGram
            FROM RII_BATCH_MOVEMENT movement
            WHERE movement.ReferenceId = line.Id
              AND movement.ReferenceTable IN (N'RII_SHIPMENT_LINE', N'RII_ShipmentLine')
              AND movement.MovementType = 6
              AND movement.ProjectCageId IS NOT NULL
              AND movement.IsDeleted = 0
        ) represented
        WHERE ISNULL(represented.FishCount, 0) < line.FishCount
           OR ISNULL(represented.BiomassGram, 0) + 0.001 < line.BiomassGram
    )
        THROW 51011, 'A repaired shipment is still not fully represented in the ledger.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT FishBatchId, SUM(CONVERT(bigint, LiveCount)) BeforeCount
            FROM #BeforeBalance
            GROUP BY FishBatchId
        ) beforeTotal
        JOIN
        (
            SELECT FishBatchId, SUM(CONVERT(bigint, AllocatedCount)) AllocatedCount
            FROM #Allocation
            GROUP BY FishBatchId
        ) allocation ON allocation.FishBatchId = beforeTotal.FishBatchId
        CROSS APPLY
        (
            SELECT SUM(CONVERT(bigint, balance.LiveCount)) AfterCount
            FROM RII_BATCH_CAGE_BALANCE balance
            WHERE balance.FishBatchId = beforeTotal.FishBatchId
              AND balance.IsDeleted = 0
        ) afterTotal
        WHERE afterTotal.AfterCount <> beforeTotal.BeforeCount - allocation.AllocatedCount
    )
        THROW 51012, 'Post-repair project fish count reconciliation failed.', 1;

    CREATE TABLE #BatchAverageChange
    (
        ProjectId bigint NOT NULL,
        FishBatchId bigint NOT NULL PRIMARY KEY,
        PreviousAverageGram decimal(18, 3) NOT NULL,
        CorrectedAverageGram decimal(18, 3) NOT NULL,
        TotalLiveCount bigint NOT NULL,
        TotalBiomassGram decimal(38, 6) NOT NULL
    );

    ;WITH ActiveBalance AS
    (
        SELECT
            balance.FishBatchId,
            CONVERT(bigint, balance.LiveCount) LiveCount,
            CONVERT(decimal(38, 6), balance.BiomassGram) BiomassGram
        FROM RII_BATCH_CAGE_BALANCE balance
        JOIN #ScopeBatch scope ON scope.FishBatchId = balance.FishBatchId
        WHERE balance.IsDeleted = 0

        UNION ALL

        SELECT
            balance.FishBatchId,
            CONVERT(bigint, balance.LiveCount),
            CONVERT(decimal(38, 6), balance.BiomassGram)
        FROM RII_BATCH_WAREHOUSE_BALANCE balance
        JOIN #ScopeBatch scope ON scope.FishBatchId = balance.FishBatchId
        WHERE balance.IsDeleted = 0
    ), BatchTotal AS
    (
        SELECT
            FishBatchId,
            SUM(LiveCount) TotalLiveCount,
            SUM(BiomassGram) TotalBiomassGram
        FROM ActiveBalance
        GROUP BY FishBatchId
    )
    INSERT INTO #BatchAverageChange
    (
        ProjectId,
        FishBatchId,
        PreviousAverageGram,
        CorrectedAverageGram,
        TotalLiveCount,
        TotalBiomassGram
    )
    SELECT
        scope.ProjectId,
        batch.Id,
        batch.CurrentAverageGram,
        corrected.CorrectedAverageGram,
        total.TotalLiveCount,
        total.TotalBiomassGram
    FROM #ScopeBatch scope
    JOIN RII_FISH_BATCH batch WITH (UPDLOCK, HOLDLOCK)
      ON batch.Id = scope.FishBatchId
    JOIN BatchTotal total ON total.FishBatchId = batch.Id
    CROSS APPLY
    (
        SELECT CONVERT(decimal(18, 3),
            ROUND(total.TotalBiomassGram / total.TotalLiveCount, 3)) CorrectedAverageGram
    ) corrected
    WHERE total.TotalLiveCount > 0
      AND total.TotalBiomassGram >= 0
      AND ABS(batch.CurrentAverageGram - corrected.CorrectedAverageGram) > 0.0005;

    UPDATE batch
    SET CurrentAverageGram = change.CorrectedAverageGram,
        UpdatedDate = @Now
    FROM RII_FISH_BATCH batch
    JOIN #BatchAverageChange change ON change.FishBatchId = batch.Id;

    SELECT
        target.ProjectId,
        project.ProjectCode,
        allocation.ShipmentLineId,
        cage.CageCode,
        allocation.AllocatedCount,
        allocation.AllocatedBiomassGram / 1000.0 AS AllocatedKg
    FROM #Allocation allocation
    JOIN #TargetLine target ON target.ShipmentLineId = allocation.ShipmentLineId
    JOIN RII_PROJECT project ON project.Id = target.ProjectId
    JOIN RII_PROJECT_CAGE projectCage ON projectCage.Id = allocation.ProjectCageId
    JOIN RII_CAGE cage ON cage.Id = projectCage.CageId
    ORDER BY target.ProjectId, allocation.ProjectCageId;

    SELECT DISTINCT
        target.ProjectId,
        project.ProjectCode,
        balance.CorrectedLiveCount,
        balance.CorrectedBiomassGram / 1000.0 AS CorrectedBiomassKg,
        movement.RepairMovementCount
    FROM #TargetLine target
    JOIN RII_PROJECT project ON project.Id = target.ProjectId
    JOIN RII_SHIPMENT_LINE line ON line.Id = target.ShipmentLineId
    JOIN RII_FISH_BATCH batch ON batch.Id = line.FishBatchId AND batch.IsDeleted = 0
    CROSS APPLY
    (
        SELECT
            SUM(CONVERT(bigint, cageBalance.LiveCount)) CorrectedLiveCount,
            SUM(cageBalance.BiomassGram) CorrectedBiomassGram
        FROM RII_BATCH_CAGE_BALANCE cageBalance
        WHERE cageBalance.FishBatchId = batch.Id
          AND cageBalance.IsDeleted = 0
    ) balance
    CROSS APPLY
    (
        SELECT COUNT(*) RepairMovementCount
        FROM RII_BATCH_MOVEMENT repairMovement
        WHERE repairMovement.FishBatchId = batch.Id
          AND repairMovement.ReferenceTable = N'RII_SHIPMENT_LINE'
          AND repairMovement.Note LIKE N'Legacy opening shipment ledger repair%'
          AND repairMovement.IsDeleted = 0
    ) movement
    ORDER BY target.ProjectId;

    SELECT
        skipped.ProjectId,
        project.ProjectCode,
        project.ProjectName,
        skipped.FishBatchId,
        batch.BatchCode,
        skipped.ShipmentLineId,
        skipped.Reason
    FROM #Skipped skipped
    JOIN RII_PROJECT project ON project.Id = skipped.ProjectId
    JOIN RII_FISH_BATCH batch ON batch.Id = skipped.FishBatchId
    ORDER BY skipped.ProjectId, skipped.ShipmentLineId;

    SELECT
        change.ProjectId,
        project.ProjectCode,
        change.FishBatchId,
        batch.BatchCode,
        change.PreviousAverageGram,
        change.CorrectedAverageGram,
        change.TotalLiveCount,
        change.TotalBiomassGram / 1000.0 AS TotalBiomassKg
    FROM #BatchAverageChange change
    JOIN RII_PROJECT project ON project.Id = change.ProjectId
    JOIN RII_FISH_BATCH batch ON batch.Id = change.FishBatchId
    ORDER BY change.ProjectId, change.FishBatchId;

    IF @Apply = 1
    BEGIN
        COMMIT TRANSACTION;
        SELECT N'COMMITTED' AS RepairStatus;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'DRY_RUN_ROLLED_BACK' AS RepairStatus;
    END;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
