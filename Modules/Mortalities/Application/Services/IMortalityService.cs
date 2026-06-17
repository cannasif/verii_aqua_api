
namespace aqua_api.Modules.Mortalities.Application.Services
{
    public interface IMortalityService
    {
        Task<ApiResponse<MortalityDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<MortalityDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<MortalityDto>> CreateAsync(CreateMortalityDto dto);
        Task<ApiResponse<MortalityDto>> UpdateAsync(long id, UpdateMortalityDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
        Task<ApiResponse<bool>> Post(long mortalityId, long userId);
        Task<ApiResponse<bool>> PostAquaAndQueueErpAsync(long mortalityId, long userId);
        Task<int> ProcessPendingErpIntegrationsAsync(DateTime operationDate, long userId);
    }
}
