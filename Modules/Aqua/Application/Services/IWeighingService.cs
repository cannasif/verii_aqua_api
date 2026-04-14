
namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IWeighingService
    {
        Task<ApiResponse<WeighingDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<WeighingDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<WeighingDto>> CreateAsync(CreateWeighingDto dto);
        Task<ApiResponse<WeighingDto>> UpdateAsync(long id, UpdateWeighingDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long weighingId, long userId);
    }
}
