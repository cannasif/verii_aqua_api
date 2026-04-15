using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aqua_api.Modules.Aqua.Api
{
    [ApiController]
    [Route("api/aqua/posting")]
    [Authorize]
    public class AquaPostingController : ControllerBase
    {
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly ITransferService _transferService;
        private readonly IMortalityService _mortalityService;
        private readonly IWeighingService _weighingService;
        private readonly IStockConvertService _stockConvertService;
        private readonly IShipmentService _shipmentService;
        private readonly IWarehouseTransferService _warehouseTransferService;
        private readonly ICageWarehouseTransferService _cageWarehouseTransferService;
        private readonly IWarehouseCageTransferService _warehouseCageTransferService;
        private readonly INetOperationService _netOperationService;
        private readonly IDailyWeatherService _dailyWeatherService;

        public AquaPostingController(
            IGoodsReceiptService goodsReceiptService,
            ITransferService transferService,
            IMortalityService mortalityService,
            IWeighingService weighingService,
            IStockConvertService stockConvertService,
            IShipmentService shipmentService,
            IWarehouseTransferService warehouseTransferService,
            ICageWarehouseTransferService cageWarehouseTransferService,
            IWarehouseCageTransferService warehouseCageTransferService,
            INetOperationService netOperationService,
            IDailyWeatherService dailyWeatherService)
        {
            _goodsReceiptService = goodsReceiptService;
            _transferService = transferService;
            _mortalityService = mortalityService;
            _weighingService = weighingService;
            _stockConvertService = stockConvertService;
            _shipmentService = shipmentService;
            _warehouseTransferService = warehouseTransferService;
            _cageWarehouseTransferService = cageWarehouseTransferService;
            _warehouseCageTransferService = warehouseCageTransferService;
            _netOperationService = netOperationService;
            _dailyWeatherService = dailyWeatherService;
        }

        [HttpPost("goods-receipt/{id:long}")]
        public async Task<IActionResult> PostGoodsReceipt(long id)
        {
            var result = await _goodsReceiptService.PostAsync(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("transfer/{id:long}")]
        public async Task<IActionResult> PostTransfer(long id)
        {
            var result = await _transferService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("mortality/{id:long}")]
        public async Task<IActionResult> PostMortality(long id)
        {
            var result = await _mortalityService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("weighing/{id:long}")]
        public async Task<IActionResult> PostWeighing(long id)
        {
            var result = await _weighingService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("stock-convert/{id:long}")]
        public async Task<IActionResult> PostStockConvert(long id)
        {
            var result = await _stockConvertService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("shipment/{id:long}")]
        public async Task<IActionResult> PostShipment(long id)
        {
            var result = await _shipmentService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("warehouse-transfer/{id:long}")]
        public async Task<IActionResult> PostWarehouseTransfer(long id)
        {
            var result = await _warehouseTransferService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("cage-warehouse-transfer/{id:long}")]
        public async Task<IActionResult> PostCageWarehouseTransfer(long id)
        {
            var result = await _cageWarehouseTransferService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("warehouse-cage-transfer/{id:long}")]
        public async Task<IActionResult> PostWarehouseCageTransfer(long id)
        {
            var result = await _warehouseCageTransferService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("net-operation/{id:long}")]
        public async Task<IActionResult> PostNetOperation(long id)
        {
            var result = await _netOperationService.Post(id, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("daily-weather")]
        public async Task<IActionResult> CreateDailyWeather([FromBody] CreateDailyWeatherRequest request)
        {
            var result = await _dailyWeatherService.CreateDaily(request, GetUserId());
            return StatusCode(result.StatusCode, result);
        }

        private long GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(raw, out var userId) ? userId : 1L;
        }
    }
}
