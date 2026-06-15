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
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public class ErpReceiptShipmentMovementSyncJob : IErpReceiptShipmentMovementSyncJob
    {
        private const string RecurringJobId = "erp-receipt-shipment-movement-sync-job";
        private const string GoodsReceiptRefTable = "RII_GoodsReceiptLine";
        private const string ShipmentRefTable = "RII_ShipmentLine";

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
                    skippedCount++;
                    continue;
                }

                if (!processedKeys.Add(sourceMovementKey))
                {
                    duplicatePayloadCount++;
                    continue;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var outcome = await ApplyMovementAsync(erpMovement, sourceMovementKey);

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

        private async Task<ApplyOutcome> ApplyGoodsReceiptMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            var project = await ResolveProjectAsync(movement);
            var stock = await ResolveStockAsync(movement);
            var warehouse = await ResolveWarehouseAsync(movement.KafesKodu);
            var projectCage = await ResolveProjectCageAsync(project, movement.KafesKodu);
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

            return IsFeedReceipt(movement)
                ? await UpsertFeedReceiptLineAsync(receipt, stock, movement, sourceMovementKey)
                : await UpsertFishReceiptLineAsync(receipt, stock, project, projectCage, movement, sourceMovementKey);
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
            MalKabulVeSevkiyatDto movement,
            string sourceMovementKey)
        {
            if (project == null)
            {
                throw new InvalidOperationException($"ERP fish receipt project could not be matched. ProjectCode={Clean(movement.ProjeKodu)}");
            }

            if (projectCage == null)
            {
                throw new InvalidOperationException($"ERP fish receipt cage could not be matched. CageCode={movement.KafesKodu?.ToString() ?? "null"}");
            }

            var fishCount = ResolveCount(movement);
            var fishBatch = await ResolveOrCreateFishBatchAsync(project, stock, receipt, movement);
            var averageGram = await ResolveAverageGramAsync(fishBatch, projectCage);
            if (averageGram <= 0)
            {
                throw new InvalidOperationException($"ERP fish receipt average gram could not be resolved. SourceMovementKey={sourceMovementKey}");
            }

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

            if (deltaCount != 0 || deltaBiomass != 0)
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

                return ApplyOutcome.Updated;
            }

            return ApplyOutcome.Skipped;
        }

        private async Task<ApplyOutcome> ApplyShipmentMovementAsync(MalKabulVeSevkiyatDto movement, string sourceMovementKey)
        {
            var project = await ResolveProjectAsync(movement)
                ?? throw new InvalidOperationException($"ERP shipment project could not be matched. ProjectCode={Clean(movement.ProjeKodu)}");
            var stock = await ResolveStockAsync(movement);
            var projectCage = await ResolveProjectCageAsync(project, movement.KafesKodu)
                ?? throw new InvalidOperationException($"ERP shipment cage could not be matched. CageCode={movement.KafesKodu?.ToString() ?? "null"}");

            var fishCount = ResolveCount(movement);
            var activeBalance = await ResolveShipmentBalanceAsync(projectCage.Id, stock.Id);
            if (activeBalance == null)
            {
                throw new InvalidOperationException($"ERP shipment live balance could not be found. ProjectCageId={projectCage.Id}, StockId={stock.Id}");
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

        private async Task<StockEntity> ResolveStockAsync(MalKabulVeSevkiyatDto movement)
        {
            var stockCode = Clean(movement.StokKodu);
            return await _db.Stocks.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpStockCode == stockCode)
                ?? throw new InvalidOperationException($"ERP stock could not be matched. StockCode={stockCode}");
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
            }

            var code = erpWarehouseCode.Value.ToString(CultureInfo.InvariantCulture);
            return await _db.Cages.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.CageCode == code);
        }

        private async Task<WarehouseEntity?> ResolveWarehouseAsync(short? erpWarehouseCode)
        {
            return erpWarehouseCode.HasValue
                ? await _db.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpWarehouseCode == erpWarehouseCode.Value)
                : null;
        }

        private async Task<FishBatch> ResolveOrCreateFishBatchAsync(Project project, StockEntity stock, GoodsReceipt receipt, MalKabulVeSevkiyatDto movement)
        {
            var existingBatch = await _db.FishBatches
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishStockId == stock.Id)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            if (existingBatch != null)
            {
                return existingBatch;
            }

            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = receipt.ReceiptNo,
                FishStockId = stock.Id,
                CurrentAverageGram = 0,
                StartDate = movement.Tarih,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _db.FishBatches.AddAsync(batch);
            await _db.SaveChangesAsync();
            return batch;
        }

        private async Task<decimal> ResolveAverageGramAsync(FishBatch fishBatch, ProjectCage projectCage)
        {
            if (fishBatch.CurrentAverageGram > 0)
            {
                return fishBatch.CurrentAverageGram;
            }

            var latestBalance = await _db.BatchCageBalances
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.FishBatchId == fishBatch.Id && x.ProjectCageId == projectCage.Id && x.AverageGram > 0)
                .OrderByDescending(x => x.AsOfDate)
                .FirstOrDefaultAsync();

            return latestBalance?.AverageGram ?? 0;
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
                    ExceptionMessage = ex.Message,
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

        private static bool IsFeedReceipt(MalKabulVeSevkiyatDto movement)
            => string.Equals(Clean(movement.GrupKodu), "YEM", StringComparison.OrdinalIgnoreCase)
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
