using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aqua_api.Modules.Stock.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockDetailController : ControllerBase
    {
        private readonly IStockDetailService _stockDetailService;
        private readonly ILocalizationService _localizationService;

        public StockDetailController(IStockDetailService stockDetailService, ILocalizationService localizationService)
        {
            _stockDetailService = stockDetailService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PagedRequest request)
        {
            var result = await _stockDetailService.GetAllStockDetailsAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("stock/{stockId:long}")]
        public async Task<IActionResult> GetByStockId(long stockId)
        {
            var result = await _stockDetailService.GetStockDetailByStockIdAsync(stockId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _stockDetailService.GetStockDetailByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StockDetailCreateDto stockDetailCreateDto)
        {

            var result = await _stockDetailService.CreateStockDetailAsync(stockDetailCreateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] StockDetailUpdateDto stockDetailUpdateDto)
        {

            var result = await _stockDetailService.UpdateStockDetailAsync(id, stockDetailUpdateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _stockDetailService.DeleteStockDetailAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}