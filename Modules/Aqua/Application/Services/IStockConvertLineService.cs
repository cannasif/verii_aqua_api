
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IStockConvertLineService
    {
        Task<ApiResponse<StockConvertLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<StockConvertLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<StockConvertLineDto>> CreateAsync(CreateStockConvertLineDto dto);
        Task<ApiResponse<StockConvertLineDto>> CreateWithAutoHeaderAsync(CreateStockConvertLineWithAutoHeaderDto dto);
        Task<ApiResponse<StockConvertLineDto>> UpdateAsync(long id, UpdateStockConvertLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
