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
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
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
            var processedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var erpStock in erpResponse.Data)
            {
                var code = erpStock.StokKodu ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    skippedCount++;
                    continue;
                }

                if (!processedCodes.Add(code))
                {
                    duplicatePayloadCount++;
                    continue;
                }

                try
                {
                    var stock = await _unitOfWork.Stocks
                        .Query(tracking: true, ignoreQueryFilters: true)
                        .FirstOrDefaultAsync(x => x.ErpStockCode == code);

                    var stockName = Clean(erpStock.StokAdi);
                    stockName = string.IsNullOrWhiteSpace(stockName) ? code : stockName;
                    var unit = Clean(erpStock.OlcuBr1);
                    var ureticiKodu = Clean(erpStock.UreticiKodu);
                    var grupKodu = Clean(erpStock.GrupKodu);
                    var grupAdi = Clean(erpStock.GrupIsim);
                    var kod1 = Clean(erpStock.Kod1);
                    var kod1Adi = Clean(erpStock.Kod1Adi);
                    var kod2 = Clean(erpStock.Kod2);
                    var kod2Adi = Clean(erpStock.Kod2Adi);
                    var kod3 = Clean(erpStock.Kod3);
                    var kod3Adi = Clean(erpStock.Kod3Adi);
                    var kod4 = Clean(erpStock.Kod4);
                    var kod4Adi = Clean(erpStock.Kod4Adi);
                    var kod5 = Clean(erpStock.Kod5);
                    var kod5Adi = Clean(erpStock.Kod5Adi);
                    var branchCode = (int)erpStock.SubeKodu;

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
                            IsERPIntegrated = true,
                            ERPIntegrationNumber = code,
                            LastSyncDate = DateTime.UtcNow,
                            CountTriedBy = 0,
                            IsDeleted = false
                        });
                        await _unitOfWork.SaveChangesAsync();
                        createdCount++;
                        continue;
                    }

                    var updated = false;
                    if (stock.StockName != stockName) { stock.StockName = stockName; updated = true; }
                    if (ApplyErpText(stock.Unit, unit, value => stock.Unit = value)) { updated = true; }
                    if (ApplyErpText(stock.UreticiKodu, ureticiKodu, value => stock.UreticiKodu = value)) { updated = true; }
                    if (ApplyErpText(stock.GrupKodu, grupKodu, value => stock.GrupKodu = value)) { updated = true; }
                    if (ApplyErpText(stock.GrupAdi, grupAdi, value => stock.GrupAdi = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod1, kod1, value => stock.Kod1 = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod1Adi, kod1Adi, value => stock.Kod1Adi = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod2, kod2, value => stock.Kod2 = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod2Adi, kod2Adi, value => stock.Kod2Adi = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod3, kod3, value => stock.Kod3 = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod3Adi, kod3Adi, value => stock.Kod3Adi = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod4, kod4, value => stock.Kod4 = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod4Adi, kod4Adi, value => stock.Kod4Adi = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod5, kod5, value => stock.Kod5 = value)) { updated = true; }
                    if (ApplyErpText(stock.Kod5Adi, kod5Adi, value => stock.Kod5Adi = value)) { updated = true; }
                    if (stock.BranchCode != branchCode) { stock.BranchCode = branchCode; updated = true; }
                    if (!stock.IsERPIntegrated) { stock.IsERPIntegrated = true; updated = true; }
                    if (string.IsNullOrWhiteSpace(stock.ERPIntegrationNumber)) { stock.ERPIntegrationNumber = code; updated = true; }
                    if (stock.CountTriedBy == null) { stock.CountTriedBy = 0; updated = true; }

                    if (!updated)
                    {
                        continue;
                    }

                    stock.LastSyncDate = DateTime.UtcNow;
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

        private static string Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static bool ApplyErpText(string? current, string incoming, Action<string> assign)
        {
            if (string.IsNullOrWhiteSpace(incoming) && !string.IsNullOrWhiteSpace(current))
            {
                return false;
            }

            var next = incoming;
            if (current == next)
            {
                return false;
            }

            assign(next);
            return true;
        }
    }
}
