using AutoMapper;
using aqua_api.Modules.Integrations.Domain.Erp;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace aqua_api.Modules.Integrations.Application.Services
{
    /// <summary>
    /// Read-oriented Netsis facade modeled after CRM's Netsis module.
    /// Existing Aqua ERP endpoints continue to work through an adapter layer.
    /// </summary>
    public class NetsisReadService : INetsisReadService, IBudgetExchangeRateReadService
    {
        private readonly AquaDbContext _dbContext;
        private readonly ILogger<NetsisReadService> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NetsisReadService(
            AquaDbContext dbContext,
            ILogger<NetsisReadService> logger,
            ILocalizationService localizationService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _logger = logger;
            _localizationService = localizationService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ApiResponse<short>> GetBranchCodeFromContextAsync()
        {
            var branchCodeStr = _httpContextAccessor.HttpContext?.Items["BranchCode"]?.ToString();

            if (!short.TryParse(branchCodeStr, out var branchCode))
            {
                return Task.FromResult(ApiResponse<short>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.BranchCodeRetrievalError"),
                    _localizationService.GetLocalizedString("ErpService.BranchCodeRetrievalErrorMessage"),
                    StatusCodes.Status500InternalServerError));
            }

            return Task.FromResult(ApiResponse<short>.SuccessResult(
                branchCode,
                _localizationService.GetLocalizedString("ErpService.BranchCodeRetrieved")));
        }

        public async Task<ApiResponse<List<DepoDto>>> GetWarehousesAsync(short? warehouseCode)
        {
            try
            {
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var result = await _dbContext.Set<RII_FN_DEPO>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_DEPO({0}, {1})",
                        warehouseCode.HasValue ? warehouseCode.Value : DBNull.Value,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<DepoDto>>(result);
                return ApiResponse<List<DepoDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.DepoRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DepoDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.ErrorRetrievingDepoRecords", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<CariDto>>> GetCustomersAsync(string? customerCode)
        {
            try
            {
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var result = await _dbContext.RII_FN_CARI
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_CARI({0}, {1})",
                        string.IsNullOrWhiteSpace(customerCode) ? DBNull.Value : customerCode,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<CariDto>>(result);
                return ApiResponse<List<CariDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.CariRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CariDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllCariExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<CariDto>>> GetCustomersByCodesAsync(IEnumerable<string> customerCodes)
        {
            try
            {
                var codes = (customerCodes ?? Array.Empty<string>())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct()
                    .ToList();

                var customerParam = codes.Count == 0 ? null : string.Join(",", codes);

                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCsv = string.IsNullOrWhiteSpace(branchFromContext)
                    ? null
                    : string.Join(",", branchFromContext.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)));

                var result = await _dbContext.RII_FN_CARI
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_CARI({0}, {1})",
                        string.IsNullOrWhiteSpace(customerParam) ? DBNull.Value : customerParam,
                        string.IsNullOrWhiteSpace(branchCsv) ? DBNull.Value : branchCsv)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<CariDto>>(result);
                return ApiResponse<List<CariDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.CariRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CariDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllCariExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<StokFunctionDto>>> GetStocksAsync(string? stockCode)
        {
            try
            {
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var result = await _dbContext.Set<RII_FN_STOK>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_STOK({0}, {1})",
                        string.IsNullOrWhiteSpace(stockCode) ? DBNull.Value : stockCode,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<StokFunctionDto>>(result);
                return ApiResponse<List<StokFunctionDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.StokRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StokFunctionDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllStokExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null)
        {
            try
            {
                var connectionString = _dbContext.Database.GetConnectionString();
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogWarning("GetBranchesAsync called but DefaultConnection is not configured.");
                    return ApiResponse<List<BranchDto>>.ErrorResult(
                        _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                        "DefaultConnection is not configured.",
                        StatusCodes.Status503ServiceUnavailable);
                }

                _logger.LogInformation(
                    "ERP branch list requested. BranchNo: {BranchNo}, ConnectionStringPresent: {HasConnectionString}",
                    branchNo,
                    !string.IsNullOrWhiteSpace(connectionString));

                var rows = await _dbContext.Set<RII_FN_BRANCHES>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_BRANCHES({0})",
                        branchNo.HasValue ? branchNo.Value : DBNull.Value)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("ERP branch list retrieved successfully. Count: {Count}", rows.Count);

                var mappedList = _mapper.Map<List<BranchDto>>(rows);
                return ApiResponse<List<BranchDto>>.SuccessResult(
                    mappedList,
                    _localizationService.GetLocalizedString("ErpService.BranchesRetrievedSuccessfully"));
            }
            catch (Exception ex)
            {
                try
                {
                    var conn = _dbContext.Database.GetDbConnection();
                    _logger.LogError(
                        ex,
                        "ERP branch list retrieval failed. BranchNo: {BranchNo}, ConnectionState: {ConnectionState}, DataSource: {DataSource}, Database: {Database}, InnerException: {InnerException}",
                        branchNo,
                        conn?.State.ToString(),
                        conn?.DataSource,
                        conn?.Database,
                        ex.InnerException?.Message);
                }
                catch
                {
                    _logger.LogError(ex, "ERP branch list retrieval failed. BranchNo: {BranchNo}", branchNo);
                }

                return ApiResponse<List<BranchDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.BranchesRetrievalError", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<KurDto>>> GetExchangeRatesAsync(DateTime date, int pricingType)
        {
            try
            {
                var resultDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var result = await _dbContext.Set<RII_FN_KUR>()
                    .FromSqlRaw("SELECT * FROM dbo.RII_FN_KUR({0}, {1})", resultDate, pricingType)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<KurDto>>(result);
                return ApiResponse<List<KurDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.ExchangeRateRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<KurDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllExchangeRateExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<ErpBudgetExchangeRateDto>>> GetBudgetExchangeRatesAsync(
            int startYear,
            int startMonth,
            int endYear,
            int endMonth)
        {
            if (!IsValidPeriod(startYear, startMonth) ||
                !IsValidPeriod(endYear, endMonth) ||
                startYear * 12 + startMonth > endYear * 12 + endMonth)
            {
                return ApiResponse<List<ErpBudgetExchangeRateDto>>.ErrorResult(
                    "Kur donemi hatali.",
                    "Kur donemi hatali.",
                    StatusCodes.Status400BadRequest);
            }

            var connection = _dbContext.Database.GetDbConnection();
            var shouldCloseConnection = connection.State == ConnectionState.Closed;

            try
            {
                if (shouldCloseConnection)
                {
                    await connection.OpenAsync();
                }

                await using var command = connection.CreateCommand();
                command.CommandText =
                    """
                    SELECT
                        KOD_ID,
                        YIL_AY,
                        TUTAR,
                        AY,
                        YIL,
                        KAYIT_TARIHI
                    FROM MAVIDEN..TB_PBI_DOVIZ
                    WHERE
                        (YIL > @StartYear OR (YIL = @StartYear AND TRY_CONVERT(int, AY) >= @StartMonth))
                        AND
                        (YIL < @EndYear OR (YIL = @EndYear AND TRY_CONVERT(int, AY) <= @EndMonth))
                    ORDER BY YIL, TRY_CONVERT(int, AY), KOD_ID, KAYIT_TARIHI DESC;
                    """;
                AddParameter(command, "@StartYear", startYear);
                AddParameter(command, "@StartMonth", startMonth);
                AddParameter(command, "@EndYear", endYear);
                AddParameter(command, "@EndMonth", endMonth);

                var rows = new List<ErpBudgetExchangeRateDto>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.IsDBNull(0) ||
                        reader.IsDBNull(2) ||
                        reader.IsDBNull(3) ||
                        reader.IsDBNull(4) ||
                        !int.TryParse(reader.GetString(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out var month))
                    {
                        continue;
                    }

                    var currencyTypeId = reader.GetInt32(0);
                    var currencyCode = ResolveCurrencyCode(currencyTypeId);
                    if (currencyCode == null || month is < 1 or > 12)
                    {
                        continue;
                    }

                    rows.Add(new ErpBudgetExchangeRateDto
                    {
                        CurrencyTypeId = currencyTypeId,
                        CurrencyCode = currencyCode,
                        YearMonth = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ExchangeRate = reader.GetDecimal(2),
                        Month = month,
                        Year = reader.GetInt32(4),
                        RecordDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                    });
                }

                return ApiResponse<List<ErpBudgetExchangeRateDto>>.SuccessResult(
                    rows,
                    "Butce kur kayitlari getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "MAVIDEN budget exchange rates could not be read for {StartYear}-{StartMonth} / {EndYear}-{EndMonth}.",
                    startYear,
                    startMonth,
                    endYear,
                    endMonth);
                return ApiResponse<List<ErpBudgetExchangeRateDto>>.ErrorResult(
                    "Butce kur kayitlari getirilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
            finally
            {
                if (shouldCloseConnection && connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<ApiResponse<List<ErpShippingAddressDto>>> GetShippingAddressesAsync(string customerCode)
        {
            try
            {
                var result = await _dbContext.Set<RII_FN_2SHIPPING>()
                    .FromSqlRaw("SELECT * FROM dbo.RII_FN_2SHIPPING({0})", customerCode)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<ErpShippingAddressDto>>(result);
                return ApiResponse<List<ErpShippingAddressDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.ExchangeRateRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ErpShippingAddressDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllErpShippingAddressExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<StokGroupDto>>> GetStockGroupsAsync(string? groupCode)
        {
            try
            {
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var result = await _dbContext.Set<RII_STGROUP>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_STGRUP({0}, {1})",
                        string.IsNullOrWhiteSpace(groupCode) ? DBNull.Value : groupCode,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<StokGroupDto>>(result);
                return ApiResponse<List<StokGroupDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.StokGroupRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StokGroupDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetAllStokGroupExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<ProjeDto>>> GetProjectsAsync()
        {
            try
            {
                var result = await _dbContext.Set<RII_FN_PROJECTCODE>()
                    .FromSqlRaw("SELECT * FROM dbo.RII_FN_PROJECTCODE()")
                    .AsNoTracking()
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<ProjeDto>>(result);
                return ApiResponse<List<ProjeDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.ProjeRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                if (IsMissingSqlObject(ex, "RII_FN_PROJECTCODE"))
                {
                    _logger.LogWarning(ex, "ERP project function is missing. Falling back to Aqua project table.");

                    var fallbackProjects = await _dbContext.Projects
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted)
                        .OrderBy(x => x.ProjectCode)
                        .Select(x => new ProjeDto
                        {
                            ProjeKod = x.ProjectCode,
                            ProjeAciklama = x.ProjectName
                        })
                        .ToListAsync();

                    return ApiResponse<List<ProjeDto>>.SuccessResult(
                        fallbackProjects,
                        _localizationService.GetLocalizedString("ErpService.ProjeRecordsRetrieved"));
                }

                return ApiResponse<List<ProjeDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.GetProjectCodesExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsAsync(DateTime? startDate = null)
        {
            try
            {
                var query = startDate.HasValue
                    ? _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi({0})", startDate.Value.Date)
                    : _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi(DEFAULT)");

                var result = await query
                    .AsNoTracking()
                    .OrderByDescending(x => x.Tarih)
                    .ToListAsync();

                var mappedResult = _mapper.Map<List<MalKabulVeSevkiyatDto>>(result);
                return ApiResponse<List<MalKabulVeSevkiyatDto>>.SuccessResult(
                    mappedResult,
                    _localizationService.GetLocalizedString("ErpService.MalKabulVeSevkiyatRecordsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MalKabulVeSevkiyatDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.MalKabulVeSevkiyatRecordsRetrievalError", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorAsync()
        {
            try
            {
                var result = await _dbContext.ErpReceiptShipmentMovements
                    .AsNoTracking()
                    .Include(x => x.Project)
                    .Include(x => x.Cage)
                    .Include(x => x.Stock)
                    .Include(x => x.FishBatch)
                    .OrderByDescending(x => x.MovementDate)
                    .ThenByDescending(x => x.Id)
                    .Select(x => new ErpReceiptShipmentMovementDto
                    {
                        Id = x.Id,
                        SourceSystem = x.SourceSystem,
                        SourceMovementKey = x.SourceMovementKey,
                        MovementDate = x.MovementDate,
                        DocumentNo = x.DocumentNo,
                        ErpWarehouseCode = x.ErpWarehouseCode,
                        ErpProjectCode = x.ErpProjectCode,
                        ErpStockCode = x.ErpStockCode,
                        ErpStockName = x.ErpStockName,
                        Quantity = x.Quantity,
                        MovementKind = x.MovementKind,
                        InOutCode = x.InOutCode,
                        StockGroupCode = x.StockGroupCode,
                        OperationType = x.OperationType,
                        ProjectId = x.ProjectId,
                        ProjectCode = x.Project != null ? x.Project.ProjectCode : null,
                        ProjectName = x.Project != null ? x.Project.ProjectName : null,
                        CageId = x.CageId,
                        CageCode = x.Cage != null ? x.Cage.CageCode : null,
                        CageName = x.Cage != null ? x.Cage.CageName : null,
                        ProjectCageId = x.ProjectCageId,
                        StockId = x.StockId,
                        StockCode = x.Stock != null ? x.Stock.ErpStockCode : null,
                        StockName = x.Stock != null ? x.Stock.StockName : null,
                        FishBatchId = x.FishBatchId,
                        BatchCode = x.FishBatch != null ? x.FishBatch.BatchCode : null,
                        GoodsReceiptId = x.GoodsReceiptId,
                        GoodsReceiptLineId = x.GoodsReceiptLineId,
                        ShipmentId = x.ShipmentId,
                        ShipmentLineId = x.ShipmentLineId,
                        BatchMovementId = x.BatchMovementId,
                        IsMatched = x.IsMatched,
                        IsProcessed = x.IsProcessed,
                        ProcessingAttemptCount = x.ProcessingAttemptCount,
                        LastSyncedAt = x.LastSyncedAt,
                        MatchedAt = x.MatchedAt,
                        ProcessedAt = x.ProcessedAt,
                        MatchError = x.MatchError,
                        ProcessError = x.ProcessError,
                        IsCancelled = x.IsCancelled,
                        CancelledAt = x.CancelledAt,
                        CancelledBy = x.CancelledBy,
                        CancellationReason = x.CancellationReason
                    })
                    .ToListAsync();

                return ApiResponse<List<ErpReceiptShipmentMovementDto>>.SuccessResult(
                    result,
                    _localizationService.GetLocalizedString("ErpService.ReceiptShipmentMovementMirrorRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ErpReceiptShipmentMovementDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.ReceiptShipmentMovementMirrorRetrievalError", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDirection)
        {
            return await GetReceiptShipmentMovementMirrorPagedAsync(new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public async Task<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorPagedAsync(
            PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                var query = BuildReceiptShipmentMovementMirrorQuery()
                    .ApplySearch(request)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                query = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(ErpReceiptShipmentMovementDto.MovementDate) : request.SortBy,
                    request.SortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(rows, totalCount, request.PageNumber, request.PageSize, "ErpService.ReceiptShipmentMovementMirrorRetrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.InternalServerError"),
                    _localizationService.GetLocalizedString("ErpService.ReceiptShipmentMovementMirrorRetrievalError", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<object>> HealthCheckAsync()
        {
            try
            {
                await _dbContext.Database.CanConnectAsync();

                return ApiResponse<object>.SuccessResult(
                    new { Status = _localizationService.GetLocalizedString("General.Healthy"), Timestamp = DateTime.UtcNow },
                    _localizationService.GetLocalizedString("ErpService.ErpConnectionSuccessful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERP Health check failed");
                return ApiResponse<object>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpService.ErpConnectionFailed"),
                    _localizationService.GetLocalizedString("ErpService.HealthCheckExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<ErpReceiptShipmentMovementDto> BuildReceiptShipmentMovementMirrorQuery()
        {
            return _dbContext.ErpReceiptShipmentMovements
                .AsNoTracking()
                .Select(x => new ErpReceiptShipmentMovementDto
                {
                    Id = x.Id,
                    SourceSystem = x.SourceSystem,
                    SourceMovementKey = x.SourceMovementKey,
                    MovementDate = x.MovementDate,
                    DocumentNo = x.DocumentNo,
                    ErpWarehouseCode = x.ErpWarehouseCode,
                    ErpProjectCode = x.ErpProjectCode,
                    ErpStockCode = x.ErpStockCode,
                    ErpStockName = x.ErpStockName,
                    Quantity = x.Quantity,
                    MovementKind = x.MovementKind,
                    InOutCode = x.InOutCode,
                    StockGroupCode = x.StockGroupCode,
                    OperationType = x.OperationType,
                    ProjectId = x.ProjectId,
                    ProjectCode = x.Project != null ? x.Project.ProjectCode : null,
                    ProjectName = x.Project != null ? x.Project.ProjectName : null,
                    CageId = x.CageId,
                    CageCode = x.Cage != null ? x.Cage.CageCode : null,
                    CageName = x.Cage != null ? x.Cage.CageName : null,
                    ProjectCageId = x.ProjectCageId,
                    StockId = x.StockId,
                    StockCode = x.Stock != null ? x.Stock.ErpStockCode : null,
                    StockName = x.Stock != null ? x.Stock.StockName : null,
                    FishBatchId = x.FishBatchId,
                    BatchCode = x.FishBatch != null ? x.FishBatch.BatchCode : null,
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptLineId = x.GoodsReceiptLineId,
                    ShipmentId = x.ShipmentId,
                    ShipmentLineId = x.ShipmentLineId,
                    BatchMovementId = x.BatchMovementId,
                    IsMatched = x.IsMatched,
                    IsProcessed = x.IsProcessed,
                    ProcessingAttemptCount = x.ProcessingAttemptCount,
                    LastSyncedAt = x.LastSyncedAt,
                    MatchedAt = x.MatchedAt,
                    ProcessedAt = x.ProcessedAt,
                    MatchError = x.MatchError,
                    ProcessError = x.ProcessError,
                    IsCancelled = x.IsCancelled,
                    CancelledAt = x.CancelledAt,
                    CancelledBy = x.CancelledBy,
                    CancellationReason = x.CancellationReason
                });
        }

        private ApiResponse<PagedResponse<TDto>> ToPagedSuccess<TDto>(
            List<TDto> rows,
            int totalCount,
            int pageNumber,
            int pageSize,
            string messageKey)
        {
            return ApiResponse<PagedResponse<TDto>>.SuccessResult(
                new PagedResponse<TDto>
                {
                    Items = rows,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                _localizationService.GetLocalizedString(messageKey));
        }

        private static bool IsValidPeriod(int year, int month)
        {
            return year is >= 2000 and <= 2100 && month is >= 1 and <= 12;
        }

        private static string? ResolveCurrencyCode(int currencyTypeId)
        {
            return currencyTypeId switch
            {
                1 => "USD",
                2 => "EUR",
                3 => "GBP",
                _ => null
            };
        }

        private static void AddParameter(global::System.Data.Common.DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static bool IsMissingSqlObject(Exception ex, string objectName)
        {
            if (ex is SqlException sqlException)
            {
                if (sqlException.Number == 208)
                {
                    return true;
                }

                return sqlException.Message.Contains(objectName, StringComparison.OrdinalIgnoreCase)
                    && sqlException.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase);
            }

            return ex.Message.Contains(objectName, StringComparison.OrdinalIgnoreCase)
                && ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase);
        }
    }
}
