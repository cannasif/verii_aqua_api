using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Integrations.Api
{
    /// <summary>
    /// Netsis/ERP read API parity layer aligned with CRM.
    /// Existing /api/Erp endpoints remain for backward compatibility.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NetsisReadController : ControllerBase
    {
        private readonly INetsisReadService _netsisReadService;
        private readonly ILocalizationService _localizationService;

        public NetsisReadController(INetsisReadService netsisReadService, ILocalizationService localizationService)
        {
            _netsisReadService = netsisReadService;
            _localizationService = localizationService;
        }

        [HttpGet("getAllCustomers")]
        public async Task<ActionResult<ApiResponse<List<CariDto>>>> GetCustomers([FromQuery] string? cariKodu = null)
        {
            var result = await _netsisReadService.GetCustomersAsync(cariKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getAllCustomers/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CariDto>>>> GetCustomersPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _netsisReadService.GetCustomersAsync(null);
            var paged = ToPagedResponse(result, pageNumber, pageSize, search, x => new[]
            {
                x.CariKod,
                x.CariIsim,
                x.CariTel,
                x.CariIl,
                x.CariIlce,
                x.Email,
                x.VergiNumarasi,
            });

            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getAllProducts")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<StokFunctionDto>>>> GetStocks([FromQuery] string? stokKodu = null)
        {
            var result = await _netsisReadService.GetStocksAsync(stokKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getAllProducts/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<StokFunctionDto>>>> GetStocksPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _netsisReadService.GetStocksAsync(null);
            var paged = ToPagedResponse(result, pageNumber, pageSize, search, x => new[]
            {
                x.StokKodu,
                x.StokAdi,
                x.GrupKodu,
                x.GrupIsim,
                x.OlcuBr1,
                x.UreticiKodu,
            });

            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getAllWarehouses")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<DepoDto>>>> GetAllWarehouses([FromQuery] short? depoKodu = null)
        {
            var result = await _netsisReadService.GetWarehousesAsync(depoKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getAllWarehouses/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<DepoDto>>>> GetWarehousesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            short? warehouseCode = short.TryParse(search, out var parsedCode) ? parsedCode : null;
            var result = await _netsisReadService.GetWarehousesAsync(warehouseCode);
            var paged = ToPagedResponse(result, pageNumber, pageSize, search, x => new[]
            {
                x.DepoKodu.ToString(),
                x.DepoIsmi,
                x.CariKodu,
                x.SubeKodu.ToString(),
            });

            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getWarehouses")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<DepoDto>>>> GetWarehouses([FromQuery] short? depoKodu = null)
        {
            var result = await _netsisReadService.GetWarehousesAsync(depoKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getBranches")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranches([FromQuery] int? branchNo = null)
        {
            var result = await _netsisReadService.GetBranchesAsync(branchNo);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getBranches/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<BranchDto>>>> GetBranchesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            int? branchNo = int.TryParse(search, out var parsedCode) ? parsedCode : null;
            var result = await _netsisReadService.GetBranchesAsync(branchNo);
            var paged = ToPagedResponse(result, pageNumber, pageSize, search, x => new[]
            {
                x.SubeKodu.ToString(),
                x.Unvan,
            });

            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getExchangeRate")]
        public async Task<ActionResult<ApiResponse<List<KurDto>>>> GetExchangeRate([FromQuery] DateTime tarih, [FromQuery] int fiyatTipi)
        {
            var result = await _netsisReadService.GetExchangeRatesAsync(tarih, fiyatTipi);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getStokGroup")]
        public async Task<ActionResult<ApiResponse<List<StokGroupDto>>>> GetStokGroup([FromQuery] string? grupKodu)
        {
            var result = await _netsisReadService.GetStockGroupsAsync(grupKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getErpShippingAddress")]
        public async Task<ActionResult<ApiResponse<List<ErpShippingAddressDto>>>> GetErpShippingAddress([FromQuery] string customerCode)
        {
            var result = await _netsisReadService.GetShippingAddressesAsync(customerCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getProjectCodes")]
        public async Task<ActionResult<ApiResponse<List<ProjeDto>>>> GetProjectCodes()
        {
            var result = await _netsisReadService.GetProjectsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("health-check")]
        [AllowAnonymous]
        public IActionResult HealthCheckPublic()
        {
            var healthResponse = new { Status = _localizationService.GetLocalizedString("General.Healthy"), Timestamp = DateTime.UtcNow };
            return StatusCode(200, healthResponse);
        }

        private static ApiResponse<PagedResponse<T>> ToPagedResponse<T>(
            ApiResponse<List<T>> source,
            int pageNumber,
            int pageSize,
            string? search,
            Func<T, IEnumerable<string?>> searchFields)
        {
            if (!source.Success || source.Data == null)
            {
                return ApiResponse<PagedResponse<T>>.ErrorResult(
                    source.Message,
                    source.ExceptionMessage,
                    source.StatusCode);
            }

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            IEnumerable<T> query = source.Data;
            var normalizedSearch = search?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(item => searchFields(item)
                    .Any(value => value?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) == true));
            }

            var materialized = query.ToList();
            var items = materialized
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return ApiResponse<PagedResponse<T>>.SuccessResult(
                new PagedResponse<T>
                {
                    Items = items,
                    TotalCount = materialized.Count,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                source.Message);
        }
    }
}
