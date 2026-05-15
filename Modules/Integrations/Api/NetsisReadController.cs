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

        [HttpGet("getAllProducts")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<StokFunctionDto>>>> GetStocks([FromQuery] string? stokKodu = null)
        {
            var result = await _netsisReadService.GetStocksAsync(stokKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getAllWarehouses")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<DepoDto>>>> GetAllWarehouses([FromQuery] short? depoKodu = null)
        {
            var result = await _netsisReadService.GetWarehousesAsync(depoKodu);
            return StatusCode(result.StatusCode, result);
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
    }
}
