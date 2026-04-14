
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface ITransferService
    {
        Task<ApiResponse<TransferDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<TransferDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<TransferDto>> CreateAsync(CreateTransferDto dto);
        Task<ApiResponse<TransferDto>> UpdateAsync(long id, UpdateTransferDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long transferId, long userId);
    }
}
