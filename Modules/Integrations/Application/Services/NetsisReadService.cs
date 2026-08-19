using AutoMapper;
using aqua_api.Modules.Integrations.Domain.Erp;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
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
    public class NetsisReadService : INetsisReadService, IBudgetExchangeRateReadService
    {
        private static readonly IReadOnlyDictionary<string, string> CustomerGridColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(CariDto.SubeKodu)] = nameof(RII_FN_CARI.SUBE_KODU),
                [nameof(CariDto.IsletmeKodu)] = nameof(RII_FN_CARI.ISLETME_KODU),
                [nameof(CariDto.CariKod)] = nameof(RII_FN_CARI.CARI_KOD),
                [nameof(CariDto.CariIsim)] = nameof(RII_FN_CARI.CARI_ISIM),
                [nameof(CariDto.CariTel)] = nameof(RII_FN_CARI.CARI_TEL),
                [nameof(CariDto.CariIl)] = nameof(RII_FN_CARI.CARI_IL),
                [nameof(CariDto.CariAdres)] = nameof(RII_FN_CARI.CARI_ADRES),
                [nameof(CariDto.CariIlce)] = nameof(RII_FN_CARI.CARI_ILCE),
                [nameof(CariDto.UlkeKodu)] = nameof(RII_FN_CARI.ULKE_KODU),
                [nameof(CariDto.Email)] = nameof(RII_FN_CARI.EMAIL),
                [nameof(CariDto.Web)] = nameof(RII_FN_CARI.WEB),
                [nameof(CariDto.VergiNumarasi)] = nameof(RII_FN_CARI.VERGI_NUMARASI),
                [nameof(CariDto.VergiDairesi)] = nameof(RII_FN_CARI.VERGI_DAIRESI),
                [nameof(CariDto.TcknNumber)] = nameof(RII_FN_CARI.TCKIMLIKNO),
            };

        private static readonly IReadOnlyDictionary<string, string> StockGridColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(StokFunctionDto.SubeKodu)] = nameof(StockEntity.BranchCode),
                [nameof(StokFunctionDto.StokKodu)] = nameof(StockEntity.ErpStockCode),
                [nameof(StokFunctionDto.StokAdi)] = nameof(StockEntity.StockName),
                [nameof(StokFunctionDto.OlcuBr1)] = nameof(StockEntity.Unit),
                [nameof(StokFunctionDto.UreticiKodu)] = nameof(StockEntity.UreticiKodu),
                [nameof(StokFunctionDto.GrupKodu)] = nameof(StockEntity.GrupKodu),
                [nameof(StokFunctionDto.GrupIsim)] = nameof(StockEntity.GrupAdi),
                [nameof(StokFunctionDto.Kod1)] = nameof(StockEntity.Kod1),
                [nameof(StokFunctionDto.Kod1Adi)] = nameof(StockEntity.Kod1Adi),
                [nameof(StokFunctionDto.Kod2)] = nameof(StockEntity.Kod2),
                [nameof(StokFunctionDto.Kod2Adi)] = nameof(StockEntity.Kod2Adi),
                [nameof(StokFunctionDto.Kod3)] = nameof(StockEntity.Kod3),
                [nameof(StokFunctionDto.Kod3Adi)] = nameof(StockEntity.Kod3Adi),
                [nameof(StokFunctionDto.Kod4)] = nameof(StockEntity.Kod4),
                [nameof(StokFunctionDto.Kod4Adi)] = nameof(StockEntity.Kod4Adi),
                [nameof(StokFunctionDto.Kod5)] = nameof(StockEntity.Kod5),
                [nameof(StokFunctionDto.Kod5Adi)] = nameof(StockEntity.Kod5Adi),
            };

        private static readonly IReadOnlyDictionary<string, string> WarehouseGridColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(DepoDto.DepoKodu)] = nameof(WarehouseEntity.ErpWarehouseCode),
                [nameof(DepoDto.DepoIsmi)] = nameof(WarehouseEntity.WarehouseName),
                [nameof(DepoDto.CariKodu)] = nameof(WarehouseEntity.CustomerCode),
                [nameof(DepoDto.SubeKodu)] = nameof(WarehouseEntity.BranchCode),
                [nameof(DepoDto.DepoKilitLe)] = nameof(WarehouseEntity.IsLocked),
                [nameof(DepoDto.Eksibakiye)] = nameof(WarehouseEntity.AllowNegativeBalance),
            };

        private static readonly IReadOnlyDictionary<string, string> BranchGridColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(BranchDto.SubeKodu)] = nameof(RII_FN_BRANCHES.SUBE_KODU),
                [nameof(BranchDto.Unvan)] = nameof(RII_FN_BRANCHES.UNVAN),
            };

        private static readonly IReadOnlyDictionary<string, string> GoodsReceiptShipmentGridColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(MalKabulVeSevkiyatDto.Tarih)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.Tarih),
                [nameof(MalKabulVeSevkiyatDto.FisNo)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.FisNo),
                ["DocumentNo"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.FisNo),
                [nameof(MalKabulVeSevkiyatDto.KafesKodu)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.KafesKodu),
                ["ErpWarehouseCode"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.KafesKodu),
                [nameof(MalKabulVeSevkiyatDto.ProjeKodu)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.ProjeKodu),
                ["ErpProjectCode"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.ProjeKodu),
                [nameof(MalKabulVeSevkiyatDto.StokKodu)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.StokKodu),
                ["ErpStockCode"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.StokKodu),
                [nameof(MalKabulVeSevkiyatDto.StokAdi)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.StokAdi),
                ["ErpStockName"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.StokAdi),
                [nameof(MalKabulVeSevkiyatDto.Miktar)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.Miktar),
                ["Quantity"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.Miktar),
                [nameof(MalKabulVeSevkiyatDto.HareketTuru)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.HareketTuru),
                ["MovementKind"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.HareketTuru),
                [nameof(MalKabulVeSevkiyatDto.GcKodu)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.GcKodu),
                ["InOutCode"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.GcKodu),
                [nameof(MalKabulVeSevkiyatDto.GrupKodu)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.GrupKodu),
                ["StockGroupCode"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.GrupKodu),
                [nameof(MalKabulVeSevkiyatDto.IslemTuru)] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.IslemTuru),
                ["OperationType"] = nameof(RII_FN_MAL_KABUL_VE_SEVKIYAT.IslemTuru),
            };

        private static readonly string[] GoodsReceiptShipmentDefaultSearchFields =
        [
            nameof(MalKabulVeSevkiyatDto.FisNo),
            nameof(MalKabulVeSevkiyatDto.KafesKodu),
            nameof(MalKabulVeSevkiyatDto.ProjeKodu),
            nameof(MalKabulVeSevkiyatDto.StokKodu),
            nameof(MalKabulVeSevkiyatDto.StokAdi),
            nameof(MalKabulVeSevkiyatDto.HareketTuru),
            nameof(MalKabulVeSevkiyatDto.GcKodu),
            nameof(MalKabulVeSevkiyatDto.GrupKodu),
            nameof(MalKabulVeSevkiyatDto.IslemTuru),
        ];

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
            return await GetWarehousesPagedAsync(new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public async Task<ApiResponse<PagedResponse<DepoDto>>> GetWarehousesPagedAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                var paging = NormalizePaging(request.PageNumber, request.PageSize);
                var branchCode = ResolvePositiveBranchCode();
                var query = _dbContext.Warehouses.AsNoTracking();

                if (branchCode.HasValue)
                {
                    query = query.Where(x => x.BranchCode == branchCode.Value);
                }

                query = query
                    .ApplySearch(request, WarehouseGridColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic, WarehouseGridColumns);

                query = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(DepoDto.DepoKodu) : request.SortBy,
                    request.SortDirection,
                    WarehouseGridColumns);

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
            return await GetCustomersPagedAsync(new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public async Task<ApiResponse<PagedResponse<CariDto>>> GetCustomersPagedAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                var paging = NormalizePaging(request.PageNumber, request.PageSize);
                var branchFromContext = _httpContextAccessor.HttpContext?.Items["BranchCode"] as string;
                var branchCode = string.IsNullOrWhiteSpace(branchFromContext) ? null : branchFromContext;

                var query = _dbContext.RII_FN_CARI
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_CARI({0}, {1})",
                        DBNull.Value,
                        string.IsNullOrWhiteSpace(branchCode) ? DBNull.Value : branchCode)
                    .AsNoTracking();

                query = query
                    .ApplySearch(request, CustomerGridColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic, CustomerGridColumns);

                query = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CariDto.CariKod) : request.SortBy,
                    request.SortDirection,
                    CustomerGridColumns);

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
            return await GetStocksPagedAsync(new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public async Task<ApiResponse<PagedResponse<StokFunctionDto>>> GetStocksPagedAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                var paging = NormalizePaging(request.PageNumber, request.PageSize);
                var branchCode = ResolvePositiveBranchCode();
                var query = _dbContext.Stocks.AsNoTracking();

                if (branchCode.HasValue)
                {
                    query = query.Where(x => x.BranchCode == branchCode.Value);
                }

                query = query
                    .ApplySearch(request, StockGridColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic, StockGridColumns);

                query = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(StokFunctionDto.StokKodu) : request.SortBy,
                    request.SortDirection,
                    StockGridColumns);

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
            return await GetBranchesPagedAsync(new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public async Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                var paging = NormalizePaging(request.PageNumber, request.PageSize);

                var query = _dbContext.Set<RII_FN_BRANCHES>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.RII_FN_BRANCHES({0})",
                        DBNull.Value)
                    .AsNoTracking();

                query = query
                    .ApplySearch(request, BranchGridColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic, BranchGridColumns);

                query = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BranchDto.SubeKodu) : request.SortBy,
                    request.SortDirection,
                    BranchGridColumns);

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

        public async Task<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            string? sortBy,
            string? sortDirection)
        {
            return await GetGoodsReceiptAndShipmentMovementsPagedAsync(new GoodsReceiptShipmentMovementPagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                BaslangicTarihi = startDate,
                SortBy = sortBy,
                SortDirection = sortDirection,
            });
        }

        public async Task<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsPagedAsync(
            GoodsReceiptShipmentMovementPagedRequest request)
        {
            try
            {
                request ??= new GoodsReceiptShipmentMovementPagedRequest();
                var query = request.BaslangicTarihi.HasValue
                    ? _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi({0})", request.BaslangicTarihi.Value.Date)
                    : _dbContext.RII_FN_MAL_KABUL_VE_SEVKIYAT
                        .FromSqlRaw("SELECT * FROM dbo.fn_MalKabulVeSevkiyatListesi(DEFAULT)");

                query = query
                    .AsNoTracking()
                    .ApplySearch(request, GoodsReceiptShipmentGridColumns, GoodsReceiptShipmentDefaultSearchFields)
                    .ApplyFilters(request.Filters, request.FilterLogic, GoodsReceiptShipmentGridColumns);

                var totalCount = await query.CountAsync();
                var ordered = query.ApplySorting(
                    string.IsNullOrWhiteSpace(request.SortBy) ? nameof(MalKabulVeSevkiyatDto.Tarih) : request.SortBy,
                    request.SortDirection,
                    GoodsReceiptShipmentGridColumns);
                ordered = ((IOrderedQueryable<RII_FN_MAL_KABUL_VE_SEVKIYAT>)ordered)
                    .ThenBy(x => x.FisNo)
                    .ThenBy(x => x.StokKodu)
                    .ThenBy(x => x.ProjeKodu)
                    .ThenBy(x => x.KafesKodu)
                    .ThenBy(x => x.HareketTuru)
                    .ThenBy(x => x.GcKodu)
                    .ThenBy(x => x.IslemTuru);

                var rows = await ordered
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                return ToPagedSuccess(
                    _mapper.Map<List<MalKabulVeSevkiyatDto>>(rows),
                    totalCount,
                    request.PageNumber,
                    request.PageSize,
                    "ErpService.MalKabulVeSevkiyatRecordsRetrieved");
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            return (Math.Max(pageNumber, 1), Math.Clamp(pageSize, 1, 500));
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
                "iscancelled" => ApplySort(query, x => x.IsCancelled, sortDirection),
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
