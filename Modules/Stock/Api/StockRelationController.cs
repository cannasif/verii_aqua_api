using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aqua_api.Modules.Stock.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockRelationController : ControllerBase
    {
        private readonly IStockRelationService _stockRelationService;
        private readonly ILocalizationService _localizationService;

        public StockRelationController(IStockRelationService stockRelationService, ILocalizationService localizationService)
        {
            _stockRelationService = stockRelationService;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StockRelationCreateDto relationDto)
        {

            var result = await _stockRelationService.CreateAsync(relationDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-stock/{stockId}")]
        public async Task<IActionResult> GetByStockId(long stockId)
        {
            var result = await _stockRelationService.GetByStockIdAsync(stockId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _stockRelationService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}