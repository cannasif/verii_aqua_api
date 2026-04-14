
namespace aqua_api.Modules.Stock.Application.Services
{
    public interface IStockImageService
    {
        Task<ApiResponse<List<StockImageDto>>> AddImagesAsync(List<StockImageCreateDto> imageDtos);
        Task<ApiResponse<List<StockImageDto>>> UploadImagesAsync(long stockId, List<IFormFile> files, List<string>? altTexts = null);
        Task<ApiResponse<List<StockImageDto>>> GetByStockIdAsync(long stockId);
        Task<ApiResponse<object>> DeleteAsync(long id);
        Task<ApiResponse<StockImageDto>> SetPrimaryImageAsync(long imageId);
    }
}
