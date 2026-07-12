namespace aqua_api.Modules.BudgetPlanning.Application.Services;

public interface IBudgetPlanningService
{
    Task<ApiResponse<PagedResponse<BudgetPlanDto>>> GetPlansAsync(PagedRequest request);
    Task<ApiResponse<BudgetPlanDto>> GetPlanAsync(long id);
    Task<ApiResponse<BudgetPlanDto>> CreatePlanAsync(CreateBudgetPlanDto dto);
    Task<ApiResponse<BudgetPlanDto>> CopyPlanAsync(long sourceBudgetPlanId, CopyBudgetPlanDto dto);
    Task<ApiResponse<List<BudgetPlanFishBatchDto>>> GetPlanFishBatchesAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanFishBatchAdjustmentDto>>> GetFishBatchAdjustmentsAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetAvailableFishBatchDto>>> GetAvailableFishBatchesAsync();
    Task<ApiResponse<List<BudgetPlanFishBatchDto>>> AddActualFishBatchesAsync(long budgetPlanId, AddActualFishBatchesToBudgetDto dto);
    Task<ApiResponse<BudgetPlanFishBatchDto>> AddVirtualFishBatchAsync(long budgetPlanId, AddVirtualFishBatchDto dto);
    Task<ApiResponse<BudgetPlanFishBatchAdjustmentDto>> CreateFishBatchAdjustmentAsync(long budgetPlanId, CreateBudgetPlanFishBatchAdjustmentDto dto);
    Task<ApiResponse<BudgetPlanSalesLineDto>> UpsertSalesLineAsync(long budgetPlanId, UpsertBudgetPlanSalesLineDto dto);
    Task<ApiResponse<BudgetPlanSalesLineDto>> UpsertSalesTonAsync(long budgetPlanId, UpsertBudgetPlanSalesTonDto dto);
    Task<ApiResponse<List<BudgetPlanSalesLineDto>>> ImportSalesTonsAsync(long budgetPlanId, ImportBudgetPlanSalesTonsDto dto);
    Task<ApiResponse<List<BudgetPlanSalesLineDto>>> GetSalesLinesAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetSalesPlanningRowDto>>> GetSalesPlanningRowsAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanExchangeRateDto>>> GetExchangeRatesAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanExchangeRateDto>>> GenerateExchangeRatesAsync(long budgetPlanId, GenerateBudgetPlanExchangeRatesDto dto);
    Task<ApiResponse<BudgetPlanExchangeRateDto>> UpsertExchangeRateAsync(long budgetPlanId, UpsertBudgetPlanExchangeRateDto dto);
    Task<ApiResponse<List<BudgetPlanFishPriceDto>>> GetFishPricesAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanFishPriceDto>>> GenerateFishPricesAsync(long budgetPlanId, GenerateBudgetPlanFishPricesDto dto);
    Task<ApiResponse<BudgetPlanFishPriceDto>> UpsertFishPriceAsync(long budgetPlanId, UpsertBudgetPlanFishPriceDto dto);
    Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> CalculateGrowthAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> CalculateAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> GetProjectionsAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanFeedingLineDto>>> GetFeedingLinesAsync(long budgetPlanId);
    Task<ApiResponse<List<BudgetPlanMortalityLineDto>>> GetMortalityLinesAsync(long budgetPlanId);
    Task<ApiResponse<BudgetKpiSummaryDto>> GetKpiSummaryAsync(long budgetPlanId);
    Task<ApiResponse<PagedResponse<BudgetMortalityRateDefinitionDto>>> GetMortalityRatesAsync(PagedRequest request);
    Task<ApiResponse<BudgetMortalityRateDefinitionDto>> GetMortalityRateAsync(long id);
    Task<ApiResponse<BudgetMortalityRateDefinitionDto>> CreateMortalityRateAsync(CreateBudgetMortalityRateDefinitionDto dto);
    Task<ApiResponse<BudgetMortalityRateDefinitionDto>> UpdateMortalityRateAsync(long id, CreateBudgetMortalityRateDefinitionDto dto);
    Task<ApiResponse<bool>> DeleteMortalityRateAsync(long id);
}
