using Hangfire;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public class WarehouseSyncJob : IWarehouseSyncJob
    {
        private const string RecurringJobId = "erp-warehouse-sync-job";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IErpService _erpService;
        private readonly AquaDbContext _db;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<WarehouseSyncJob> _logger;

        public WarehouseSyncJob(
            IUnitOfWork unitOfWork,
            IErpService erpService,
            AquaDbContext db,
            ILocalizationService localizationService,
            ILogger<WarehouseSyncJob> logger)
        {
            _unitOfWork = unitOfWork;
            _erpService = erpService;
            _db = db;
            _localizationService = localizationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation(_localizationService.GetLocalizedString("WarehouseSyncJob.Started"));

            var erpResponse = await _erpService.GetDeposAsync(null);
            if (erpResponse == null || !erpResponse.Success)
            {
                var message = erpResponse?.ExceptionMessage ?? erpResponse?.Message ?? _localizationService.GetLocalizedString("WarehouseSyncJob.ErpFetchFailed");
                var ex = new InvalidOperationException(message);
                await LogRecordFailureAsync("ERP_FETCH", ex);
                _logger.LogWarning("Warehouse sync aborted: ERP fetch failed. Message: {Message}", message);
                return;
            }

            if (erpResponse.Data == null || erpResponse.Data.Count == 0)
            {
                _logger.LogInformation("Warehouse sync skipped: no ERP records returned.");
                return;
            }

            var createdCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var failedCount = 0;
            var duplicatePayloadCount = 0;
            var processedCodeAndBranch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var erpWarehouse in erpResponse.Data)
            {
                if (erpWarehouse.DepoKodu <= 0)
                {
                    skippedCount++;
                    continue;
                }

                var branchCode = (int)erpWarehouse.SubeKodu;
                var compositeKey = $"{erpWarehouse.DepoKodu}|{branchCode}";
                if (!processedCodeAndBranch.Add(compositeKey))
                {
                    duplicatePayloadCount++;
                    continue;
                }

                try
                {
                    var warehouse = await _unitOfWork.Repository<WarehouseEntity>()
                        .Query(tracking: true, ignoreQueryFilters: true)
                        .FirstOrDefaultAsync(x => x.ErpWarehouseCode == erpWarehouse.DepoKodu && x.BranchCode == branchCode);

                    var warehouseName = string.IsNullOrWhiteSpace(erpWarehouse.DepoIsmi)
                        ? erpWarehouse.DepoKodu.ToString()
                        : erpWarehouse.DepoIsmi!;
                    var customerCode = erpWarehouse.CariKodu;
                    var isLocked = erpWarehouse.DepoKilitLe == 'E';
                    var allowNegativeBalance = erpWarehouse.Eksibakiye == 'E';

                    if (warehouse == null)
                    {
                        await _unitOfWork.Repository<WarehouseEntity>().AddAsync(new WarehouseEntity
                        {
                            ErpWarehouseCode = erpWarehouse.DepoKodu,
                            WarehouseName = warehouseName,
                            CustomerCode = customerCode,
                            BranchCode = branchCode,
                            IsLocked = isLocked,
                            AllowNegativeBalance = allowNegativeBalance,
                            LastSyncedAt = DateTimeProvider.Now,
                            IsDeleted = false
                        });
                        await _unitOfWork.SaveChangesAsync();
                        createdCount++;
                        continue;
                    }

                    var updated = false;
                    if (warehouse.WarehouseName != warehouseName) { warehouse.WarehouseName = warehouseName; updated = true; }
                    if (warehouse.CustomerCode != customerCode) { warehouse.CustomerCode = customerCode; updated = true; }
                    if (warehouse.BranchCode != branchCode) { warehouse.BranchCode = branchCode; updated = true; }
                    if (warehouse.IsLocked != isLocked) { warehouse.IsLocked = isLocked; updated = true; }
                    if (warehouse.AllowNegativeBalance != allowNegativeBalance) { warehouse.AllowNegativeBalance = allowNegativeBalance; updated = true; }

                    warehouse.LastSyncedAt = DateTimeProvider.Now;
                    warehouse.UpdatedDate = DateTimeProvider.Now;
                    warehouse.UpdatedBy = null;

                    await _unitOfWork.SaveChangesAsync();

                    if (updated)
                    {
                        updatedCount++;
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await LogRecordFailureAsync(compositeKey, ex);
                    _db.ChangeTracker.Clear();
                }
            }

            _logger.LogInformation(
                "Warehouse sync completed. created={Created}, updated={Updated}, failed={Failed}, skipped={Skipped}, duplicatePayload={DuplicatePayload}.",
                createdCount,
                updatedCount,
                failedCount,
                skippedCount,
                duplicatePayloadCount);
            _logger.LogInformation(_localizationService.GetLocalizedString("WarehouseSyncJob.Completed"));
        }

        private async Task LogRecordFailureAsync(string code, Exception ex)
        {
            _logger.LogError(ex, "Warehouse sync record failed. WarehouseKey: {WarehouseKey}", code);

            try
            {
                _db.JobFailureLogs.Add(new JobFailureLog
                {
                    JobId = $"{RecurringJobId}:{code}:{DateTimeProvider.Now:yyyyMMddHHmmssfff}",
                    JobName = $"{typeof(WarehouseSyncJob).FullName}.ExecuteAsync",
                    FailedAt = DateTimeProvider.Now,
                    Reason = $"WarehouseKey={code}",
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
                _logger.LogWarning(logEx, "Warehouse sync failure could not be written to RII_JOB_FAILURE_LOG. WarehouseKey: {WarehouseKey}", code);
            }
        }
    }
}
