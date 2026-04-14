using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace aqua_api.Modules.Stock.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockImageController : ControllerBase
    {
        private readonly IStockImageService _stockImageService;
        private readonly ILocalizationService _localizationService;

        public StockImageController(IStockImageService stockImageService, ILocalizationService localizationService)
        {
            _stockImageService = stockImageService;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<StockImageCreateDto> imageDtos)
        {

            var result = await _stockImageService.AddImagesAsync(imageDtos);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("upload/{stockId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(
            [FromRoute] long stockId,
            List<IFormFile> files,
            [FromForm] List<string>? altTexts = null)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileRequired"),
                    _localizationService.GetLocalizedString("FileUploadService.FileRequired"),
                    400));
            }

            var result = await _stockImageService.UploadImagesAsync(stockId, files, altTexts);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-stock/{stockId}")]
        public async Task<IActionResult> GetByStockId(long stockId)
        {
            var result = await _stockImageService.GetByStockIdAsync(stockId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _stockImageService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("set-primary/{id}")]
        public async Task<IActionResult> SetPrimary(long id)
        {
            var result = await _stockImageService.SetPrimaryImageAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
