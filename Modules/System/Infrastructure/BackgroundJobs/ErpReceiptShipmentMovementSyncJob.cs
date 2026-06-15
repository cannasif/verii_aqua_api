using System.Globalization;
using System.Text;
using Hangfire;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public class ErpReceiptShipmentMovementSyncJob : IErpReceiptShipmentMovementSyncJob
    {
        private const string RecurringJobId = "erp-receipt-shipment-movement-sync-job";

        private readonly INetsisReadService _netsisReadService;
        private readonly AquaDbContext _db;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<ErpReceiptShipmentMovementSyncJob> _logger;

        public ErpReceiptShipmentMovementSyncJob(
            INetsisReadService netsisReadService,
            AquaDbContext db,
            ILocalizationService localizationService,
            ILogger<ErpReceiptShipmentMovementSyncJob> logger)
        {
            _netsisReadService = netsisReadService;
            _db = db;
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
                var ex = new InvalidOperationException(message);
                await LogRecordFailureAsync("ERP_FETCH", ex);
                _logger.LogWarning("ERP receipt/shipment mirror sync aborted. Message: {Message}", message);
                return;
            }

            if (erpResponse.Data == null || erpResponse.Data.Count == 0)
            {
                _logger.LogInformation("ERP receipt/shipment mirror sync skipped: no ERP records returned.");
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
                if (string.IsNullOrWhiteSpace(erpMovement.StokKodu) || erpMovement.Tarih == default)
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
                    var mirror = await _db.ErpReceiptShipmentMovements
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.SourceMovementKey == sourceMovementKey);

                    var isNew = mirror == null;
                    mirror ??= new ErpReceiptShipmentMovement
                    {
                        SourceSystem = "Netsis",
                        SourceMovementKey = sourceMovementKey,
                        CreatedDate = DateTimeProvider.Now,
                        IsDeleted = false
                    };

                    await ApplyErpFieldsAndMatchesAsync(mirror, erpMovement);

                    if (isNew)
                    {
                        _db.ErpReceiptShipmentMovements.Add(mirror);
                        createdCount++;
                    }
                    else
                    {
                        mirror.UpdatedDate = DateTimeProvider.Now;
                        mirror.UpdatedBy = null;
                        updatedCount++;
                    }

                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await LogRecordFailureAsync(sourceMovementKey, ex);
                    _db.ChangeTracker.Clear();
                }
            }

            _logger.LogInformation(
                "ERP receipt/shipment mirror sync completed. created={Created}, updated={Updated}, failed={Failed}, skipped={Skipped}, duplicatePayload={DuplicatePayload}.",
                createdCount,
                updatedCount,
                failedCount,
                skippedCount,
                duplicatePayloadCount);
            _logger.LogInformation(_localizationService.GetLocalizedString("ErpReceiptShipmentMovementSyncJob.Completed"));
        }

        private async Task ApplyErpFieldsAndMatchesAsync(ErpReceiptShipmentMovement mirror, MalKabulVeSevkiyatDto erpMovement)
        {
            var now = DateTimeProvider.Now;
            var projectCode = Clean(erpMovement.ProjeKodu);
            var stockCode = Clean(erpMovement.StokKodu);
            var documentNo = Clean(erpMovement.FisNo);
            var movementKind = Clean(erpMovement.HareketTuru);
            var inOutCode = Clean(erpMovement.GcKodu);
            var operationType = Clean(erpMovement.IslemTuru);
            var stockGroupCode = Clean(erpMovement.GrupKodu);

            mirror.SourceSystem = "Netsis";
            mirror.MovementDate = erpMovement.Tarih;
            mirror.DocumentNo = string.IsNullOrWhiteSpace(documentNo) ? null : documentNo;
            mirror.ErpWarehouseCode = erpMovement.KafesKodu;
            mirror.ErpProjectCode = string.IsNullOrWhiteSpace(projectCode) ? null : projectCode;
            mirror.ErpStockCode = stockCode;
            mirror.ErpStockName = Clean(erpMovement.StokAdi);
            mirror.Quantity = erpMovement.Miktar ?? 0;
            mirror.MovementKind = movementKind;
            mirror.InOutCode = inOutCode;
            mirror.StockGroupCode = string.IsNullOrWhiteSpace(stockGroupCode) ? null : stockGroupCode;
            mirror.OperationType = operationType;
            mirror.LastSyncedAt = now;
            mirror.IsDeleted = false;
            mirror.DeletedDate = null;
            mirror.DeletedBy = null;

            var project = string.IsNullOrWhiteSpace(projectCode)
                ? null
                : await _db.Projects
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectCode == projectCode);

            var stock = string.IsNullOrWhiteSpace(stockCode)
                ? null
                : await _db.Stocks
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpStockCode == stockCode);

            var cage = await ResolveCageAsync(erpMovement.KafesKodu);
            var projectCage = project != null && cage != null
                ? await _db.ProjectCages
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id)
                    .OrderByDescending(x => x.AssignedDate)
                    .FirstOrDefaultAsync()
                : null;

            var fishBatch = project != null && stock != null
                ? await _db.FishBatches
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(x => !x.IsDeleted && x.ProjectId == project.Id && x.FishStockId == stock.Id)
                    .OrderByDescending(x => x.StartDate)
                    .FirstOrDefaultAsync()
                : null;

            mirror.ProjectId = project?.Id;
            mirror.StockId = stock?.Id;
            mirror.CageId = cage?.Id;
            mirror.ProjectCageId = projectCage?.Id;
            mirror.FishBatchId = fishBatch?.Id;

            var matchErrors = BuildMatchErrors(erpMovement, project, stock, cage, projectCage);
            mirror.IsMatched = matchErrors.Count == 0;
            mirror.MatchError = matchErrors.Count == 0 ? null : string.Join(" | ", matchErrors);
            mirror.MatchedAt = matchErrors.Count == 0 ? now : null;
        }

        private async Task<Cage?> ResolveCageAsync(short? erpWarehouseCode)
        {
            if (!erpWarehouseCode.HasValue)
            {
                return null;
            }

            var warehouse = await _db.Warehouses
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.ErpWarehouseCode == erpWarehouseCode.Value);

            if (warehouse != null)
            {
                var mapping = await _db.CageWarehouseMappings
                    .AsNoTracking()
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
            return await _db.Cages
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.CageCode == code);
        }

        private static List<string> BuildMatchErrors(
            MalKabulVeSevkiyatDto erpMovement,
            Project? project,
            StockEntity? stock,
            Cage? cage,
            ProjectCage? projectCage)
        {
            var errors = new List<string>();
            if (project == null)
            {
                errors.Add($"Project not matched: {Clean(erpMovement.ProjeKodu)}");
            }

            if (stock == null)
            {
                errors.Add($"Stock not matched: {Clean(erpMovement.StokKodu)}");
            }

            if (erpMovement.KafesKodu.HasValue && cage == null)
            {
                errors.Add($"Cage/Warehouse not matched: {erpMovement.KafesKodu.Value}");
            }

            if (project != null && cage != null && projectCage == null)
            {
                errors.Add($"Project cage not matched: ProjectId={project.Id}, CageId={cage.Id}");
            }

            return errors;
        }

        private async Task LogRecordFailureAsync(string key, Exception ex)
        {
            _logger.LogError(ex, "ERP receipt/shipment mirror sync record failed. SourceMovementKey: {SourceMovementKey}", key);

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
                _logger.LogWarning(logEx, "ERP receipt/shipment mirror sync failure could not be written to RII_JOB_FAILURE_LOG. SourceMovementKey: {SourceMovementKey}", key);
            }
        }

        private static string BuildSourceMovementKey(MalKabulVeSevkiyatDto movement)
        {
            var quantity = (movement.Miktar ?? 0).ToString("0.########", CultureInfo.InvariantCulture);
            var parts = new[]
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
            };

            return string.Join("|", parts.Select(NormalizeKeyPart));
        }

        private static string NormalizeKeyPart(string value)
        {
            var clean = Clean(value);
            if (clean.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(clean.Length);
            foreach (var character in clean)
            {
                builder.Append(character == '|' ? '/' : character);
            }

            return builder.ToString();
        }

        private static string Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
