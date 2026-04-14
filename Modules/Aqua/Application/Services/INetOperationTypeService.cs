
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface INetOperationTypeService
    {
        Task<ApiResponse<NetOperationTypeDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<NetOperationTypeDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<NetOperationTypeDto>> CreateAsync(CreateNetOperationTypeDto dto);
        Task<ApiResponse<NetOperationTypeDto>> UpdateAsync(long id, UpdateNetOperationTypeDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
