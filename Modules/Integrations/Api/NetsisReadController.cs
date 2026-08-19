using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly IBudgetExchangeRateReadService _budgetExchangeRateReadService;
        private readonly IErpReceiptResyncService _erpReceiptResyncService;
        private readonly ILocalizationService _localizationService;

        public NetsisReadController(
            INetsisReadService netsisReadService,
            IBudgetExchangeRateReadService budgetExchangeRateReadService,
            IErpReceiptResyncService erpReceiptResyncService,
            ILocalizationService localizationService)
        {
            _netsisReadService = netsisReadService;
            _budgetExchangeRateReadService = budgetExchangeRateReadService;
            _erpReceiptResyncService = erpReceiptResyncService;
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
            [FromQuery] PagedRequest request)
        {
            var paged = await _netsisReadService.GetCustomersPagedAsync(request);
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
            [FromQuery] PagedRequest request)
        {
            var paged = await _netsisReadService.GetStocksPagedAsync(request);
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
            [FromQuery] PagedRequest request)
        {
            var paged = await _netsisReadService.GetWarehousesPagedAsync(request);
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
            [FromQuery] PagedRequest request)
        {
            var paged = await _netsisReadService.GetBranchesPagedAsync(request);
            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getExchangeRate")]
        public async Task<ActionResult<ApiResponse<List<KurDto>>>> GetExchangeRate([FromQuery] DateTime tarih, [FromQuery] int fiyatTipi)
        {
            var result = await _netsisReadService.GetExchangeRatesAsync(tarih, fiyatTipi);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getBudgetExchangeRates")]
        public async Task<ActionResult<ApiResponse<List<ErpBudgetExchangeRateDto>>>> GetBudgetExchangeRates(
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth)
        {
            var result = await _budgetExchangeRateReadService.GetBudgetExchangeRatesAsync(startYear, startMonth, endYear, endMonth);
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

        [HttpGet("getGoodsReceiptAndShipmentMovements")]
        public async Task<ActionResult<ApiResponse<List<MalKabulVeSevkiyatDto>>>> GetGoodsReceiptAndShipmentMovements(
            [FromQuery] DateTime? baslangicTarihi = null)
        {
            var result = await _netsisReadService.GetGoodsReceiptAndShipmentMovementsAsync(baslangicTarihi);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getGoodsReceiptAndShipmentMovements/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>>> GetGoodsReceiptAndShipmentMovementsPaged(
            [FromQuery] GoodsReceiptShipmentMovementPagedRequest request)
        {
            var paged = await _netsisReadService.GetGoodsReceiptAndShipmentMovementsPagedAsync(request);
            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("getReceiptShipmentMovementMirror/paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>>> GetReceiptShipmentMovementMirrorPaged(
            [FromQuery] PagedRequest request)
        {
            var paged = await _netsisReadService.GetReceiptShipmentMovementMirrorPagedAsync(request);
            return StatusCode(paged.StatusCode, paged);
        }

        [HttpGet("receipt-shipment-movements/{documentNo}/resync-preview")]
        public async Task<ActionResult<ApiResponse<ErpReceiptResyncPreviewDto>>> PreviewReceiptResync(
            string documentNo,
            [FromQuery] string operationType,
            [FromQuery] string inOutCode = "G")
        {
            var result = await _erpReceiptResyncService.PreviewAsync(documentNo, inOutCode, operationType);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("receipt-shipment-movements/resync")]
        public async Task<ActionResult<ApiResponse<ErpReceiptResyncResultDto>>> ResyncReceipt(
            [FromBody] ErpReceiptResyncRequestDto request)
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = long.TryParse(rawUserId, out var parsedUserId) ? parsedUserId : 1L;
            var result = await _erpReceiptResyncService.ResyncAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("receipt-shipment-movements/{movementId:long}/cancellation-preview")]
        public async Task<ActionResult<ApiResponse<ErpMovementCancellationPreviewDto>>> PreviewMovementCancellation(long movementId)
        {
            var result = await _erpReceiptResyncService.PreviewMovementCancellationAsync(movementId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("receipt-shipment-movements/cancel")]
        public async Task<ActionResult<ApiResponse<ErpMovementCancellationResultDto>>> CancelMovement(
            [FromBody] ErpMovementCancellationRequestDto request)
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = long.TryParse(rawUserId, out var parsedUserId) ? parsedUserId : 1L;
            var result = await _erpReceiptResyncService.CancelMovementAsync(request, userId);
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
