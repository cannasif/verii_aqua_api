using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.Integrations.Api;

/// <summary>
/// Aqua'nın yerel ERP mal kabul/sevkiyat hareket aynasını sorgular.
/// Netsis fonksiyon okumalarından farklı olarak bu kaynak gerçek veritabanı sayfalaması destekler.
/// </summary>
[ApiController]
[Route("api/erp-receipt-shipment-movements")]
[Authorize]
public sealed class ErpReceiptShipmentMovementsController : ControllerBase
{
    private readonly INetsisReadService _netsisReadService;

    public ErpReceiptShipmentMovementsController(INetsisReadService netsisReadService)
    {
        _netsisReadService = netsisReadService;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>>> GetPaged(
        [FromBody] PagedRequest request)
    {
        var result = await _netsisReadService.GetReceiptShipmentMovementMirrorPagedAsync(request);
        return StatusCode(result.StatusCode, result);
    }
}
