using Hangfire;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public class StockSyncJob : IStockSyncJob
    {
        private const string RecurringJobId = "erp-stock-sync-job";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IErpService _erpService;
        private readonly AquaDbContext _db;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<StockSyncJob> _logger;

        public StockSyncJob(
            IUnitOfWork unitOfWork,
            IErpService erpService,
            AquaDbContext db,
            ILocalizationService localizationService,
            ILogger<StockSyncJob> logger)
        {
            _unitOfWork = unitOfWork;
            _erpService = erpService;
            _db = db;
            _localizationService = localizationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation(_localizationService.GetLocalizedString("StockSyncJob.Started"));

            var erpResponse = await _erpService.GetStoksAsync(null);
            if (erpResponse == null || !erpResponse.Success)
            {
                var message = erpResponse?.ExceptionMessage ?? erpResponse?.Message ?? _localizationService.GetLocalizedString("StockSyncJob.ErpFetchFailed");
                var ex = new InvalidOperationException(message);
                await LogRecordFailureAsync("ERP_FETCH", ex);
                _logger.LogWarning("Stock sync aborted: ERP fetch failed. Message: {Message}", message);
                return;
            }

            if (erpResponse.Data == null || erpResponse.Data.Count == 0)
            {
                _logger.LogInformation("Stock sync skipped: no ERP records returned.");
                return;
            }

            var createdCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var failedCount = 0;
            var duplicatePayloadCount = 0;
            var branchCode = 0;
            var compositeKey = string.Empty;
            var processedCodeAndBranch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var erpStock in erpResponse.Data)
            {
                var code = erpStock.StokKodu ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    skippedCount++;
                    continue;
                }

                branchCode = (int)erpStock.SubeKodu;
                compositeKey = $"{code}|{branchCode}";
                if (!processedCodeAndBranch.Add(compositeKey))
                {
                    duplicatePayloadCount++;
                    continue;
                }

                try
                {
                    var stock = await _unitOfWork.Stocks
                        .Query(tracking: true, ignoreQueryFilters: true)
                        .FirstOrDefaultAsync(x => x.ErpStockCode == code && x.BranchCode == branchCode);

                    var stockName = string.IsNullOrWhiteSpace(erpStock.StokAdi) ? code : erpStock.StokAdi!;
                    var unit = erpStock.OlcuBr1 ?? string.Empty;
                    var ureticiKodu = erpStock.UreticiKodu ?? string.Empty;
                    var grupKodu = erpStock.GrupKodu ?? string.Empty;
                    var grupAdi = erpStock.GrupIsim ?? string.Empty;
                    var kod1 = erpStock.Kod1 ?? string.Empty;
                    var kod1Adi = erpStock.Kod1Adi ?? string.Empty;
                    var kod2 = erpStock.Kod2 ?? string.Empty;
                    var kod2Adi = erpStock.Kod2Adi ?? string.Empty;
                    var kod3 = erpStock.Kod3 ?? string.Empty;
                    var kod3Adi = erpStock.Kod3Adi ?? string.Empty;
                    var kod4 = erpStock.Kod4 ?? string.Empty;
                    var kod4Adi = erpStock.Kod4Adi ?? string.Empty;
                    var kod5 = erpStock.Kod5 ?? string.Empty;
                    var kod5Adi = erpStock.Kod5Adi ?? string.Empty;

                    if (stock == null)
                    {
                        await _unitOfWork.Stocks.AddAsync(new StockEntity
                        {
                            ErpStockCode = code,
                            StockName = stockName,
                            Unit = unit,
                            UreticiKodu = ureticiKodu,
                            GrupKodu = grupKodu,
                            GrupAdi = grupAdi,
                            Kod1 = kod1,
                            Kod1Adi = kod1Adi,
                            Kod2 = kod2,
                            Kod2Adi = kod2Adi,
                            Kod3 = kod3,
                            Kod3Adi = kod3Adi,
                            Kod4 = kod4,
                            Kod4Adi = kod4Adi,
                            Kod5 = kod5,
                            Kod5Adi = kod5Adi,
                            BranchCode = branchCode,
                            IsDeleted = false
                        });
                        await _unitOfWork.SaveChangesAsync();
                        createdCount++;
                        continue;
                    }

                    var updated = false;
                    if (stock.StockName != stockName) { stock.StockName = stockName; updated = true; }
                    if (stock.Unit != unit) { stock.Unit = unit; updated = true; }
                    if (stock.UreticiKodu != ureticiKodu) { stock.UreticiKodu = ureticiKodu; updated = true; }
                    if (stock.GrupKodu != grupKodu) { stock.GrupKodu = grupKodu; updated = true; }
                    if (stock.GrupAdi != grupAdi) { stock.GrupAdi = grupAdi; updated = true; }
                    if (stock.Kod1 != kod1) { stock.Kod1 = kod1; updated = true; }
                    if (stock.Kod1Adi != kod1Adi) { stock.Kod1Adi = kod1Adi; updated = true; }
                    if (stock.Kod2 != kod2) { stock.Kod2 = kod2; updated = true; }
                    if (stock.Kod2Adi != kod2Adi) { stock.Kod2Adi = kod2Adi; updated = true; }
                    if (stock.Kod3 != kod3) { stock.Kod3 = kod3; updated = true; }
                    if (stock.Kod3Adi != kod3Adi) { stock.Kod3Adi = kod3Adi; updated = true; }
                    if (stock.Kod4 != kod4) { stock.Kod4 = kod4; updated = true; }
                    if (stock.Kod4Adi != kod4Adi) { stock.Kod4Adi = kod4Adi; updated = true; }
                    if (stock.Kod5 != kod5) { stock.Kod5 = kod5; updated = true; }
                    if (stock.Kod5Adi != kod5Adi) { stock.Kod5Adi = kod5Adi; updated = true; }
                    if (stock.BranchCode != branchCode) { stock.BranchCode = branchCode; updated = true; }

                    if (!updated)
                    {
                        continue;
                    }

                    stock.UpdatedDate = DateTimeProvider.Now;
                    stock.UpdatedBy = null;
                    await _unitOfWork.SaveChangesAsync();
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await LogRecordFailureAsync(code, ex);
                    _db.ChangeTracker.Clear();
                }
            }

            _logger.LogInformation(
                "Stock sync completed. created={Created}, updated={Updated}, failed={Failed}, skipped={Skipped}, duplicatePayload={DuplicatePayload}.",
                createdCount,
                updatedCount,
                failedCount,
                skippedCount,
                duplicatePayloadCount);
            _logger.LogInformation(_localizationService.GetLocalizedString("StockSyncJob.Completed"));
        }

        private async Task LogRecordFailureAsync(string code, Exception ex)
        {
            _logger.LogError(ex, "Stock sync record failed. StockCode: {StockCode}", code);

            try
            {
                _db.JobFailureLogs.Add(new JobFailureLog
                {
                    JobId = $"{RecurringJobId}:{code}:{DateTimeProvider.Now:yyyyMMddHHmmssfff}",
                    JobName = $"{typeof(StockSyncJob).FullName}.ExecuteAsync",
                    FailedAt = DateTimeProvider.Now,
                    Reason = $"StockCode={code}",
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
                _logger.LogWarning(logEx, "Stock sync failure could not be written to RII_JOB_FAILURE_LOG. StockCode: {StockCode}", code);
            }
        }
    }
}
