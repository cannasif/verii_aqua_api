using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aqua_api.Modules.BudgetPlanning.Api;

[ApiController]
[Route("api/budget/Planning")]
[Authorize]
public class BudgetPlanningController : ControllerBase
{
    private readonly IBudgetPlanningService _service;

    public BudgetPlanningController(IBudgetPlanningService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<BudgetPlanDto>>>> GetPlans([FromQuery] PagedRequest request)
    {
        var result = await _service.GetPlansAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetPlanDto>>> GetPlan(long id)
    {
        var result = await _service.GetPlanAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetPlanDto>>> CreatePlan([FromBody] CreateBudgetPlanDto dto)
    {
        var result = await _service.CreatePlanAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{sourceBudgetPlanId:long}/copy")]
    public async Task<ActionResult<ApiResponse<BudgetPlanDto>>> CopyPlan(long sourceBudgetPlanId, [FromBody] CopyBudgetPlanDto dto)
    {
        var result = await _service.CopyPlanAsync(sourceBudgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("available-fish-batches")]
    public async Task<ActionResult<ApiResponse<List<BudgetAvailableFishBatchDto>>>> GetAvailableFishBatches()
    {
        var result = await _service.GetAvailableFishBatchesAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/fish-batches")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFishBatchDto>>>> GetPlanFishBatches(long budgetPlanId)
    {
        var result = await _service.GetPlanFishBatchesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/fish-batch-adjustments")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFishBatchAdjustmentDto>>>> GetFishBatchAdjustments(long budgetPlanId)
    {
        var result = await _service.GetFishBatchAdjustmentsAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/actual-fish-batches")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFishBatchDto>>>> AddActualFishBatches(long budgetPlanId, [FromBody] AddActualFishBatchesToBudgetDto dto)
    {
        var result = await _service.AddActualFishBatchesAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/virtual-fish-batches")]
    public async Task<ActionResult<ApiResponse<BudgetPlanFishBatchDto>>> AddVirtualFishBatch(long budgetPlanId, [FromBody] AddVirtualFishBatchDto dto)
    {
        var result = await _service.AddVirtualFishBatchAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/fish-batch-adjustments")]
    public async Task<ActionResult<ApiResponse<BudgetPlanFishBatchAdjustmentDto>>> CreateFishBatchAdjustment(long budgetPlanId, [FromBody] CreateBudgetPlanFishBatchAdjustmentDto dto)
    {
        var result = await _service.CreateFishBatchAdjustmentAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/sales-lines")]
    public async Task<ActionResult<ApiResponse<BudgetPlanSalesLineDto>>> UpsertSalesLine(long budgetPlanId, [FromBody] UpsertBudgetPlanSalesLineDto dto)
    {
        var result = await _service.UpsertSalesLineAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/sales-ton-lines")]
    public async Task<ActionResult<ApiResponse<BudgetPlanSalesLineDto>>> UpsertSalesTon(long budgetPlanId, [FromBody] UpsertBudgetPlanSalesTonDto dto)
    {
        var result = await _service.UpsertSalesTonAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/sales-planning-rows")]
    public async Task<ActionResult<ApiResponse<List<BudgetSalesPlanningRowDto>>>> GetSalesPlanningRows(long budgetPlanId)
    {
        var result = await _service.GetSalesPlanningRowsAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/sales-lines")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanSalesLineDto>>>> GetSalesLines(long budgetPlanId)
    {
        var result = await _service.GetSalesLinesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/exchange-rates")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanExchangeRateDto>>>> GetExchangeRates(long budgetPlanId)
    {
        var result = await _service.GetExchangeRatesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/exchange-rates/generate")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanExchangeRateDto>>>> GenerateExchangeRates(long budgetPlanId, [FromBody] GenerateBudgetPlanExchangeRatesDto dto)
    {
        var result = await _service.GenerateExchangeRatesAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/exchange-rates")]
    public async Task<ActionResult<ApiResponse<BudgetPlanExchangeRateDto>>> UpsertExchangeRate(long budgetPlanId, [FromBody] UpsertBudgetPlanExchangeRateDto dto)
    {
        var result = await _service.UpsertExchangeRateAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/fish-prices")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFishPriceDto>>>> GetFishPrices(long budgetPlanId)
    {
        var result = await _service.GetFishPricesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/fish-prices/generate")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFishPriceDto>>>> GenerateFishPrices(long budgetPlanId, [FromBody] GenerateBudgetPlanFishPricesDto dto)
    {
        var result = await _service.GenerateFishPricesAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/fish-prices")]
    public async Task<ActionResult<ApiResponse<BudgetPlanFishPriceDto>>> UpsertFishPrice(long budgetPlanId, [FromBody] UpsertBudgetPlanFishPriceDto dto)
    {
        var result = await _service.UpsertFishPriceAsync(budgetPlanId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/calculate")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>>> Calculate(long budgetPlanId)
    {
        var result = await _service.CalculateAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{budgetPlanId:long}/calculate-growth")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>>> CalculateGrowth(long budgetPlanId)
    {
        var result = await _service.CalculateGrowthAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/projections")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>>> GetProjections(long budgetPlanId)
    {
        var result = await _service.GetProjectionsAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/feeding-lines")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanFeedingLineDto>>>> GetFeedingLines(long budgetPlanId)
    {
        var result = await _service.GetFeedingLinesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/mortality-lines")]
    public async Task<ActionResult<ApiResponse<List<BudgetPlanMortalityLineDto>>>> GetMortalityLines(long budgetPlanId)
    {
        var result = await _service.GetMortalityLinesAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{budgetPlanId:long}/kpi-summary")]
    public async Task<ActionResult<ApiResponse<BudgetKpiSummaryDto>>> GetKpiSummary(long budgetPlanId)
    {
        var result = await _service.GetKpiSummaryAsync(budgetPlanId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("mortality-rates")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BudgetMortalityRateDefinitionDto>>>> GetMortalityRates([FromQuery] PagedRequest request)
    {
        var result = await _service.GetMortalityRatesAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("mortality-rates/{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetMortalityRateDefinitionDto>>> GetMortalityRate(long id)
    {
        var result = await _service.GetMortalityRateAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("mortality-rates")]
    public async Task<ActionResult<ApiResponse<BudgetMortalityRateDefinitionDto>>> CreateMortalityRate([FromBody] CreateBudgetMortalityRateDefinitionDto dto)
    {
        var result = await _service.CreateMortalityRateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("mortality-rates/{id:long}")]
    public async Task<ActionResult<ApiResponse<BudgetMortalityRateDefinitionDto>>> UpdateMortalityRate(long id, [FromBody] CreateBudgetMortalityRateDefinitionDto dto)
    {
        var result = await _service.UpdateMortalityRateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("mortality-rates/{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMortalityRate(long id)
    {
        var result = await _service.DeleteMortalityRateAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
