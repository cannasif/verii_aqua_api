using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.FishGrowths.Application.Services;

public sealed class FishGrowthLedgerReplayService : IFishGrowthLedgerReplayService
{
    private const string FishGrowthReferenceTable = "RII_FISH_GROWTH";
    private const string ShipmentReferenceTable = "RII_SHIPMENT";
    private const string ShipmentLineReferenceTable = "RII_SHIPMENT_LINE";
    private const string MortalityReferenceTable = "RII_MORTALITY";
    private const string FeedingDistributionReferenceTable = "RII_FEEDING_DISTRIBUTION";
    private const decimal BiomassToleranceGram = 0.01m;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;

    public FishGrowthLedgerReplayService(
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
    }

    public async Task PrepareAsync(
        long projectId,
        long fishBatchId,
        long projectCageId,
        long? userId)
    {
        var fishBatch = await _unitOfWork.Db.FishBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fishBatchId && x.ProjectId == projectId && !x.IsDeleted)
            ?? throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.FishBatchNotFound"));

        var synchronization = new LedgerSynchronizationResult();
        var normalizedOpeningHistory = await NormalizeOpeningImportHistoryAsync(
            fishBatchId,
            projectCageId,
            userId);
        await SynchronizeShipmentMovementsAsync(
            fishBatch,
            projectCageId,
            userId,
            synchronization);
        await SynchronizeMortalityMovementsAsync(
            fishBatch,
            projectCageId,
            userId,
            synchronization);
        await SynchronizeFeedingMovementsAsync(
            fishBatchId,
            projectCageId,
            userId);

