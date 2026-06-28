using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Hangfire;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    [AutomaticRetry(Attempts = 0, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class ErpReceiptShipmentMovementSyncJob : IErpReceiptShipmentMovementSyncJob
    {
        private const string RecurringJobId = "erp-receipt-shipment-movement-sync-job";
        private const string GoodsReceiptRefTable = "RII_GOODS_RECEIPT_LINE";
        private const string ShipmentRefTable = "RII_SHIPMENT_LINE";

        private readonly INetsisReadService _netsisReadService;
        private readonly AquaDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<ErpReceiptShipmentMovementSyncJob> _logger;

        public ErpReceiptShipmentMovementSyncJob(
            INetsisReadService netsisReadService,
            AquaDbContext db,
            IUnitOfWork unitOfWork,
            IBalanceLedgerManager balanceLedgerManager,
            ILocalizationService localizationService,
            ILogger<ErpReceiptShipmentMovementSyncJob> logger)
        {
            _netsisReadService = netsisReadService;
            _db = db;
            _unitOfWork = unitOfWork;
            _balanceLedgerManager = balanceLedgerManager;
            _localizationService = localizationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation(_localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.Started"));

            var erpResponse = await _netsisReadService.GetGoodsReceiptAndShipmentMovementsAsync(null);
            if (erpResponse == null || !erpResponse.Success)
            {
                var message = erpResponse?.ExceptionMessage
                    ?? erpResponse?.Message
                    ?? _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.ErpFetchFailed");
                await LogRecordFailureAsync("ERP_FETCH", new InvalidOperationException(message));
                _logger.LogWarning("ERP receipt/shipment operation sync aborted. Message: {Message}", message);
                return;
            }

            if (erpResponse.Data == null || erpResponse.Data.Count == 0)
            {
                _logger.LogInformation("ERP receipt/shipment operation sync skipped: no ERP records returned.");
                return;
            }

            var createdCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var failedCount = 0;
            var duplicatePayloadCount = 0;
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var erpMovement in erpResponse.Data)
            {
                var sourceMovementKey = BuildSourceMovementKey(erpMovement);
                if (string.IsNullOrWhiteSpace(erpMovement.StokKodu) || erpMovement.Tarih == default || (erpMovement.Miktar ?? 0) <= 0)
                {
                    await MarkMirrorFailureAsync(
                        erpMovement,
                        sourceMovementKey,
                        new InvalidOperationException(_localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.InvalidMovementData")));
                    skippedCount++;
                    continue;
                }

                if (!processedKeys.Add(sourceMovementKey))
                {
                    duplicatePayloadCount++;
                    continue;
                }

                if (await IsSourceMovementAlreadyProcessedAsync(sourceMovementKey))
                {
                    skippedCount++;
                    continue;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var mirrorMovement = await UpsertMirrorMovementAsync(erpMovement, sourceMovementKey);
                    var outcome = await ApplyMovementAsync(erpMovement, sourceMovementKey);
                    await EnrichMirrorMovementAsync(mirrorMovement, erpMovement, sourceMovementKey, outcome);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    if (outcome == ApplyOutcome.Created)
                    {
                        createdCount++;
                    }
                    else if (outcome == ApplyOutcome.Updated)
                    {
                        updatedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await _unitOfWork.RollbackTransactionAsync();
                    _db.ChangeTracker.Clear();
                    await MarkMirrorFailureAsync(erpMovement, sourceMovementKey, ex);
                    await LogRecordFailureAsync(sourceMovementKey, ex);
                    _db.ChangeTracker.Clear();
                }
            }

            _logger.LogInformation(
                "ERP receipt/shipment operation sync completed. created={Created}, updated={Updated}, failed={Failed}, skipped={Skipped}, duplicatePayload={DuplicatePayload}.",
                createdCount,
                updatedCount,
                failedCount,
                skippedCount,
                duplicatePayloadCount);
            _logger.LogInformation(_localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.Completed"));
        }

        private async Task<bool> IsSourceMovementAlreadyProcessedAsync(string sourceMovementKey)
        {
            return await _db.ErpReceiptShipmentMovements
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.SourceMovementKey == sourceMovementKey && x.IsProcessed);
        }

        private async Task<ApplyOutcome> ApplyMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            if (IsGoodsReceipt(movement))
            {
                return await ApplyGoodsReceiptMovementAsync(movement, sourceMovementKey);
            }

            if (IsShipment(movement))
            {
                return await ApplyShipmentMovementAsync(movement, sourceMovementKey);
            }

            return ApplyOutcome.Skipped;
        }

        private async Task<ErpReceiptShipmentMovement> UpsertMirrorMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            var now = DateTimeProvider.Now;
            var mirrorMovement = await _db.ErpReceiptShipmentMovements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SourceMovementKey == sourceMovementKey);

            if (mirrorMovement == null)
            {
                mirrorMovement = new ErpReceiptShipmentMovement
                {
                    SourceSystem = "Netsis",
                    SourceMovementKey = sourceMovementKey,
                    CreatedDate = now,
                    IsDeleted = false
                };
                await _db.ErpReceiptShipmentMovements.AddAsync(mirrorMovement);
            }

            mirrorMovement.SourceSystem = "Netsis";
            mirrorMovement.MovementDate = movement.Tarih;
            mirrorMovement.DocumentNo = OptionalShorten(movement.FisNo, 15);
            mirrorMovement.ErpWarehouseCode = movement.KafesKodu;
            mirrorMovement.ErpProjectCode = OptionalShorten(movement.ProjeKodu, 15);
            mirrorMovement.ErpStockCode = Shorten(Clean(movement.StokKodu), 35);
            mirrorMovement.ErpStockName = OptionalShorten(movement.StokAdi, 200);
            mirrorMovement.Quantity = movement.Miktar ?? 0;
            mirrorMovement.MovementKind = Shorten(Clean(movement.HareketTuru), 1);
            mirrorMovement.InOutCode = Shorten(Clean(movement.GcKodu), 1);
            mirrorMovement.StockGroupCode = OptionalShorten(movement.GrupKodu, 8);
            mirrorMovement.OperationType = Shorten(Clean(movement.IslemTuru), 50);
            mirrorMovement.LastSyncedAt = now;
            mirrorMovement.ProcessingAttemptCount += 1;
            mirrorMovement.UpdatedDate = mirrorMovement.Id > 0 ? now : null;
            mirrorMovement.IsDeleted = false;

            return mirrorMovement;
        }

        private async Task EnrichMirrorMovementAsync(
            ErpReceiptShipmentMovement mirrorMovement,
            MalKabulVeSevkiyatDto movement,
            string sourceMovementKey,
            ApplyOutcome outcome)
        {
            var now = DateTimeProvider.Now;
            var project = await ResolveProjectAsync(movement);
            var stock = await ResolveStockOrDefaultAsync(movement);
            var cage = await ResolveCageAsync(movement.KafesKodu);
            var projectCage = await ResolveProjectCageAsync(project, movement.KafesKodu);

            mirrorMovement.ProjectId = project?.Id;
            mirrorMovement.StockId = stock?.Id;
            mirrorMovement.CageId = cage?.Id;
            mirrorMovement.ProjectCageId = projectCage?.Id;
            mirrorMovement.StockGroupCode = OptionalShorten(movement.GrupKodu, 8)
                ?? OptionalShorten(stock?.GrupKodu, 8);
            mirrorMovement.GoodsReceiptId = null;
            mirrorMovement.GoodsReceiptLineId = null;
            mirrorMovement.ShipmentId = null;
            mirrorMovement.ShipmentLineId = null;
            mirrorMovement.BatchMovementId = null;
            mirrorMovement.FishBatchId = null;

            var goodsReceiptLine = await _db.GoodsReceiptLines
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpSourceMovementKey == sourceMovementKey);

            if (goodsReceiptLine != null)
            {
                mirrorMovement.GoodsReceiptLineId = goodsReceiptLine.Id;
                mirrorMovement.GoodsReceiptId = goodsReceiptLine.GoodsReceiptId;
                mirrorMovement.FishBatchId = goodsReceiptLine.FishBatchId;
            }

            var shipmentLine = await _db.ShipmentLines
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpSourceMovementKey == sourceMovementKey);

            if (shipmentLine != null)
            {
                mirrorMovement.ShipmentLineId = shipmentLine.Id;
                mirrorMovement.ShipmentId = shipmentLine.ShipmentId;
                mirrorMovement.FishBatchId = shipmentLine.FishBatchId;
            }

            var lineId = mirrorMovement.GoodsReceiptLineId ?? mirrorMovement.ShipmentLineId;
            var refTable = mirrorMovement.GoodsReceiptLineId.HasValue ? GoodsReceiptRefTable : ShipmentRefTable;
            if (lineId.HasValue)
            {
                mirrorMovement.BatchMovementId = await _db.BatchMovements
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.ReferenceTable == refTable && x.ReferenceId == lineId.Value)
                    .OrderByDescending(x => x.Id)
                    .Select(x => (long?)x.Id)
                    .FirstOrDefaultAsync();
            }

            mirrorMovement.IsMatched =
                mirrorMovement.ProjectId.HasValue ||
                mirrorMovement.CageId.HasValue ||
                mirrorMovement.StockId.HasValue ||
                mirrorMovement.GoodsReceiptLineId.HasValue ||
                mirrorMovement.ShipmentLineId.HasValue;
            mirrorMovement.IsProcessed = mirrorMovement.GoodsReceiptLineId.HasValue || mirrorMovement.ShipmentLineId.HasValue;
            mirrorMovement.MatchedAt = mirrorMovement.IsMatched ? now : null;
            mirrorMovement.ProcessedAt = mirrorMovement.IsProcessed ? now : null;
            mirrorMovement.MatchError = mirrorMovement.IsMatched
                ? null
                : _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.MatchFailed");
            mirrorMovement.ProcessError = outcome == ApplyOutcome.Skipped && !mirrorMovement.IsProcessed
                ? _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.UnsupportedOperationType")
                : null;
            mirrorMovement.UpdatedDate = now;
        }

        private async Task MarkMirrorFailureAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey, Exception ex)
        {
            try
            {
                var mirrorMovement = await UpsertMirrorMovementAsync(movement, sourceMovementKey);
                mirrorMovement.IsProcessed = false;
                mirrorMovement.ProcessedAt = null;
                mirrorMovement.ProcessError = Shorten(BuildDiagnosticMessage(ex), 2000);
                mirrorMovement.MatchError ??= _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.MatchOrProcessFailed");
                mirrorMovement.UpdatedDate = DateTimeProvider.Now;
                await _db.SaveChangesAsync();
            }
            catch (Exception mirrorEx)
            {
                _logger.LogWarning(mirrorEx, "ERP receipt/shipment movement mirror could not be updated. SourceMovementKey: {SourceMovementKey}", sourceMovementKey);
            }
        }

        private async Task<ApplyOutcome> ApplyGoodsReceiptMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            var stock = await ResolveStockAsync(movement);
            var warehouse = await ResolveWarehouseAsync(movement.KafesKodu);
            var isFeedReceipt = IsFeedReceipt(movement, stock);
            var project = isFeedReceipt
                ? await ResolveProjectAsync(movement)
                : await ResolveOrCreateProjectAsync(movement);
            var projectCage = isFeedReceipt
                ? await ResolveProjectCageAsync(project, movement.KafesKodu)
                : await ResolveOrCreateProjectCageAsync(project, movement.KafesKodu);
            var receiptNo = BuildDocumentNo("ERP-GR", movement);
            var now = DateTimeProvider.Now;

            var receipt = await _db.GoodsReceipts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ReceiptNo == receiptNo);

            if (receipt == null)
            {
                receipt = new GoodsReceipt
                {
                    ProjectId = project?.Id,
                    ReceiptNo = receiptNo,
                    ReceiptDate = movement.Tarih,
                    Status = DocumentStatus.Posted,
                    WarehouseId = warehouse?.Id,
                    Note = BuildHeaderNote(movement),
                    CreatedDate = now,
                    IsDeleted = false
                };
                await _db.GoodsReceipts.AddAsync(receipt);
                await _db.SaveChangesAsync();
            }
            else
            {
                if (!receipt.ProjectId.HasValue && project != null) receipt.ProjectId = project.Id;
                if (!receipt.WarehouseId.HasValue && warehouse != null) receipt.WarehouseId = warehouse.Id;
                receipt.UpdatedDate = now;
            }

            return isFeedReceipt
                ? await UpsertFeedReceiptLineAsync(receipt, stock, movement, sourceMovementKey)
                : await UpsertFishReceiptLineAsync(receipt, stock, project, projectCage, warehouse, movement, sourceMovementKey);
        }

        private async Task<ApplyOutcome> UpsertFeedReceiptLineAsync(
            GoodsReceipt receipt,
            StockEntity stock,
            MalKabulVeSevkiyatDto movement,
            string sourceMovementKey)
        {
            var quantityKg = movement.Miktar ?? 0;
            var totalGram = Math.Round(quantityKg * 1000m, 3, MidpointRounding.AwayFromZero);
            var existingLine = await _db.GoodsReceiptLines
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpSourceMovementKey == sourceMovementKey);

            if (existingLine == null)
            {
                await _db.GoodsReceiptLines.AddAsync(new GoodsReceiptLine
                {
                    GoodsReceiptId = receipt.Id,
                    ItemType = GoodsReceiptItemType.Feed,
                    StockId = stock.Id,
                    QtyUnit = quantityKg,
                    GramPerUnit = 1000m,
                    TotalGram = totalGram,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    ErpSourceMovementKey = sourceMovementKey,
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                });
                return ApplyOutcome.Created;
            }

            var changed = existingLine.StockId != stock.Id
                || existingLine.QtyUnit != quantityKg
                || existingLine.TotalGram != totalGram;

            existingLine.StockId = stock.Id;
            existingLine.QtyUnit = quantityKg;
            existingLine.GramPerUnit = 1000m;
            existingLine.TotalGram = totalGram;
            existingLine.UpdatedDate = DateTimeProvider.Now;

            return changed ? ApplyOutcome.Updated : ApplyOutcome.Skipped;
        }

        private async Task<ApplyOutcome> UpsertFishReceiptLineAsync(
            GoodsReceipt receipt,
            StockEntity stock,
            Project? project,
            ProjectCage? projectCage,
            WarehouseEntity? warehouse,
            MalKabulVeSevkiyatDto movement,
            string sourceMovementKey)
        {
            if (project == null)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.FishReceiptProjectNotMatched",
                    Clean(movement.ProjeKodu)));
            }

            if (projectCage == null && warehouse == null)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.FishReceiptCageNotMatched",
                    movement.KafesKodu?.ToString(CultureInfo.InvariantCulture) ?? "null",
                    Clean(movement.ProjeKodu)));
            }

            var fishCount = ResolveCount(movement);
            var fishBatch = await ResolveOrCreateFishBatchAsync(project, stock, receipt, movement, projectCage, warehouse);
            var averageGram = await ResolveAverageGramAsync(fishBatch, projectCage, warehouse);
            var biomassGram = Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);
            var existingLine = await _db.GoodsReceiptLines
                .Include(x => x.FishDistributions)
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpSourceMovementKey == sourceMovementKey);

            if (existingLine == null)
            {
                var line = new GoodsReceiptLine
                {
                    GoodsReceiptId = receipt.Id,
                    ItemType = GoodsReceiptItemType.Fish,
                    StockId = stock.Id,
                    FishCount = fishCount,
                    FishAverageGram = averageGram,
                    FishTotalGram = biomassGram,
                    FishBatchId = fishBatch.Id,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    ErpSourceMovementKey = sourceMovementKey,
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                };
                await _db.GoodsReceiptLines.AddAsync(line);
                await _db.SaveChangesAsync();

                if (projectCage != null)
                {
                    await _db.GoodsReceiptFishDistributions.AddAsync(new GoodsReceiptFishDistribution
                    {
                        GoodsReceiptLineId = line.Id,
                        ProjectCageId = projectCage.Id,
                        FishBatchId = fishBatch.Id,
                        FishCount = fishCount,
                        CreatedDate = DateTimeProvider.Now,
                        IsDeleted = false
                    });

                    await _balanceLedgerManager.ApplyDelta(
                        project.Id,
                        fishBatch.Id,
                        projectCage.Id,
                        fishCount,
                        biomassGram,
                        BatchMovementType.Stocking,
                        movement.Tarih,
                        "ERP fish receipt",
                        GoodsReceiptRefTable,
                        line.Id,
                        null,
                        projectCage.Id,
                        null,
                        stock.Id,
                        null,
                        averageGram);
                }
                else if (warehouse != null)
                {
                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        project.Id,
                        fishBatch.Id,
                        warehouse.Id,
                        fishCount,
                        biomassGram,
                        BatchMovementType.Stocking,
                        movement.Tarih,
                        "ERP fish warehouse receipt",
                        GoodsReceiptRefTable,
                        line.Id,
                        null,
                        warehouse.Id,
                        null,
                        stock.Id,
                        null,
                        averageGram);
                }

                return ApplyOutcome.Created;
            }

            var oldCount = existingLine.FishCount ?? 0;
            var oldBiomass = existingLine.FishTotalGram ?? 0;
            var deltaCount = fishCount - oldCount;
            var deltaBiomass = biomassGram - oldBiomass;

            existingLine.StockId = stock.Id;
            existingLine.FishBatchId = fishBatch.Id;
            existingLine.FishCount = fishCount;
            existingLine.FishAverageGram = averageGram;
            existingLine.FishTotalGram = biomassGram;
            existingLine.UpdatedDate = DateTimeProvider.Now;

            if (projectCage != null)
            {
                var distribution = existingLine.FishDistributions.FirstOrDefault(x => !x.IsDeleted && x.ProjectCageId == projectCage.Id);
                if (distribution == null)
                {
                    await _db.GoodsReceiptFishDistributions.AddAsync(new GoodsReceiptFishDistribution
                    {
                        GoodsReceiptLineId = existingLine.Id,
                        ProjectCageId = projectCage.Id,
                        FishBatchId = fishBatch.Id,
                        FishCount = fishCount,
                        CreatedDate = DateTimeProvider.Now,
                        IsDeleted = false
                    });
                }
                else
                {
                    distribution.FishBatchId = fishBatch.Id;
                    distribution.FishCount = fishCount;
                    distribution.UpdatedDate = DateTimeProvider.Now;
                }
            }

            if (deltaCount != 0 || deltaBiomass != 0)
            {
                if (projectCage != null)
                {
                    await _balanceLedgerManager.ApplyDelta(
                        project.Id,
                        fishBatch.Id,
                        projectCage.Id,
                        deltaCount,
                        deltaBiomass,
                        BatchMovementType.Stocking,
                        movement.Tarih,
                        "ERP fish receipt delta",
                        GoodsReceiptRefTable,
                        existingLine.Id,
                        null,
                        projectCage.Id,
                        null,
                        stock.Id,
                        null,
                        averageGram);
                }
                else if (warehouse != null)
                {
                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        project.Id,
                        fishBatch.Id,
                        warehouse.Id,
                        deltaCount,
                        deltaBiomass,
                        BatchMovementType.Stocking,
                        movement.Tarih,
                        "ERP fish warehouse receipt delta",
                        GoodsReceiptRefTable,
                        existingLine.Id,
                        null,
                        warehouse.Id,
                        null,
                        stock.Id,
                        null,
                        averageGram);
                }

                return ApplyOutcome.Updated;
            }

            return ApplyOutcome.Skipped;
        }

        private async Task<ApplyOutcome> ApplyShipmentMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            var project = await ResolveProjectAsync(movement)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.ShipmentProjectNotMatched",
                    Clean(movement.ProjeKodu)));
            var stock = await ResolveStockAsync(movement);
            var projectCage = await ResolveProjectCageAsync(project, movement.KafesKodu)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.ShipmentCageNotMatched",
                    movement.KafesKodu?.ToString(CultureInfo.InvariantCulture) ?? "null",
                    Clean(movement.ProjeKodu)));

            var fishCount = ResolveCount(movement);
            var activeBalance = await ResolveShipmentBalanceAsync(projectCage.Id, stock.Id);
            if (activeBalance == null)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.ShipmentLiveBalanceNotFound",
                    projectCage.Id,
                    stock.Id));
            }

            var averageGram = activeBalance.AverageGram > 0 ? activeBalance.AverageGram : activeBalance.FishBatch?.CurrentAverageGram ?? 0;
            var biomassGram = Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);
            var shipmentNo = BuildDocumentNo("ERP-SH", movement);
            var now = DateTimeProvider.Now;

            var shipment = await _db.Shipments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ShipmentNo == shipmentNo);

            if (shipment == null)
            {
                shipment = new Shipment
                {
                    ProjectId = project.Id,
                    ShipmentNo = shipmentNo,
                    ShipmentDate = movement.Tarih,
                    Status = DocumentStatus.Posted,
                    Note = BuildHeaderNote(movement),
                    CreatedDate = now,
                    IsDeleted = false
                };
                await _db.Shipments.AddAsync(shipment);
                await _db.SaveChangesAsync();
            }
            else
            {
                shipment.ProjectId = project.Id;
                shipment.UpdatedDate = now;
            }

            var existingLine = await _db.ShipmentLines
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpSourceMovementKey == sourceMovementKey);

            if (existingLine == null)
            {
                var line = new ShipmentLine
                {
                    ShipmentId = shipment.Id,
                    FishBatchId = activeBalance.FishBatchId,
                    FromProjectCageId = projectCage.Id,
                    FishCount = fishCount,
                    AverageGram = averageGram,
                    BiomassGram = biomassGram,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    ErpSourceMovementKey = sourceMovementKey,
                    CreatedDate = now,
                    IsDeleted = false
                };
                await _db.ShipmentLines.AddAsync(line);
                await _db.SaveChangesAsync();

                await _balanceLedgerManager.ApplyDelta(
                    project.Id,
                    activeBalance.FishBatchId,
                    projectCage.Id,
                    -fishCount,
                    -biomassGram,
                    BatchMovementType.Shipment,
                    movement.Tarih,
                    "ERP shipment",
                    ShipmentRefTable,
                    line.Id,
                    projectCage.Id,
                    null,
                    stock.Id,
                    null,
                    averageGram,
                    null);

                return ApplyOutcome.Created;
            }

            var deltaCount = fishCount - existingLine.FishCount;
            var deltaBiomass = biomassGram - existingLine.BiomassGram;

            existingLine.FishBatchId = activeBalance.FishBatchId;
            existingLine.FromProjectCageId = projectCage.Id;
            existingLine.FishCount = fishCount;
            existingLine.AverageGram = averageGram;
            existingLine.BiomassGram = biomassGram;
            existingLine.UpdatedDate = now;

            if (deltaCount != 0 || deltaBiomass != 0)
            {
                await _balanceLedgerManager.ApplyDelta(
                    project.Id,
                    activeBalance.FishBatchId,
                    projectCage.Id,
                    -deltaCount,
                    -deltaBiomass,
                    BatchMovementType.Shipment,
                    movement.Tarih,
                    "ERP shipment delta",
                    ShipmentRefTable,
                    existingLine.Id,
                    projectCage.Id,
                    null,
                    stock.Id,
                    null,
                    averageGram,
                    null);

                return ApplyOutcome.Updated;
            }

            return ApplyOutcome.Skipped;
        }

        private async Task<Project?> ResolveProjectAsync(MalKabulVeSevkiyatDto movement)
        {
            var projectCode = Clean(movement.ProjeKodu);
            return string.IsNullOrWhiteSpace(projectCode)
                ? null
                : await _db.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectCode == projectCode);
        }

        private async Task<Project?> ResolveOrCreateProjectAsync(MalKabulVeSevkiyatDto movement)
        {
            var projectCode = Clean(movement.ProjeKodu);
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return null;
            }

            var existingProject = await _db.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectCode == projectCode);

            if (existingProject != null)
            {
                return existingProject;
            }

            var project = new Project
            {
                ProjectCode = Shorten(projectCode, 50),
                ProjectName = Shorten(projectCode, 200),
                StartDate = movement.Tarih.Date,
                Status = DocumentStatus.Posted,
                Note = Shorten($"ERP hareketinden otomatik oluşturuldu. Fiş={Clean(movement.FisNo)}", 500),
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _db.Projects.AddAsync(project);
            await _db.SaveChangesAsync();
            return project;
        }

        private async Task<StockEntity> ResolveStockAsync(MalKabulVeSevkiyatDto movement)
        {
            var stockCode = Clean(movement.StokKodu);
            return await _db.Stocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpStockCode == stockCode)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString(
                    "ErpReceiptShipmentMovementSyncJob.StockNotMatched",
                    stockCode));
        }

        private async Task<StockEntity?> ResolveStockOrDefaultAsync(MalKabulVeSevkiyatDto movement)
        {
            var stockCode = Clean(movement.StokKodu);
            return string.IsNullOrWhiteSpace(stockCode)
                ? null
                : await _db.Stocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpStockCode == stockCode);
        }

        private async Task<ProjectCage?> ResolveProjectCageAsync(Project? project, short? erpWarehouseCode)
        {
            if (project == null)
            {
                return null;
            }

            var cage = await ResolveCageAsync(erpWarehouseCode);
            return cage == null
                ? null
                : await _db.ProjectCages
                    .IgnoreQueryFilters()
                    .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id)
                    .OrderByDescending(x => x.AssignedDate)
                    .FirstOrDefaultAsync();
        }

        private async Task<ProjectCage?> ResolveOrCreateProjectCageAsync(Project? project, short? erpWarehouseCode)
        {
            if (project == null)
            {
                return null;
            }

            var existingProjectCage = await ResolveProjectCageAsync(project, erpWarehouseCode);
            if (existingProjectCage != null)
            {
                return existingProjectCage;
            }

            var cage = await ResolveCageAsync(erpWarehouseCode);
            if (cage == null)
            {
                return null;
            }

            var activeAssignment = await _db.ProjectCages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ReleasedDate == null && x.CageId == cage.Id);

            if (activeAssignment != null)
            {
                return null;
            }

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _db.ProjectCages.AddAsync(projectCage);
            await _db.SaveChangesAsync();
            return projectCage;
        }

        private async Task<Cage?> ResolveCageAsync(short? erpWarehouseCode)
        {
            if (!erpWarehouseCode.HasValue)
            {
                return null;
            }

            var warehouse = await ResolveWarehouseAsync(erpWarehouseCode);
            if (warehouse != null)
            {
                var mapping = await _db.CageWarehouseMappings
                    .IgnoreQueryFilters()
                    .Include(x => x.Cage)
                    .Where(x => !x.IsDeleted && x.IsActive && x.WarehouseId == warehouse.Id && x.Cage != null && !x.Cage.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (mapping?.Cage != null)
                {
                    return mapping.Cage;
                }

                var cageByWarehouseName = await ResolveCageFromWarehouseNameAsync(warehouse.WarehouseName);
                if (cageByWarehouseName != null)
                {
                    await EnsureCageWarehouseMappingAsync(cageByWarehouseName, warehouse);
                    return cageByWarehouseName;
                }
            }

            var code = erpWarehouseCode.Value.ToString(CultureInfo.InvariantCulture);
            return await _db.Cages.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.CageCode == code);
        }

        private async Task<Cage?> ResolveCageFromWarehouseNameAsync(string? warehouseName)
        {
            var normalizedWarehouseName = Clean(warehouseName);
            if (string.IsNullOrWhiteSpace(normalizedWarehouseName))
            {
                return null;
            }

            var cages = await _db.Cages
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            return cages
                .Where(x => !string.IsNullOrWhiteSpace(x.CageCode))
                .OrderByDescending(x => x.CageCode.Length)
                .FirstOrDefault(x => WarehouseNameMatchesCageCode(normalizedWarehouseName, Clean(x.CageCode)));
        }

        private static bool WarehouseNameMatchesCageCode(string warehouseName, string cageCode)
        {
            if (string.IsNullOrWhiteSpace(warehouseName) || string.IsNullOrWhiteSpace(cageCode))
            {
                return false;
            }

            return warehouseName.Equals(cageCode, StringComparison.OrdinalIgnoreCase) ||
                   warehouseName.StartsWith($"{cageCode} ", StringComparison.OrdinalIgnoreCase) ||
                   warehouseName.StartsWith($"{cageCode}-", StringComparison.OrdinalIgnoreCase) ||
                   warehouseName.StartsWith($"{cageCode}_", StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnsureCageWarehouseMappingAsync(Cage cage, WarehouseEntity warehouse)
        {
            var activeMapping = await _db.CageWarehouseMappings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.IsActive && x.CageId == cage.Id);

            if (activeMapping == null)
            {
                await _db.CageWarehouseMappings.AddAsync(new CageWarehouseMapping
                {
                    CageId = cage.Id,
                    WarehouseId = warehouse.Id,
                    IsActive = true,
                    Note = _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.CageWarehouseMappingAutoRepaired"),
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                });
                return;
            }

            if (activeMapping.WarehouseId == warehouse.Id)
            {
                return;
            }

            activeMapping.WarehouseId = warehouse.Id;
            activeMapping.Note = _localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.CageWarehouseMappingAutoRepaired");
            activeMapping.UpdatedDate = DateTimeProvider.Now;
        }

        private async Task<WarehouseEntity?> ResolveWarehouseAsync(short? erpWarehouseCode)
        {
            return erpWarehouseCode.HasValue
                ? await _db.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpWarehouseCode == erpWarehouseCode.Value)
                : null;
        }

        private async Task<FishBatch> ResolveOrCreateFishBatchAsync(
            Project project,
            StockEntity stock,
            GoodsReceipt receipt,
            MalKabulVeSevkiyatDto movement,
            ProjectCage? projectCage,
            WarehouseEntity? warehouse)
        {
            var batchCode = Shorten(receipt.ReceiptNo, 50);
            var existingBatch = await _db.FishBatches
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.BatchCode == batchCode)
                .FirstOrDefaultAsync();

            if (existingBatch != null)
            {
                return existingBatch;
            }

            existingBatch = await _db.FishBatches
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishStockId == stock.Id)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            if (existingBatch != null)
            {
                return existingBatch;
            }

            var initialAverageGram = await ResolveInitialAverageGramAsync(project.Id, stock.Id, projectCage, warehouse);
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = batchCode,
                FishStockId = stock.Id,
                CurrentAverageGram = initialAverageGram,
                StartDate = movement.Tarih,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _db.FishBatches.AddAsync(batch);
            await _db.SaveChangesAsync();
            return batch;
        }

        private async Task<decimal> ResolveInitialAverageGramAsync(
            long projectId,
            long stockId,
            ProjectCage? projectCage,
            WarehouseEntity? warehouse)
        {
            if (projectCage != null)
            {
                var cageAverageGram = await _db.BatchCageBalances
                    .AsNoTracking()
                    .Include(x => x.FishBatch)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectCageId == projectCage.Id &&
                        x.AverageGram > 0 &&
                        x.FishBatch != null &&
                        !x.FishBatch.IsDeleted &&
                        x.FishBatch.ProjectId == projectId &&
                        x.FishBatch.FishStockId == stockId)
                    .OrderByDescending(x => x.AsOfDate)
                    .Select(x => (decimal?)x.AverageGram)
                    .FirstOrDefaultAsync();

                if (cageAverageGram.GetValueOrDefault() > 0)
                {
                    return cageAverageGram.Value;
                }
            }

            if (warehouse != null)
            {
                var warehouseAverageGram = await _db.BatchWarehouseBalances
                    .AsNoTracking()
                    .Include(x => x.FishBatch)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == projectId &&
                        x.WarehouseId == warehouse.Id &&
                        x.AverageGram > 0 &&
                        x.FishBatch != null &&
                        !x.FishBatch.IsDeleted &&
                        x.FishBatch.FishStockId == stockId)
                    .OrderByDescending(x => x.AsOfDate)
                    .Select(x => (decimal?)x.AverageGram)
                    .FirstOrDefaultAsync();

                if (warehouseAverageGram.GetValueOrDefault() > 0)
                {
                    return warehouseAverageGram.Value;
                }
            }

            var projectAverageGram = await _db.FishBatches
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.ProjectId == projectId &&
                    x.FishStockId == stockId &&
                    x.CurrentAverageGram > 0)
                .OrderByDescending(x => x.StartDate)
                .Select(x => (decimal?)x.CurrentAverageGram)
                .FirstOrDefaultAsync();

            return projectAverageGram.GetValueOrDefault() > 0 ? projectAverageGram.Value : 1m;
        }

        private async Task<decimal> ResolveAverageGramAsync(FishBatch fishBatch, ProjectCage? projectCage, WarehouseEntity? warehouse)
        {
            if (fishBatch.CurrentAverageGram > 0)
            {
                return fishBatch.CurrentAverageGram;
            }

            if (projectCage != null)
            {
                var latestCageBalance = await _db.BatchCageBalances
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.FishBatchId == fishBatch.Id && x.ProjectCageId == projectCage.Id && x.AverageGram > 0)
                    .OrderByDescending(x => x.AsOfDate)
                    .FirstOrDefaultAsync();

                if (latestCageBalance != null)
                {
                    return latestCageBalance.AverageGram;
                }
            }

            if (warehouse != null)
            {
                var latestWarehouseBalance = await _db.BatchWarehouseBalances
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.FishBatchId == fishBatch.Id && x.WarehouseId == warehouse.Id && x.AverageGram > 0)
                    .OrderByDescending(x => x.AsOfDate)
                    .FirstOrDefaultAsync();

                if (latestWarehouseBalance != null)
                {
                    return latestWarehouseBalance.AverageGram;
                }
            }

            return 0;
        }

        private async Task<BatchCageBalance?> ResolveShipmentBalanceAsync(long projectCageId, long stockId)
        {
            return await _db.BatchCageBalances
                .Include(x => x.FishBatch)
                .Where(x =>
                    !x.IsDeleted &&
                    x.ProjectCageId == projectCageId &&
                    x.LiveCount > 0 &&
                    x.FishBatch != null &&
                    !x.FishBatch.IsDeleted &&
                    x.FishBatch.FishStockId == stockId)
                .OrderByDescending(x => x.AsOfDate)
                .FirstOrDefaultAsync();
        }

        private async Task LogRecordFailureAsync(string key, Exception ex)
        {
            _logger.LogError(ex, "ERP receipt/shipment operation sync record failed. SourceMovementKey: {SourceMovementKey}", key);

            try
            {
                _db.JobFailureLogs.Add(new JobFailureLog
                {
                    JobId = $"{RecurringJobId}:{key}:{DateTimeProvider.Now:yyyyMMddHHmmssfff}",
                    JobName = $"{typeof(ErpReceiptShipmentMovementSyncJob).FullName}.ExecuteAsync",
                    FailedAt = DateTimeProvider.Now,
                    Reason = $"SourceMovementKey={key}",
                    ExceptionType = ex.GetType().FullName,
                    ExceptionMessage = BuildDiagnosticMessage(ex),
                    StackTrace = ex.StackTrace?.Length > 8000 ? ex.StackTrace[..8000] : ex.StackTrace,
                    Queue = "default",
                    RetryCount = 0,
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "ERP receipt/shipment operation sync failure could not be written to RII_JOB_FAILURE_LOG. SourceMovementKey: {SourceMovementKey}", key);
            }
        }

        private static bool IsGoodsReceipt(MalKabulVeSevkiyatDto movement)
            => string.Equals(Clean(movement.GcKodu), "G", StringComparison.OrdinalIgnoreCase);

        private static bool IsShipment(MalKabulVeSevkiyatDto movement)
            => string.Equals(Clean(movement.GcKodu), "C", StringComparison.OrdinalIgnoreCase);

        private static bool IsFeedReceipt(MalKabulVeSevkiyatDto movement, StockEntity? stock = null)
            => string.Equals(Clean(movement.GrupKodu), "YEM", StringComparison.OrdinalIgnoreCase)
               || string.Equals(Clean(stock?.GrupKodu), "YEM", StringComparison.OrdinalIgnoreCase)
               || Clean(movement.StokKodu).StartsWith("Y", StringComparison.OrdinalIgnoreCase)
               || Clean(stock?.ErpStockCode).StartsWith("Y", StringComparison.OrdinalIgnoreCase)
               || Clean(movement.StokAdi).Contains("Yem", StringComparison.OrdinalIgnoreCase)
               || Clean(stock?.StockName).Contains("Yem", StringComparison.OrdinalIgnoreCase)
               || Clean(movement.IslemTuru).Contains("Yem", StringComparison.OrdinalIgnoreCase);

        private static int ResolveCount(MalKabulVeSevkiyatDto movement)
            => Convert.ToInt32(Math.Round(movement.Miktar ?? 0, 0, MidpointRounding.AwayFromZero));

        private static string BuildDocumentNo(string prefix, MalKabulVeSevkiyatDto movement)
        {
            var documentNo = Clean(movement.FisNo);
            if (string.IsNullOrWhiteSpace(documentNo))
            {
                documentNo = $"{movement.Tarih:yyyyMMdd}-{Clean(movement.ProjeKodu)}-{movement.KafesKodu?.ToString(CultureInfo.InvariantCulture) ?? "NA"}";
            }

            return Shorten($"{prefix}-{documentNo}", 50);
        }

        private static string BuildHeaderNote(MalKabulVeSevkiyatDto movement)
            => Shorten($"ERP import | {Clean(movement.IslemTuru)} | Project={Clean(movement.ProjeKodu)} | Cage={movement.KafesKodu?.ToString(CultureInfo.InvariantCulture) ?? "null"}", 500);

        private static string BuildSourceMovementKey(MalKabulVeSevkiyatDto movement)
        {
            var quantity = (movement.Miktar ?? 0).ToString("0.########", CultureInfo.InvariantCulture);
            var raw = string.Join("|", new[]
            {
                "Netsis",
                movement.Tarih.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
                Clean(movement.FisNo),
                movement.KafesKodu?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Clean(movement.ProjeKodu),
                Clean(movement.StokKodu),
                Clean(movement.HareketTuru),
                Clean(movement.GcKodu),
                quantity,
                Clean(movement.IslemTuru)
            });

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return $"Netsis|{Convert.ToHexString(hashBytes)}";
        }

        private static string Shorten(string value, int maxLength)
            => value.Length <= maxLength ? value : value[..maxLength];

        private static string BuildDiagnosticMessage(Exception ex)
        {
            var messages = new List<string>();
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (!string.IsNullOrWhiteSpace(current.Message) &&
                    !messages.Contains(current.Message, StringComparer.OrdinalIgnoreCase))
                {
                    messages.Add(current.Message);
                }
            }

            var baseMessage = ex.GetBaseException().Message;
            if (!string.IsNullOrWhiteSpace(baseMessage) &&
                !messages.Contains(baseMessage, StringComparer.OrdinalIgnoreCase))
            {
                messages.Add(baseMessage);
            }

            return string.Join(" | ", messages);
        }

        private static string? OptionalShorten(string? value, int maxLength)
        {
            var cleanValue = Clean(value);
            return string.IsNullOrWhiteSpace(cleanValue) ? null : Shorten(cleanValue, maxLength);
        }

        private static string Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private enum ApplyOutcome
        {
            Skipped,
            Created,
            Updated
        }
    }
}
