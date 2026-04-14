
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface ITransferLineService
    {
        Task<ApiResponse<TransferLineDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<TransferLineDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<TransferLineDto>> CreateAsync(CreateTransferLineDto dto);
        Task<ApiResponse<TransferLineDto>> UpdateAsync(long id, UpdateTransferLineDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
