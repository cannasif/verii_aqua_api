
namespace aqua_api.Modules.Stock.Application.Services
{
    public interface IStockRelationService
    {
        Task<ApiResponse<StockRelationDto>> CreateAsync(StockRelationCreateDto relationDto);
        Task<ApiResponse<List<StockRelationDto>>> GetByStockIdAsync(long stockId);
        Task<ApiResponse<object>> DeleteAsync(long id);
    }
}
