namespace aqua_api.Modules.Budget.Application.Services
{
    public interface IBudgetWaterTemperatureService
    {
        Task<ApiResponse<BudgetWaterTemperatureDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BudgetWaterTemperatureDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BudgetWaterTemperatureDto>> CreateAsync(CreateBudgetWaterTemperatureDto dto);
        Task<ApiResponse<BudgetWaterTemperatureDto>> UpdateAsync(long id, UpdateBudgetWaterTemperatureDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
