
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IGoodsReceiptService
    {
        Task<ApiResponse<GoodsReceiptDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<GoodsReceiptDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto dto);
        Task<ApiResponse<GoodsReceiptDto>> UpdateAsync(long id, UpdateGoodsReceiptDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> PostAsync(long goodsReceiptId, long userId);
    }
}
