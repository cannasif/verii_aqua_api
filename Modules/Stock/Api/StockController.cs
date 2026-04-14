using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aqua_api.Modules.Stock.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILocalizationService _localizationService;

        public StockController(IStockService stockService, ILocalizationService localizationService)
        {
            _stockService = stockService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PagedRequest request)
        {
            var result = await _stockService.GetAllStocksAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("withImages")]
        public async Task<IActionResult> GetWithImages([FromQuery] PagedRequest request)
        {
            var result = await _stockService.GetAllStocksWithImagesAsync(request);
            return StatusCode(result.StatusCode, result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _stockService.GetStockByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StockCreateDto stockCreateDto)
        {

            var result = await _stockService.CreateStockAsync(stockCreateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] StockUpdateDto stockUpdateDto)
        {

            var result = await _stockService.UpdateStockAsync(id, stockUpdateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _stockService.DeleteStockAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}