        if (!synchronization.HasBalanceMovementChanges && !normalizedOpeningHistory)
        {
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        if (normalizedOpeningHistory)
        {
            synchronization.TouchedCageIds.Add(projectCageId);
        }

        foreach (var cageDelta in synchronization.CageDeltas)
        {
            await EnsureReconciliationCanBeAppliedAsync(
                fishBatchId,
                cageDelta.Key,
                cageDelta.Value);
        }

        foreach (var warehouseDelta in synchronization.MissingWarehouseDeltas)
        {
            await EnsureWarehouseReconciliationCanBeAppliedAsync(
                projectId,
                fishBatchId,
                warehouseDelta.Key,
                warehouseDelta.Value);
        }

        await _unitOfWork.SaveChangesAsync();
        foreach (var cageId in synchronization.TouchedCageIds)
        {
            await RebuildCageBalanceFromLedgerAsync(
                fishBatchId,
                cageId,
                userId);
        }
        foreach (var warehouseId in synchronization.TouchedWarehouseIds)
        {
            await RebuildWarehouseBalanceAsync(
                projectId,
                fishBatchId,
                warehouseId,
                userId);
        }

        await _unitOfWork.SaveChangesAsync();
        await UpdateFishBatchAverageAsync(fishBatchId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> NormalizeOpeningImportHistoryAsync(
        long fishBatchId,
        long projectCageId,
        long? userId)
    {
        var movements = await _unitOfWork.Db.BatchMovements
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId)
            .ToListAsync();
        var historicalExits = movements
            .Where(BatchReportMassCalculator.IsOpeningImportHistoricalExit)
            .ToList();
        if (historicalExits.Count == 0)
        {
            return false;
        }

        var inventoryDeltas = BatchReportMassCalculator.CalculateMovementBiomassDeltas(movements);
        var now = DateTimeProvider.UtcNow;
        var changed = false;
        foreach (var movement in historicalExits)
        {
            if (!inventoryDeltas.TryGetValue(movement.Id, out var inventoryBiomassDelta))
            {
                continue;
            }

            var inventoryAverageGram = movement.SignedCount != 0
                ? RoundAverage(Math.Abs(inventoryBiomassDelta / movement.SignedCount))
                : 0m;
            var reportedBiomassGram = movement.ReportedBiomassGram
                ?? movement.SignedBiomassGram;
            if (movement.SignedBiomassGram == inventoryBiomassDelta
                && movement.ReportedBiomassGram == reportedBiomassGram
                && movement.FromAverageGram == inventoryAverageGram)
            {
                continue;
            }

            movement.ReportedBiomassGram = reportedBiomassGram;
            movement.SignedBiomassGram = inventoryBiomassDelta;
            movement.FromAverageGram = inventoryAverageGram;
            movement.ToAverageGram = inventoryAverageGram;
            movement.UpdatedBy = userId;
            movement.UpdatedDate = now;
            changed = true;
        }

        return changed;
    }

    public async Task<FishGrowthLedgerState> GetStateBeforeAsync(
        long fishBatchId,
        long projectCageId,
        DateTime effectiveDate)
    {
        var movements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && (x.MovementDate < effectiveDate
                    || (x.MovementDate == effectiveDate
                        && (x.MovementType == BatchMovementType.Stocking
                            || x.MovementType == BatchMovementType.OpeningImport
                            || x.MovementType == BatchMovementType.Adjustment))))
            .Select(x => new
            {
                x.SignedCount,
                x.SignedBiomassGram
            })
            .ToListAsync();

        var fishCount = movements.Sum(x => (long)x.SignedCount);
        var biomassGram = movements.Sum(x => x.SignedBiomassGram);
        EnsureNonNegativeState(fishCount, biomassGram);

        if (fishCount <= 0 || biomassGram <= 0m)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));
        }

        return new FishGrowthLedgerState(
            checked((int)fishCount),
            RoundBiomass(biomassGram),
            CalculateAverageGram(fishCount, biomassGram));
    }

    public async Task ReplayAsync(
        long projectId,
        long fishBatchId,
        long projectCageId,
        DateTime fromDate,
        long? userId)
    {
        var movements = await _unitOfWork.Db.BatchMovements
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId)
            .OrderBy(x => x.MovementDate)
            .ThenBy(x => x.MovementType == BatchMovementType.FishGrowth ? 0 : 1)
            .ThenBy(x => x.CreatedDate)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var unsupportedMovement = movements.FirstOrDefault(x =>
            x.MovementDate >= fromDate
            && x.MovementType is BatchMovementType.Transfer
                or BatchMovementType.WarehouseTransfer
                or BatchMovementType.StockConvert);
        if (unsupportedMovement != null)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.StructuralMovementReplayRequired"));
        }

        var openingMovementIds = movements
            .Where(x => IsOpeningMovementBeforeGrowth(x, fromDate))
            .Select(x => x.Id)
            .ToHashSet();
        long fishCount = movements
            .Where(x => x.MovementDate < fromDate || openingMovementIds.Contains(x.Id))
            .Sum(x => (long)x.SignedCount);
        decimal biomassGram = movements
            .Where(x => x.MovementDate < fromDate || openingMovementIds.Contains(x.Id))
            .Sum(x => x.SignedBiomassGram);
        EnsureNonNegativeState(fishCount, biomassGram);

        var growthIds = movements
            .Where(x =>
                x.MovementType == BatchMovementType.FishGrowth
                && x.ReferenceTable == FishGrowthReferenceTable)
            .Select(x => x.ReferenceId)
            .Distinct()
            .ToList();
        var growthById = growthIds.Count == 0
            ? new Dictionary<long, FishGrowth>()
            : await _unitOfWork.Db.FishGrowths
                .Where(x => !x.IsDeleted && growthIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);
        var touchedWarehouseIds = new HashSet<long>();

        foreach (var movement in movements.Where(x =>
                     x.MovementDate >= fromDate && !openingMovementIds.Contains(x.Id)))
        {
            switch (movement.MovementType)
            {
                case BatchMovementType.FishGrowth:
                    ReplayGrowthMovement(
                        movement,
                        growthById,
                        ref fishCount,
                        ref biomassGram,
                        userId);
                    break;

                case BatchMovementType.Mortality:
                    ReplayProportionalMovement(
                        movement,
                        ref fishCount,
                        ref biomassGram,
                        userId);
                    break;

                case BatchMovementType.Shipment:
                    var sourceAverageGram = CalculateAverageGram(fishCount, biomassGram);
                    ReplayProportionalMovement(
                        movement,
                        ref fishCount,
                        ref biomassGram,
                        userId);
                    await UpdateShipmentSnapshotAsync(
                        movement,
                        sourceAverageGram,
                        userId,
                        touchedWarehouseIds);
                    break;

                case BatchMovementType.Weighing:
                    ReplayWeighingMovement(
                        movement,
                        ref fishCount,
                        ref biomassGram,
                        userId);
                    break;

                case BatchMovementType.Feeding:
                    break;

                default:
                    ApplyFixedMovement(movement, ref fishCount, ref biomassGram);
                    break;
            }
        }

        await UpdateCageBalanceAsync(
            fishBatchId,
            projectCageId,
            fishCount,
            biomassGram,
            movements,
            userId);

        await _unitOfWork.SaveChangesAsync();
        foreach (var warehouseId in touchedWarehouseIds)
        {
            await RebuildWarehouseBalanceAsync(
                projectId,
                fishBatchId,
                warehouseId,
                userId);
        }

        await _unitOfWork.SaveChangesAsync();
        await UpdateFishBatchAverageAsync(fishBatchId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RebuildWarehouseAndBatchAsync(
        long projectId,
        long fishBatchId,
        long warehouseId,
        long? userId)
    {
        await RebuildWarehouseBalanceAsync(
            projectId,
            fishBatchId,
            warehouseId,
            userId);
        await _unitOfWork.SaveChangesAsync();
        await UpdateFishBatchAverageAsync(fishBatchId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SynchronizeShipmentMovementsAsync(
        FishBatch fishBatch,
        long projectCageId,
        long? userId,
        LedgerSynchronizationResult synchronization)
    {
        var lines = await _unitOfWork.Db.ShipmentLines
            .Include(x => x.Shipment)
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatch.Id
                && x.FromProjectCageId == projectCageId
                && x.Shipment != null
                && !x.Shipment.IsDeleted
                && x.Shipment.Status == DocumentStatus.Posted)
            .OrderBy(x => x.Shipment!.ShipmentDate)
            .ThenBy(x => x.Id)
            .ToListAsync();
        if (lines.Count == 0)
        {
            return;
        }

        var movements = await _unitOfWork.Db.BatchMovements
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatch.Id
                && x.MovementType == BatchMovementType.Shipment)
            .OrderBy(x => x.Id)
            .ToListAsync();
        var usedCageMovementIds = new HashSet<long>();
        var usedWarehouseMovementIds = new HashSet<long>();

        foreach (var line in lines)
        {
            var shipment = line.Shipment!;
            var isOpeningImport = IsOpeningImportShipmentLine(line);
            var cageMovements = FindShipmentMovements(
                movements,
                line,
                shipment,
                projectCageId,
                null,
                -line.FishCount,
                usedCageMovementIds);
            var inventoryAverageGram = isOpeningImport
                ? cageMovements
                    .Select(x => x.FromAverageGram)
                    .FirstOrDefault(x => x is > 0m)
                    ?? await ResolveOpeningImportAverageGramAsync(fishBatch.Id, projectCageId)
                : line.AverageGram;
            var sourceSignedBiomassGram = RoundBiomass(-line.FishCount * inventoryAverageGram);
            decimal? sourceReportedBiomassGram = isOpeningImport ? -line.BiomassGram : null;
            if (cageMovements.Count == 0)
            {
                var movement = CreateShipmentMovement(
                    fishBatch,
                    line,
                    shipment,
                    projectCageId,
                    null,
                    -line.FishCount,
                    sourceSignedBiomassGram,
                    inventoryAverageGram,
                    sourceReportedBiomassGram,
                    userId);
                await _unitOfWork.Db.BatchMovements.AddAsync(movement);
                synchronization.RegisterMovementReplacement(
                    null,
                    null,
                    0,
                    0m,
                    movement.ProjectCageId,
                    movement.WarehouseId,
                    movement.SignedCount,
                    movement.SignedBiomassGram);
            }
            else
            {
                SynchronizeShipmentMovementGroup(
                    cageMovements,
                    line,
                    shipment,
                    projectCageId,
                    null,
                    -line.FishCount,
                    sourceSignedBiomassGram,
                    inventoryAverageGram,
                    sourceReportedBiomassGram,
                    userId,
                    synchronization);
            }

            if (!shipment.TargetWarehouseId.HasValue)
            {
                continue;
            }

            var warehouseMovements = FindShipmentMovements(
                movements,
                line,
                shipment,
                null,
                shipment.TargetWarehouseId,
                line.FishCount,
                usedWarehouseMovementIds);
            if (warehouseMovements.Count == 0)
            {
                var movement = CreateShipmentMovement(
                    fishBatch,
                    line,
                    shipment,
                    null,
                    shipment.TargetWarehouseId,
                    line.FishCount,
                    -sourceSignedBiomassGram,
                    inventoryAverageGram,
                    isOpeningImport ? line.BiomassGram : null,
                    userId);
                await _unitOfWork.Db.BatchMovements.AddAsync(movement);
                synchronization.RegisterMovementReplacement(
                    null,
                    null,
                    0,
                    0m,
                    movement.ProjectCageId,
                    movement.WarehouseId,
                    movement.SignedCount,
                    movement.SignedBiomassGram);
            }
            else
            {
                SynchronizeShipmentMovementGroup(
                    warehouseMovements,
                    line,
                    shipment,
                    null,
                    shipment.TargetWarehouseId,
                    line.FishCount,
                    -sourceSignedBiomassGram,
                    inventoryAverageGram,
                    isOpeningImport ? line.BiomassGram : null,
                    userId,
                    synchronization);
            }
        }
    }

    private async Task SynchronizeMortalityMovementsAsync(
        FishBatch fishBatch,
        long projectCageId,
        long? userId,
        LedgerSynchronizationResult synchronization)
    {
        var documentGroups = await _unitOfWork.Db.MortalityLines
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatch.Id
                && x.ProjectCageId == projectCageId
                && x.Mortality != null
                && !x.Mortality.IsDeleted
                && x.Mortality.Status == DocumentStatus.Posted)
            .GroupBy(x => new
            {
                x.MortalityId,
                x.Mortality!.MortalityDate
            })
            .Select(group => new
            {
                group.Key.MortalityId,
                group.Key.MortalityDate,
                DeadCount = group.Sum(x => x.DeadCount)
            })
            .ToListAsync();

        foreach (var document in documentGroups)
        {
            var movementTotals = await _unitOfWork.Db.BatchMovements
                .Where(x =>
                    !x.IsDeleted
                    && x.FishBatchId == fishBatch.Id
                    && x.ProjectCageId == projectCageId
                    && x.MovementType == BatchMovementType.Mortality
                    && x.ReferenceTable == MortalityReferenceTable
                    && x.ReferenceId == document.MortalityId)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    SignedCount = group.Sum(x => x.SignedCount)
                })
                .FirstOrDefaultAsync();
            var desiredSignedCount = -document.DeadCount;
            var existingSignedCount = movementTotals?.SignedCount ?? 0;
            var missingSignedCount = desiredSignedCount - existingSignedCount;
            if (missingSignedCount == 0)
            {
                continue;
            }

            if (missingSignedCount > 0)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.LedgerReconciliationUnsafe"));
            }

            var averageGram = await CalculateHistoricalAverageBeforeAsync(
                fishBatch.Id,
                projectCageId,
                document.MortalityDate);
            if (averageGram <= 0m)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.LedgerReconciliationUnsafe"));
            }

            var biomassDelta = RoundBiomass(missingSignedCount * averageGram);
            var movement = new BatchMovement
            {
                FishBatchId = fishBatch.Id,
                ProjectCageId = projectCageId,
                FromProjectCageId = projectCageId,
                FromStockId = fishBatch.FishStockId,
                ToStockId = fishBatch.FishStockId,
                MovementDate = document.MortalityDate,
                MovementType = BatchMovementType.Mortality,
                SignedCount = missingSignedCount,
                SignedBiomassGram = biomassDelta,
                FromAverageGram = averageGram,
                ToAverageGram = averageGram,
                ActorUserId = userId,
                ReferenceTable = MortalityReferenceTable,
                ReferenceId = document.MortalityId,
                Note = "Fish growth ledger reconciliation | mortality",
                CreatedBy = userId,
                IsDeleted = false
            };
            await _unitOfWork.Db.BatchMovements.AddAsync(movement);
            synchronization.RegisterMovementReplacement(
                null,
                null,
                0,
                0m,
                movement.ProjectCageId,
                movement.WarehouseId,
                movement.SignedCount,
                biomassDelta);
        }
    }

    private async Task SynchronizeFeedingMovementsAsync(
        long fishBatchId,
        long projectCageId,
        long? userId)
    {
        var distributions = await _unitOfWork.Db.FeedingDistributions
            .Include(x => x.FeedingLine)
                .ThenInclude(x => x!.Feeding)
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && x.FeedingLine != null
                && !x.FeedingLine.IsDeleted
                && x.FeedingLine.Feeding != null
                && !x.FeedingLine.Feeding.IsDeleted
                && x.FeedingLine.Feeding.Status == DocumentStatus.Posted)
            .ToListAsync();

        if (distributions.Count == 0)
        {
            return;
        }

        var distributionIds = distributions.Select(x => x.Id).ToList();
        var feedingMovements = await _unitOfWork.Db.BatchMovements
            .Where(x =>
                !x.IsDeleted
                && x.MovementType == BatchMovementType.Feeding
                && x.ReferenceTable == FeedingDistributionReferenceTable
                && distributionIds.Contains(x.ReferenceId))
            .OrderByDescending(x => x.Id)
            .ToListAsync();
        var movementByDistributionId = feedingMovements
            .GroupBy(x => x.ReferenceId)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var distribution in distributions)
        {
            if (movementByDistributionId.TryGetValue(distribution.Id, out var movement))
            {
                movement.FeedGram = distribution.FeedGram;
                movement.UpdatedBy = userId;
                movement.UpdatedDate = DateTimeProvider.UtcNow;
                continue;
            }

            await _unitOfWork.Db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                MovementDate = distribution.FeedingLine!.Feeding!.FeedingDate,
                MovementType = BatchMovementType.Feeding,
                SignedCount = 0,
                SignedBiomassGram = 0m,
                FeedGram = distribution.FeedGram,
                ActorUserId = userId,
                ReferenceTable = FeedingDistributionReferenceTable,
                ReferenceId = distribution.Id,
                Note = $"FeedingDistribution | feedGram={distribution.FeedGram}",
                CreatedBy = userId,
                IsDeleted = false
            });
        }
    }

    private async Task EnsureReconciliationCanBeAppliedAsync(
        long fishBatchId,
        long projectCageId,
        BalanceDelta missingDelta)
    {
        var ledgerMovements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId)
            .Select(x => new
            {
                x.SignedCount,
                x.SignedBiomassGram
            })
            .ToListAsync();
        var balance = await _unitOfWork.Db.BatchCageBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId);
        if (balance == null)
        {
            return;
        }

        var ledgerCount = ledgerMovements.Sum(x => (long)x.SignedCount);
        var ledgerBiomass = ledgerMovements.Sum(x => x.SignedBiomassGram);
        var matchesActiveLedger = balance.LiveCount == ledgerCount
            && BiomassMatches(balance.BiomassGram, ledgerBiomass);
        var expectedCount = ledgerCount + missingDelta.SignedCount;
        var expectedBiomass = ledgerBiomass + missingDelta.SignedBiomassGram;
        var matchesReconciledLedger = balance.LiveCount == expectedCount
            && BiomassMatches(balance.BiomassGram, expectedBiomass);

        if (!matchesActiveLedger && !matchesReconciledLedger)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.LedgerReconciliationUnsafe"));
        }
    }

    private async Task EnsureWarehouseReconciliationCanBeAppliedAsync(
        long projectId,
        long fishBatchId,
        long warehouseId,
        BalanceDelta missingDelta)
    {
        var ledgerMovements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.WarehouseId == warehouseId)
            .Select(x => new { x.SignedCount, x.SignedBiomassGram })
            .ToListAsync();
        var balance = await _unitOfWork.Db.BatchWarehouseBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted
                && x.ProjectId == projectId
                && x.FishBatchId == fishBatchId
                && x.WarehouseId == warehouseId);
        if (balance == null)
        {
            return;
        }

        var ledgerCount = ledgerMovements.Sum(x => (long)x.SignedCount);
        var ledgerBiomass = ledgerMovements.Sum(x => x.SignedBiomassGram);
        var matchesActiveLedger = balance.LiveCount == ledgerCount
            && BiomassMatches(balance.BiomassGram, ledgerBiomass);
        var matchesReconciledLedger = balance.LiveCount == ledgerCount + missingDelta.SignedCount
            && BiomassMatches(
                balance.BiomassGram,
                ledgerBiomass + missingDelta.SignedBiomassGram);
        if (!matchesActiveLedger && !matchesReconciledLedger)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.LedgerReconciliationUnsafe"));
        }
    }

    private static IReadOnlyList<BatchMovement> FindShipmentMovements(
        IEnumerable<BatchMovement> movements,
        ShipmentLine line,
        Shipment shipment,
        long? projectCageId,
        long? warehouseId,
        int signedCount,
        ISet<long> usedMovementIds)
    {
        var isCageExit = projectCageId.HasValue;
        var available = movements
            .Where(x =>
                !usedMovementIds.Contains(x.Id)
                && x.FishBatchId == line.FishBatchId
                && (isCageExit
                    ? x.ProjectCageId.HasValue && !x.WarehouseId.HasValue
                    : x.WarehouseId.HasValue && !x.ProjectCageId.HasValue))
            .ToList();
        var exactLineMovements = available
            .Where(x =>
                ReferenceTableEquals(x.ReferenceTable, ShipmentLineReferenceTable)
                && x.ReferenceId == line.Id)
            .OrderByDescending(x => x.SignedCount != 0)
            .ThenBy(x => x.Id)
            .ToList();
        if (exactLineMovements.Count > 0)
        {
            foreach (var movement in exactLineMovements)
            {
                usedMovementIds.Add(movement.Id);
            }

            return exactLineMovements;
        }

        var headerMovement = available
            .Where(x =>
                x.ProjectCageId == projectCageId
                && x.WarehouseId == warehouseId
                && x.SignedCount == signedCount
                && x.MovementDate.Date == shipment.ShipmentDate.Date
                && ReferenceTableEquals(x.ReferenceTable, ShipmentReferenceTable)
                && x.ReferenceId == shipment.Id)
            .OrderBy(x => x.Id)
            .FirstOrDefault();
        if (headerMovement == null)
        {
            return [];
        }

        usedMovementIds.Add(headerMovement.Id);
        return [headerMovement];
    }

    private static BatchMovement CreateShipmentMovement(
        FishBatch fishBatch,
        ShipmentLine line,
        Shipment shipment,
        long? projectCageId,
        long? warehouseId,
        int signedCount,
        decimal signedBiomassGram,
        decimal inventoryAverageGram,
        decimal? reportedBiomassGram,
        long? userId)
    {
        var isCageExit = projectCageId.HasValue;
        return new BatchMovement
        {
            FishBatchId = fishBatch.Id,
            ProjectCageId = projectCageId,
            FromProjectCageId = isCageExit ? projectCageId : null,
            WarehouseId = warehouseId,
            ToWarehouseId = warehouseId,
            FromStockId = fishBatch.FishStockId,
            ToStockId = fishBatch.FishStockId,
            FromAverageGram = isCageExit ? inventoryAverageGram : null,
            ToAverageGram = inventoryAverageGram,
            MovementDate = shipment.ShipmentDate,
            MovementType = BatchMovementType.Shipment,
            SignedCount = signedCount,
            SignedBiomassGram = signedBiomassGram,
            ReportedBiomassGram = reportedBiomassGram,
            ActorUserId = userId,
            ReferenceTable = ShipmentLineReferenceTable,
            ReferenceId = line.Id,
            Note = "Fish growth ledger reconciliation | shipment",
            CreatedBy = userId,
            IsDeleted = false
        };
    }

    private static void SynchronizeShipmentMovementGroup(
        IReadOnlyList<BatchMovement> movements,
        ShipmentLine line,
        Shipment shipment,
        long? projectCageId,
        long? warehouseId,
        int expectedSignedCount,
        decimal expectedSignedBiomassGram,
        decimal inventoryAverageGram,
        decimal? reportedBiomassGram,
        long? userId,
        LedgerSynchronizationResult synchronization)
    {
        var now = DateTimeProvider.UtcNow;
        foreach (var movement in movements)
        {
            var oldProjectCageId = movement.ProjectCageId;
            var oldWarehouseId = movement.WarehouseId;
            var oldSignedCount = movement.SignedCount;
            var oldSignedBiomassGram = movement.SignedBiomassGram;

            movement.ProjectCageId = projectCageId;
            movement.FromProjectCageId = projectCageId;
            movement.ToProjectCageId = null;
            movement.WarehouseId = warehouseId;
            movement.FromWarehouseId = null;
            movement.ToWarehouseId = warehouseId;
            movement.MovementDate = shipment.ShipmentDate;
            movement.ReferenceTable = ShipmentLineReferenceTable;
            movement.ReferenceId = line.Id;
            movement.ActorUserId = userId;
            movement.ReportedBiomassGram = null;
            movement.UpdatedBy = userId;
            movement.UpdatedDate = now;

            synchronization.RegisterMovementReplacement(
                oldProjectCageId,
                oldWarehouseId,
                oldSignedCount,
                oldSignedBiomassGram,
                movement.ProjectCageId,
                movement.WarehouseId,
                movement.SignedCount,
                movement.SignedBiomassGram);
        }

        var primaryMovement = movements.FirstOrDefault(x => x.SignedCount != 0)
            ?? movements[0];
        var currentSignedCount = movements.Sum(x => x.SignedCount);
        var currentSignedBiomassGram = movements.Sum(x => x.SignedBiomassGram);
        var countDelta = expectedSignedCount - currentSignedCount;
        var biomassDelta = RoundBiomass(expectedSignedBiomassGram - currentSignedBiomassGram);
        var oldPrimaryCount = primaryMovement.SignedCount;
        var oldPrimaryBiomass = primaryMovement.SignedBiomassGram;
        primaryMovement.SignedCount += countDelta;
        primaryMovement.SignedBiomassGram = RoundBiomass(
            primaryMovement.SignedBiomassGram + biomassDelta);
        primaryMovement.FromAverageGram = projectCageId.HasValue ? inventoryAverageGram : null;
        primaryMovement.ToAverageGram = inventoryAverageGram;
        primaryMovement.ReportedBiomassGram = reportedBiomassGram;

        synchronization.RegisterMovementReplacement(
            primaryMovement.ProjectCageId,
            primaryMovement.WarehouseId,
            oldPrimaryCount,
            oldPrimaryBiomass,
            primaryMovement.ProjectCageId,
            primaryMovement.WarehouseId,
            primaryMovement.SignedCount,
            primaryMovement.SignedBiomassGram);
    }

    private async Task<decimal> ResolveOpeningImportAverageGramAsync(
        long fishBatchId,
        long projectCageId)
    {
        var openingMovements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && x.MovementType == BatchMovementType.OpeningImport
                && x.SignedCount > 0)
            .Select(x => new { x.SignedCount, x.SignedBiomassGram })
            .ToListAsync();
        var openingCount = openingMovements.Sum(x => (long)x.SignedCount);
        var openingBiomassGram = openingMovements.Sum(x => x.SignedBiomassGram);
        if (openingCount > 0 && openingBiomassGram > 0m)
        {
            return CalculateAverageGram(openingCount, openingBiomassGram);
        }

        var balance = await _unitOfWork.Db.BatchCageBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId);
        if (balance?.AverageGram is > 0m)
        {
            return RoundAverage(balance.AverageGram);
        }

        throw new InvalidOperationException(
            _localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));
    }

    private static bool IsOpeningImportShipmentLine(ShipmentLine line) =>
        line.ErpSourceMovementKey?.StartsWith("OPENING_IMPORT:", StringComparison.OrdinalIgnoreCase) == true;

    private static bool ReferenceTableEquals(string left, string right) =>
        string.Equals(
            NormalizeReferenceTable(left),
            NormalizeReferenceTable(right),
            StringComparison.Ordinal);

    private static string NormalizeReferenceTable(string value) =>
        new(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());

    private void ReplayGrowthMovement(
        BatchMovement movement,
        IReadOnlyDictionary<long, FishGrowth> growthById,
        ref long fishCount,
        ref decimal biomassGram,
        long? userId)
    {
        if (movement.ReferenceTable != FishGrowthReferenceTable
            || !growthById.TryGetValue(movement.ReferenceId, out var growth))
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.MovementNotFound"));
        }

        EnsureLiveState(fishCount, biomassGram);
        var previousAverageGram = CalculateAverageGram(fishCount, biomassGram);
        var targetAverageGram = RoundAverage(growth.NewAverageGram);
        if (targetAverageGram <= previousAverageGram)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.NewAverageMustExceedPrevious"));
        }

        var previousBiomassGram = RoundBiomass(biomassGram);
        var targetBiomassGram = BatchMath.CalculateBiomassGram(
            checked((int)fishCount),
            targetAverageGram);
        var now = DateTimeProvider.UtcNow;

        growth.FishCount = checked((int)fishCount);
        growth.PreviousAverageGram = previousAverageGram;
        growth.GrowthGram = RoundAverage(targetAverageGram - previousAverageGram);
        growth.PreviousBiomassGram = previousBiomassGram;
        growth.NewBiomassGram = targetBiomassGram;
        growth.UpdatedBy = userId;
        growth.UpdatedDate = now;

        movement.SignedCount = 0;
        movement.SignedBiomassGram = RoundBiomass(targetBiomassGram - previousBiomassGram);
        movement.FromAverageGram = previousAverageGram;
        movement.ToAverageGram = targetAverageGram;
        movement.ActorUserId = userId;
        movement.Note = $"Fish growth replay | projectId={growth.ProjectId}"
            + $" | fromCage={growth.ProjectCageId} | toCage={growth.ProjectCageId}"
            + $" | fromAvg={previousAverageGram:0.###} | toAvg={targetAverageGram:0.###}";
        movement.UpdatedBy = userId;
        movement.UpdatedDate = now;

        biomassGram = targetBiomassGram;
    }

    private void ReplayProportionalMovement(
        BatchMovement movement,
        ref long fishCount,
        ref decimal biomassGram,
        long? userId)
    {
        EnsureLiveState(fishCount, biomassGram);
        var averageGram = CalculateAverageGram(fishCount, biomassGram);
        if (BatchReportMassCalculator.IsOpeningImportHistoricalExit(movement)
            && !movement.ReportedBiomassGram.HasValue)
        {
            movement.ReportedBiomassGram = movement.SignedBiomassGram;
        }
        var biomassDelta = RoundBiomass(movement.SignedCount * averageGram);
        var nextCount = fishCount + movement.SignedCount;
        var nextBiomass = biomassGram + biomassDelta;
        EnsureNonNegativeState(nextCount, nextBiomass);

        movement.SignedBiomassGram = biomassDelta;
        movement.FromAverageGram = averageGram;
        movement.ToAverageGram = nextCount > 0 ? averageGram : null;
        movement.ActorUserId = userId;
        movement.UpdatedBy = userId;
        movement.UpdatedDate = DateTimeProvider.UtcNow;
        fishCount = nextCount;
        biomassGram = RoundBiomass(nextBiomass);
    }

    private void ReplayWeighingMovement(
        BatchMovement movement,
        ref long fishCount,
        ref decimal biomassGram,
        long? userId)
    {
        if (fishCount <= 0 || movement.ToAverageGram is not > 0m)
        {
            ApplyFixedMovement(movement, ref fishCount, ref biomassGram);
            return;
        }

        var previousAverageGram = CalculateAverageGram(fishCount, biomassGram);
        var measuredAverageGram = RoundAverage(movement.ToAverageGram.Value);
        var measuredBiomassGram = BatchMath.CalculateBiomassGram(
            checked((int)fishCount),
            measuredAverageGram);
        movement.SignedCount = 0;
        movement.SignedBiomassGram = RoundBiomass(measuredBiomassGram - biomassGram);
        movement.FromAverageGram = previousAverageGram;
        movement.ToAverageGram = measuredAverageGram;
        movement.ActorUserId = userId;
        movement.UpdatedBy = userId;
        movement.UpdatedDate = DateTimeProvider.UtcNow;
        biomassGram = measuredBiomassGram;
    }

    private async Task UpdateShipmentSnapshotAsync(
        BatchMovement movement,
        decimal sourceAverageGram,
        long? userId,
        ISet<long> touchedWarehouseIds)
    {
        if (movement.SignedCount >= 0)
        {
            return;
        }

        IQueryable<ShipmentLine> lineQuery = _unitOfWork.Db.ShipmentLines
            .Include(x => x.Shipment)
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == movement.FishBatchId
                && x.FromProjectCageId == movement.ProjectCageId);
        if (ReferenceTableEquals(movement.ReferenceTable, ShipmentLineReferenceTable))
        {
            lineQuery = lineQuery.Where(x => x.Id == movement.ReferenceId);
        }
        else if (ReferenceTableEquals(movement.ReferenceTable, ShipmentReferenceTable))
        {
            lineQuery = lineQuery.Where(x => x.ShipmentId == movement.ReferenceId);
        }
        else
        {
            return;
        }

        var lines = await lineQuery.ToListAsync();
        foreach (var line in lines)
        {
            if (!movement.ReportedBiomassGram.HasValue)
            {
                line.AverageGram = sourceAverageGram;
                line.BiomassGram = BatchMath.CalculateBiomassGram(
                    line.FishCount,
                    sourceAverageGram);
                ApplyShipmentPricing(line);
                line.UpdatedBy = userId;
                line.UpdatedDate = DateTimeProvider.UtcNow;
            }

            if (!line.Shipment!.TargetWarehouseId.HasValue)
            {
                continue;
            }

            var warehouseId = line.Shipment.TargetWarehouseId.Value;
            touchedWarehouseIds.Add(warehouseId);
            var warehouseMovements = await _unitOfWork.Db.BatchMovements
                .Where(x =>
                    !x.IsDeleted
                    && x.FishBatchId == line.FishBatchId
                    && x.WarehouseId == warehouseId
                    && x.MovementType == BatchMovementType.Shipment
                    && x.SignedCount > 0
                    && ((x.ReferenceTable == ShipmentLineReferenceTable && x.ReferenceId == line.Id)
                        || (x.ReferenceTable == ShipmentReferenceTable && x.ReferenceId == line.ShipmentId)))
                .ToListAsync();
            foreach (var warehouseMovement in warehouseMovements)
            {
                warehouseMovement.SignedBiomassGram = RoundBiomass(
                    warehouseMovement.SignedCount * sourceAverageGram);
                warehouseMovement.FromAverageGram = null;
                warehouseMovement.ToAverageGram = sourceAverageGram;
                warehouseMovement.ActorUserId = userId;
                warehouseMovement.UpdatedBy = userId;
                warehouseMovement.UpdatedDate = DateTimeProvider.UtcNow;
            }
        }
    }

    private async Task UpdateCageBalanceAsync(
        long fishBatchId,
        long projectCageId,
        long fishCount,
        decimal biomassGram,
        IReadOnlyCollection<BatchMovement> movements,
        long? userId)
    {
        EnsureNonNegativeState(fishCount, biomassGram);
        var balance = await _unitOfWork.Db.BatchCageBalances
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId);
        if (balance == null)
        {
            balance = new BatchCageBalance
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                CreatedBy = userId,
                IsDeleted = false
            };
            await _unitOfWork.Db.BatchCageBalances.AddAsync(balance);
        }

        balance.LiveCount = checked((int)fishCount);
        balance.BiomassGram = RoundBiomass(biomassGram);
        balance.AverageGram = fishCount > 0
            ? CalculateAverageGram(fishCount, biomassGram)
            : 0m;
        balance.AsOfDate = movements.Count == 0
            ? DateTimeProvider.Now.Date
            : movements.Max(x => x.MovementDate);
        balance.UpdatedBy = userId;
        balance.UpdatedDate = DateTimeProvider.UtcNow;
    }

    private async Task RebuildCageBalanceFromLedgerAsync(
        long fishBatchId,
        long projectCageId,
        long? userId)
    {
        var movements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId)
            .ToListAsync();
        await UpdateCageBalanceAsync(
            fishBatchId,
            projectCageId,
            movements.Sum(x => (long)x.SignedCount),
            movements.Sum(x => x.SignedBiomassGram),
            movements,
            userId);
    }

    private async Task RebuildWarehouseBalanceAsync(
        long projectId,
        long fishBatchId,
        long warehouseId,
        long? userId)
    {
        var movements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.WarehouseId == warehouseId)
            .ToListAsync();
        var fishCount = movements.Sum(x => (long)x.SignedCount);
        var biomassGram = movements.Sum(x => x.SignedBiomassGram);
        EnsureNonNegativeState(fishCount, biomassGram);

        var balance = await _unitOfWork.Db.BatchWarehouseBalances
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted
                && x.ProjectId == projectId
                && x.FishBatchId == fishBatchId
                && x.WarehouseId == warehouseId);
        if (balance == null)
        {
            balance = new BatchWarehouseBalance
            {
                ProjectId = projectId,
                FishBatchId = fishBatchId,
                WarehouseId = warehouseId,
                CreatedBy = userId,
                IsDeleted = false
            };
            await _unitOfWork.Db.BatchWarehouseBalances.AddAsync(balance);
        }

        balance.LiveCount = checked((int)fishCount);
        balance.BiomassGram = RoundBiomass(biomassGram);
        balance.AverageGram = fishCount > 0
            ? CalculateAverageGram(fishCount, biomassGram)
            : 0m;
        balance.AsOfDate = movements.Count == 0
            ? DateTimeProvider.Now.Date
            : movements.Max(x => x.MovementDate);
        balance.UpdatedBy = userId;
        balance.UpdatedDate = DateTimeProvider.UtcNow;
    }

    private async Task UpdateFishBatchAverageAsync(long fishBatchId, long? userId)
    {
        var cageBalances = await _unitOfWork.Db.BatchCageBalances
            .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId)
            .Select(x => new
            {
                x.LiveCount,
                x.BiomassGram
            })
            .ToListAsync();
        var warehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
            .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId)
            .Select(x => new
            {
                x.LiveCount,
                x.BiomassGram
            })
            .ToListAsync();
        var totalFishCount = cageBalances.Sum(x => (long)x.LiveCount)
            + warehouseBalances.Sum(x => (long)x.LiveCount);
        var totalBiomassGram = cageBalances.Sum(x => x.BiomassGram)
            + warehouseBalances.Sum(x => x.BiomassGram);

        var fishBatch = await _unitOfWork.Db.FishBatches
            .FirstOrDefaultAsync(x => x.Id == fishBatchId && !x.IsDeleted)
            ?? throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.FishBatchNotFound"));
        fishBatch.CurrentAverageGram = totalFishCount > 0
            ? CalculateAverageGram(totalFishCount, totalBiomassGram)
            : 0m;
        fishBatch.UpdatedBy = userId;
        fishBatch.UpdatedDate = DateTimeProvider.UtcNow;
    }

    private static void ApplyShipmentPricing(ShipmentLine line)
    {
        var pricing = AquaLinePricingMath.NormalizeShipmentLine(
            line.BiomassGram,
            line.CurrencyCode,
            line.ExchangeRate,
            line.UnitPrice);
        line.CurrencyCode = pricing.CurrencyCode;
        line.ExchangeRate = pricing.ExchangeRate;
        line.UnitPrice = pricing.UnitPrice;
        line.LocalUnitPrice = pricing.LocalUnitPrice;
        line.LineAmount = pricing.LineAmount;
        line.LocalLineAmount = pricing.LocalLineAmount;
    }

    private void ApplyFixedMovement(
        BatchMovement movement,
        ref long fishCount,
        ref decimal biomassGram)
    {
        var nextCount = fishCount + movement.SignedCount;
        var nextBiomass = biomassGram + movement.SignedBiomassGram;
        EnsureNonNegativeState(nextCount, nextBiomass);
        fishCount = nextCount;
        biomassGram = RoundBiomass(nextBiomass);
    }

    private void EnsureLiveState(long fishCount, decimal biomassGram)
    {
        EnsureNonNegativeState(fishCount, biomassGram);
        if (fishCount <= 0 || biomassGram <= 0m)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));
        }
    }

    private void EnsureNonNegativeState(long fishCount, decimal biomassGram)
    {
        if (fishCount < 0 || biomassGram < -BiomassToleranceGram)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.LedgerReconciliationUnsafe"));
        }
    }

    private static decimal CalculateAverageGram(long fishCount, decimal biomassGram) =>
        fishCount > 0
            ? RoundAverage(biomassGram / fishCount)
            : 0m;

    private static decimal RoundAverage(decimal value) =>
        Math.Round(value, 3, MidpointRounding.AwayFromZero);

    private static decimal RoundBiomass(decimal value) =>
        Math.Round(value, 3, MidpointRounding.AwayFromZero);

    private static bool BiomassMatches(decimal left, decimal right) =>
        Math.Abs(left - right) <= BiomassToleranceGram;

    private async Task<decimal> CalculateHistoricalAverageBeforeAsync(
        long fishBatchId,
        long projectCageId,
        DateTime movementDate)
    {
        var persistedMovements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && x.MovementDate < movementDate)
            .Select(x => new { x.SignedCount, x.SignedBiomassGram })
            .ToListAsync();
        var pendingMovements = _unitOfWork.Db.ChangeTracker
            .Entries<BatchMovement>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .Where(x =>
                !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.ProjectCageId == projectCageId
                && x.MovementDate < movementDate)
            .ToList();
        var fishCount = persistedMovements.Sum(x => (long)x.SignedCount)
            + pendingMovements.Sum(x => (long)x.SignedCount);
        var biomassGram = persistedMovements.Sum(x => x.SignedBiomassGram)
            + pendingMovements.Sum(x => x.SignedBiomassGram);
        return fishCount > 0 && biomassGram > 0m
            ? CalculateAverageGram(fishCount, biomassGram)
            : 0m;
    }

    private static bool IsOpeningMovementBeforeGrowth(
        BatchMovement movement,
        DateTime effectiveDate) =>
        movement.MovementDate == effectiveDate
        && movement.MovementType is BatchMovementType.Stocking
            or BatchMovementType.OpeningImport
            or BatchMovementType.Adjustment;

    private sealed class LedgerSynchronizationResult
    {
        public Dictionary<long, BalanceDelta> CageDeltas { get; } = [];
        public HashSet<long> TouchedCageIds { get; } = [];
        public HashSet<long> TouchedWarehouseIds { get; } = [];
        public Dictionary<long, BalanceDelta> MissingWarehouseDeltas { get; } = [];
        public bool HasBalanceMovementChanges =>
            CageDeltas.Count > 0 || MissingWarehouseDeltas.Count > 0;

        public void RegisterMovementReplacement(
            long? oldProjectCageId,
            long? oldWarehouseId,
            int oldSignedCount,
            decimal oldSignedBiomassGram,
            long? newProjectCageId,
            long? newWarehouseId,
            int newSignedCount,
            decimal newSignedBiomassGram)
        {
            if (oldProjectCageId.HasValue)
            {
                AddCageBalanceMovement(
                    oldProjectCageId.Value,
                    -oldSignedCount,
                    -oldSignedBiomassGram);
            }
            if (oldWarehouseId.HasValue)
            {
                AddWarehouseBalanceMovement(
                    oldWarehouseId.Value,
                    -oldSignedCount,
                    -oldSignedBiomassGram);
            }
            if (newProjectCageId.HasValue)
            {
                AddCageBalanceMovement(
                    newProjectCageId.Value,
                    newSignedCount,
                    newSignedBiomassGram);
            }
            if (newWarehouseId.HasValue)
            {
                AddWarehouseBalanceMovement(
                    newWarehouseId.Value,
                    newSignedCount,
                    newSignedBiomassGram);
            }
        }

        private void AddCageBalanceMovement(
            long projectCageId,
            int signedCount,
            decimal signedBiomassGram)
        {
            TouchedCageIds.Add(projectCageId);
            CageDeltas.TryGetValue(projectCageId, out var existing);
            CageDeltas[projectCageId] = new BalanceDelta(
                existing.SignedCount + signedCount,
                existing.SignedBiomassGram + signedBiomassGram);
        }

        private void AddWarehouseBalanceMovement(
            long warehouseId,
            int signedCount,
            decimal signedBiomassGram)
        {
            TouchedWarehouseIds.Add(warehouseId);
            MissingWarehouseDeltas.TryGetValue(warehouseId, out var existing);
            MissingWarehouseDeltas[warehouseId] = new BalanceDelta(
                existing.SignedCount + signedCount,
                existing.SignedBiomassGram + signedBiomassGram);
        }
    }

    private readonly record struct BalanceDelta(long SignedCount, decimal SignedBiomassGram);
}
