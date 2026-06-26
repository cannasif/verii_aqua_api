namespace aqua_api.Modules.Budget.Application.Services
{
    public interface IBudgetCalibrationDefinitionService
    {
        Task<ApiResponse<BudgetCalibrationDefinitionDto>> GetByIdAsync(long id);
        Task<ApiResponse<PagedResponse<BudgetCalibrationDefinitionDto>>> GetAllAsync(PagedRequest request);
        Task<ApiResponse<BudgetCalibrationDefinitionDto>> CreateAsync(CreateBudgetCalibrationDefinitionDto dto);
        Task<ApiResponse<BudgetCalibrationDefinitionDto>> UpdateAsync(long id, UpdateBudgetCalibrationDefinitionDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(long id);
    }
}
