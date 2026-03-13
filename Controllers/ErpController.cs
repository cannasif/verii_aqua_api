using Microsoft.AspNetCore.Mvc;
using aqua_api.Interfaces;
using aqua_api.DTOs;
using aqua_api.DTOs.ErpDto;
using Microsoft.AspNetCore.Authorization;

namespace aqua_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ErpController : ControllerBase
    {
        private readonly IErpService _IErpService;
        private readonly ILocalizationService _localizationService;

        public ErpController(IErpService erpService, ILocalizationService localizationService)
        {
            _IErpService = erpService;
            _localizationService = localizationService;
        }

        [HttpGet("getAllCustomers")]
        public async Task<ActionResult<ApiResponse<List<CariDto>>>> GetCaris([FromQuery] string? cariKodu = null)
        {
            var result = await _IErpService.GetCarisAsync(cariKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getAllProducts")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<StokDto>>>> GetStoks([FromQuery] string? stokKodu = null)
        {
            var result = await _IErpService.GetStoksAsync(stokKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getBranches")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranches([FromQuery] int? branchNo = null)
        {
            var result = await _IErpService.GetBranchesAsync(branchNo);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getExchangeRate")]
        public async Task<ActionResult<ApiResponse<List<KurDto>>>> GetExchangeRate(
            [FromQuery] DateTime tarih,
            [FromQuery] int fiyatTipi)
        {
            var result = await _IErpService.GetExchangeRateAsync(tarih, fiyatTipi);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getStokGroup")]
        public async Task<ActionResult<ApiResponse<List<StokGroupDto>>>> GetStokGroup([FromQuery] string? grupKodu)
        {
            var result = await _IErpService.GetStokGroupAsync(grupKodu);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getErpShippingAddress")]
        public async Task<ActionResult<ApiResponse<List<ErpShippingAddressDto>>>> GetErpShippingAddress([FromQuery] string customerCode)
        {
            var result = await _IErpService.GetErpShippingAddressAsync(customerCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getProjectCodes")]
        public async Task<ActionResult<ApiResponse<List<ProjeDto>>>> GetProjectCodes()
        {
            var result = await _IErpService.GetProjectCodesAsync();
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
