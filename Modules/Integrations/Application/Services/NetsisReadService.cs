using AutoMapper;
using aqua_api.Modules.Integrations.Domain.Erp;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Integrations.Application.Services
{
    /// <summary>
    /// Read-oriented Netsis facade modeled after CRM's Netsis module.
    /// Existing Aqua ERP endpoints continue to work through an adapter layer.
    /// </summary>
    public class NetsisReadService : INetsisReadService
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

        public async Task<ApiResponse<PagedResponse<DepoDto>>> GetWarehousesPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDirection)
        {
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                var branchCode = ResolvePositiveBranchCode();
                var query = _dbContext.Warehouses.AsNoTracking();

                if (branchCode.HasValue)
                {
                    query = query.Where(x => x.BranchCode == branchCode.Value);
                }

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    short? warehouseCode = short.TryParse(normalizedSearch, out var parsedWarehouseCode) ? parsedWarehouseCode : null;
                    int? searchedBranchCode = int.TryParse(normalizedSearch, out var parsedBranchCode) ? parsedBranchCode : null;

                    query = query.Where(x =>
                        (warehouseCode.HasValue && x.ErpWarehouseCode == warehouseCode.Value)
                        || EF.Functions.Like(x.WarehouseName, pattern)
                        || (x.CustomerCode != null && EF.Functions.Like(x.CustomerCode, pattern))
                        || (searchedBranchCode.HasValue && x.BranchCode == searchedBranchCode.Value));
                }

                query = ApplyWarehouseSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(rows.Select(MapWarehouseMirror).ToList(), totalCount, paging.PageNumber, paging.PageSize, "ErpService.DepoRecordsRetrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<DepoDto>>.ErrorResult(
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

        public async Task<ApiResponse<PagedResponse<CariDto>>> GetCustomersPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDirection)
        {
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var query = _dbContext.RII_FN_CARI
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_CARI({0}, {1})",
                        DBNull.Value,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking();

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    query = query.Where(x =>
                        EF.Functions.Like(x.CARI_KOD, pattern)
                        || (x.CARI_ISIM != null && EF.Functions.Like(x.CARI_ISIM, pattern))
                        || (x.CARI_TEL != null && EF.Functions.Like(x.CARI_TEL, pattern))
                        || (x.CARI_IL != null && EF.Functions.Like(x.CARI_IL, pattern))
                        || (x.CARI_ILCE != null && EF.Functions.Like(x.CARI_ILCE, pattern))
                        || (x.EMAIL != null && EF.Functions.Like(x.EMAIL, pattern))
                        || (x.VERGI_NUMARASI != null && EF.Functions.Like(x.VERGI_NUMARASI, pattern)));
                }

                query = ApplyCustomerSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(_mapper.Map<List<CariDto>>(rows), totalCount, paging.PageNumber, paging.PageSize, "ErpService.CariRecordsRetrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CariDto>>.ErrorResult(
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

        public async Task<ApiResponse<PagedResponse<StokFunctionDto>>> GetStocksPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDirection)
        {
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                var branchCode = ResolvePositiveBranchCode();
                var query = _dbContext.Stocks.AsNoTracking();

                if (branchCode.HasValue)
                {
                    query = query.Where(x => x.BranchCode == branchCode.Value);
                }

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    query = query.Where(x =>
                        EF.Functions.Like(x.ErpStockCode, pattern)
                        || EF.Functions.Like(x.StockName, pattern)
                        || (x.GrupKodu != null && EF.Functions.Like(x.GrupKodu, pattern))
                        || (x.GrupAdi != null && EF.Functions.Like(x.GrupAdi, pattern))
                        || (x.Unit != null && EF.Functions.Like(x.Unit, pattern))
                        || (x.UreticiKodu != null && EF.Functions.Like(x.UreticiKodu, pattern)));
                }

                query = ApplyStockSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(rows.Select(MapStockMirror).ToList(), totalCount, paging.PageNumber, paging.PageSize, "ErpService.StokRecordsRetrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StokFunctionDto>>.ErrorResult(
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

        public async Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDirection)
        {
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                int? branchNo = int.TryParse(search, out var parsedCode) ? parsedCode : null;

                var query = _dbContext.Set<RII_FN_BRANCHES>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_BRANCHES({0})",
                        branchNo.HasValue ? branchNo.Value : DBNull.Value)
                    .AsNoTracking();

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    query = query.Where(x =>
                        EF.Functions.Like(x.SUBE_KODU.ToString(), pattern)
                        || (x.UNVAN != null && EF.Functions.Like(x.UNVAN, pattern)));
                }

                query = ApplyBranchSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(_mapper.Map<List<BranchDto>>(rows), totalCount, paging.PageNumber, paging.PageSize, "ErpService.BranchesRetrievedSuccessfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BranchDto>>.ErrorResult(
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

        public async Task<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            string? sortBy,
            string? sortDirection)
        {
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                var query = startDate.HasValue
                    ? _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi({0})", startDate.Value.Date)
                    : _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi(DEFAULT)");

                query = query.AsNoTracking();

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    query = query.Where(x =>
                        (x.FisNo != null && EF.Functions.Like(x.FisNo, pattern))
                        || (x.KafesKodu.HasValue && EF.Functions.Like(x.KafesKodu.Value.ToString(), pattern))
                        || (x.ProjeKodu != null && EF.Functions.Like(x.ProjeKodu, pattern))
                        || EF.Functions.Like(x.StokKodu, pattern)
                        || (x.StokAdi != null && EF.Functions.Like(x.StokAdi, pattern))
                        || EF.Functions.Like(x.HareketTuru, pattern)
                        || EF.Functions.Like(x.GcKodu, pattern)
                        || (x.GrupKodu != null && EF.Functions.Like(x.GrupKodu, pattern))
                        || EF.Functions.Like(x.IslemTuru, pattern));
                }

                query = ApplyGoodsReceiptShipmentSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(_mapper.Map<List<MalKabulVeSevkiyatDto>>(rows), totalCount, paging.PageNumber, paging.PageSize, "ErpService.MalKabulVeSevkiyatRecordsRetrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>.ErrorResult(
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
                        ProcessError = x.ProcessError
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
            try
            {
                var paging = NormalizePaging(pageNumber, pageSize);
                var query = BuildReceiptShipmentMovementMirrorQuery();

                var normalizedSearch = NormalizeSearch(search);
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    var pattern = BuildLikePattern(normalizedSearch);
                    query = query.Where(x =>
                        EF.Functions.Like(x.DocumentNo ?? string.Empty, pattern)
                        || EF.Functions.Like(x.ErpProjectCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.ErpStockCode, pattern)
                        || EF.Functions.Like(x.ErpStockName ?? string.Empty, pattern)
                        || EF.Functions.Like(x.MovementKind, pattern)
                        || EF.Functions.Like(x.InOutCode, pattern)
                        || EF.Functions.Like(x.StockGroupCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.OperationType, pattern)
                        || EF.Functions.Like(x.ProjectCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.ProjectName ?? string.Empty, pattern)
                        || EF.Functions.Like(x.CageCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.CageName ?? string.Empty, pattern)
                        || EF.Functions.Like(x.StockCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.StockName ?? string.Empty, pattern)
                        || EF.Functions.Like(x.BatchCode ?? string.Empty, pattern)
                        || EF.Functions.Like(x.MatchError ?? string.Empty, pattern)
                        || EF.Functions.Like(x.ProcessError ?? string.Empty, pattern));
                }

                query = ApplyReceiptShipmentMovementMirrorSort(query, sortBy, sortDirection);

                var totalCount = await query.CountAsync();
                var rows = await query
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(rows, totalCount, paging.PageNumber, paging.PageSize, "ErpService.ReceiptShipmentMovementMirrorRetrieved");
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
                    ProcessError = x.ProcessError
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            return (Math.Max(pageNumber, 1), Math.Clamp(pageSize, 1, 500));
        }

        private static string? NormalizeSearch(string? search)
        {
            return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        }

        private static string BuildLikePattern(string search)
        {
            return $"%{search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")}%";
        }

        private int? ResolvePositiveBranchCode()
        {
            var branchText = _httpContextAccessor.HttpContext?.Items["BranchCode"]?.ToString();
            return int.TryParse(branchText, out var branchCode) && branchCode > 0 ? branchCode : null;
        }

        private static short ToShort(int value)
        {
            if (value < short.MinValue)
            {
                return short.MinValue;
            }

            if (value > short.MaxValue)
            {
                return short.MaxValue;
            }

            return (short)value;
        }

        private static StokFunctionDto MapStockMirror(StockEntity stock)
        {
            return new StokFunctionDto
            {
                SubeKodu = ToShort(stock.BranchCode),
                IsletmeKodu = 0,
                StokKodu = stock.ErpStockCode,
                StokAdi = stock.StockName,
                OlcuBr1 = stock.Unit,
                UreticiKodu = stock.UreticiKodu,
                GrupKodu = stock.GrupKodu,
                GrupIsim = stock.GrupAdi,
                Kod1 = stock.Kod1,
                Kod1Adi = stock.Kod1Adi,
                Kod2 = stock.Kod2,
                Kod2Adi = stock.Kod2Adi,
                Kod3 = stock.Kod3,
                Kod3Adi = stock.Kod3Adi,
                Kod4 = stock.Kod4,
                Kod4Adi = stock.Kod4Adi,
                Kod5 = stock.Kod5,
                Kod5Adi = stock.Kod5Adi,
            };
        }

        private static DepoDto MapWarehouseMirror(WarehouseEntity warehouse)
        {
            return new DepoDto
            {
                DepoKodu = warehouse.ErpWarehouseCode,
                DepoIsmi = warehouse.WarehouseName,
                CariKodu = warehouse.CustomerCode,
                SubeKodu = ToShort(warehouse.BranchCode),
                DepoKilitLe = warehouse.IsLocked ? 'E' : 'H',
                Eksibakiye = warehouse.AllowNegativeBalance ? 'E' : 'H',
            };
        }

        private static IQueryable<T> ApplySort<T, TKey>(IQueryable<T> query, Expression<Func<T, TKey>> keySelector, string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<RII_FN_CARI> ApplyCustomerSort(IQueryable<RII_FN_CARI> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "CariKod").Trim().ToLowerInvariant() switch
            {
                "cariisim" or "customername" => ApplySort(query, x => x.CARI_ISIM, sortDirection),
                "caritel" or "phone" => ApplySort(query, x => x.CARI_TEL, sortDirection),
                "cariil" or "city" => ApplySort(query, x => x.CARI_IL, sortDirection),
                "cariilce" or "district" => ApplySort(query, x => x.CARI_ILCE, sortDirection),
                "verginumarasi" or "taxnumber" => ApplySort(query, x => x.VERGI_NUMARASI, sortDirection),
                _ => ApplySort(query, x => x.CARI_KOD, sortDirection),
            };
        }

        private static IQueryable<StockEntity> ApplyStockSort(IQueryable<StockEntity> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "StokKodu").Trim().ToLowerInvariant() switch
            {
                "stokadi" or "stockname" => ApplySort(query, x => x.StockName, sortDirection),
                "olcubr1" or "unit" => ApplySort(query, x => x.Unit, sortDirection),
                "grupkodu" or "groupcode" => ApplySort(query, x => x.GrupKodu, sortDirection),
                "grupisim" or "grupadi" or "groupname" => ApplySort(query, x => x.GrupAdi, sortDirection),
                "ureticikodu" or "producercode" => ApplySort(query, x => x.UreticiKodu, sortDirection),
                _ => ApplySort(query, x => x.ErpStockCode, sortDirection),
            };
        }

        private static IQueryable<WarehouseEntity> ApplyWarehouseSort(IQueryable<WarehouseEntity> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "DepoKodu").Trim().ToLowerInvariant() switch
            {
                "depoismi" or "warehousename" => ApplySort(query, x => x.WarehouseName, sortDirection),
                "carikodu" or "customercode" => ApplySort(query, x => x.CustomerCode, sortDirection),
                "subekodu" or "branchcode" => ApplySort(query, x => x.BranchCode, sortDirection),
                "depokilitle" or "locked" => ApplySort(query, x => x.IsLocked, sortDirection),
                "eksibakiye" or "negativebalance" => ApplySort(query, x => x.AllowNegativeBalance, sortDirection),
                _ => ApplySort(query, x => x.ErpWarehouseCode, sortDirection),
            };
        }

        private static IQueryable<RII_FN_BRANCHES> ApplyBranchSort(IQueryable<RII_FN_BRANCHES> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "SubeKodu").Trim().ToLowerInvariant() switch
            {
                "unvan" or "branchname" => ApplySort(query, x => x.UNVAN, sortDirection),
                _ => ApplySort(query, x => x.SUBE_KODU, sortDirection),
            };
        }

        private static IQueryable<RII_FN_MAL_KABUL_VE_SEVKIYAT> ApplyGoodsReceiptShipmentSort(IQueryable<RII_FN_MAL_KABUL_VE_SEVKIYAT> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "Tarih").Trim().ToLowerInvariant() switch
            {
                "fisno" or "documentno" => ApplySort(query, x => x.FisNo, sortDirection),
                "kafeskodu" or "erpwarehousecode" => ApplySort(query, x => x.KafesKodu, sortDirection),
                "projekodu" or "erpprojectcode" => ApplySort(query, x => x.ProjeKodu, sortDirection),
                "stokkodu" or "erpstockcode" => ApplySort(query, x => x.StokKodu, sortDirection),
                "stokadi" or "erpstockname" => ApplySort(query, x => x.StokAdi, sortDirection),
                "miktar" or "quantity" => ApplySort(query, x => x.Miktar, sortDirection),
                "hareketturu" or "movementkind" => ApplySort(query, x => x.HareketTuru, sortDirection),
                "gckodu" or "inoutcode" => ApplySort(query, x => x.GcKodu, sortDirection),
                "grupkodu" or "stockgroupcode" => ApplySort(query, x => x.GrupKodu, sortDirection),
                "islemturu" or "operationtype" => ApplySort(query, x => x.IslemTuru, sortDirection),
                _ => ApplySort(query, x => x.Tarih, string.IsNullOrWhiteSpace(sortDirection) ? "desc" : sortDirection),
            };
        }

        private static IQueryable<ErpReceiptShipmentMovementDto> ApplyReceiptShipmentMovementMirrorSort(IQueryable<ErpReceiptShipmentMovementDto> query, string? sortBy, string? sortDirection)
        {
            return (sortBy ?? "MovementDate").Trim().ToLowerInvariant() switch
            {
                "documentno" => ApplySort(query, x => x.DocumentNo, sortDirection),
                "erpwarehousecode" => ApplySort(query, x => x.ErpWarehouseCode, sortDirection),
                "erpprojectcode" => ApplySort(query, x => x.ErpProjectCode, sortDirection),
                "erpstockcode" => ApplySort(query, x => x.ErpStockCode, sortDirection),
                "erpstockname" => ApplySort(query, x => x.ErpStockName, sortDirection),
                "quantity" => ApplySort(query, x => x.Quantity, sortDirection),
                "movementkind" => ApplySort(query, x => x.MovementKind, sortDirection),
                "inoutcode" => ApplySort(query, x => x.InOutCode, sortDirection),
                "stockgroupcode" => ApplySort(query, x => x.StockGroupCode, sortDirection),
                "operationtype" => ApplySort(query, x => x.OperationType, sortDirection),
                "projectcode" => ApplySort(query, x => x.ProjectCode, sortDirection),
                "projectname" => ApplySort(query, x => x.ProjectName, sortDirection),
                "cagecode" => ApplySort(query, x => x.CageCode, sortDirection),
                "cagename" => ApplySort(query, x => x.CageName, sortDirection),
                "stockcode" => ApplySort(query, x => x.StockCode, sortDirection),
                "batchcode" => ApplySort(query, x => x.BatchCode, sortDirection),
                "ismatched" => ApplySort(query, x => x.IsMatched, sortDirection),
                "isprocessed" => ApplySort(query, x => x.IsProcessed, sortDirection),
                "processingattemptcount" => ApplySort(query, x => x.ProcessingAttemptCount, sortDirection),
                "lastsyncedat" => ApplySort(query, x => x.LastSyncedAt, sortDirection),
                "processerror" => ApplySort(query, x => x.ProcessError, sortDirection),
                _ => ApplySort(query, x => x.MovementDate, string.IsNullOrWhiteSpace(sortDirection) ? "desc" : sortDirection),
            };
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
