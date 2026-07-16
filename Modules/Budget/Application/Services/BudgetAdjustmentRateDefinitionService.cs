using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Budget.Application.Services;

public class BudgetAdjustmentRateDefinitionService : IBudgetAdjustmentRateDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly IReadOnlyDictionary<string, string> FeedMortalityColumnMapping =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["waterTemperatureYear"] = "WaterTemperature.Year",
            ["waterTemperatureMonth"] = "WaterTemperature.Month",
            ["waterTemperatureCelsius"] = "WaterTemperature.WaterTemperatureCelsius",
            ["calibrationCode"] = "CalibrationDefinition.CalibrationCode",
            ["calibrationInfo"] = "CalibrationDefinition.CalibrationInfo",
            ["feedStockCode"] = "FeedStock.ErpStockCode",
            ["feedStockName"] = "FeedStock.StockName"
        };

    private static readonly IReadOnlyDictionary<string, string> GrowthQualityColumnMapping =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fishStockCode"] = "FishStock.ErpStockCode",
            ["fishStockName"] = "FishStock.StockName"
        };

    public BudgetAdjustmentRateDefinitionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResponse<BudgetFeedMortalityRateDto>>> GetFeedMortalityRatesAsync(PagedRequest request)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();
        var query = FeedMortalityQuery().AsNoTracking()
            .ApplyFilters(request.Filters, request.FilterLogic, FeedMortalityColumnMapping);
        query = query.ApplySorting(string.IsNullOrWhiteSpace(request.SortBy) ? "WaterTemperatureId" : request.SortBy, request.SortDirection, FeedMortalityColumnMapping);
        var totalCount = await query.CountAsync();
        var rows = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();
        return ApiResponse<PagedResponse<BudgetFeedMortalityRateDto>>.SuccessResult(new PagedResponse<BudgetFeedMortalityRateDto>
        {
            Items = rows.Select(MapFeedMortality).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        }, "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetFeedMortalityRateDto>> GetFeedMortalityRateAsync(long id)
    {
        var entity = await FeedMortalityQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity == null
            ? ApiResponse<BudgetFeedMortalityRateDto>.ErrorResult("Yem fire orani bulunamadi.", "Yem fire orani bulunamadi.", StatusCodes.Status404NotFound)
            : ApiResponse<BudgetFeedMortalityRateDto>.SuccessResult(MapFeedMortality(entity), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetFeedMortalityRateDto>> CreateFeedMortalityRateAsync(CreateBudgetFeedMortalityRateDto dto)
    {
        var validation = await ValidateFeedMortalityAsync(dto);
        if (!validation.Success) return ApiResponse<BudgetFeedMortalityRateDto>.ErrorResult(validation.Message, validation.Message, validation.StatusCode);
        var entity = new BudgetFeedMortalityRate
        {
            WaterTemperatureId = dto.WaterTemperatureId,
            CalibrationDefinitionId = dto.CalibrationDefinitionId,
            FeedStockId = dto.FeedStockId,
            ReductionRatePercent = dto.ReductionRatePercent,
            Description = Normalize(dto.Description)
        };
        await _unitOfWork.Repository<BudgetFeedMortalityRate>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetFeedMortalityRateAsync(entity.Id);
    }

    public async Task<ApiResponse<BudgetFeedMortalityRateDto>> UpdateFeedMortalityRateAsync(long id, CreateBudgetFeedMortalityRateDto dto)
    {
        var entity = await _unitOfWork.Db.BudgetFeedMortalityRates.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return ApiResponse<BudgetFeedMortalityRateDto>.ErrorResult("Yem fire orani bulunamadi.", "Yem fire orani bulunamadi.", StatusCodes.Status404NotFound);
        var validation = await ValidateFeedMortalityAsync(dto, id);
        if (!validation.Success) return ApiResponse<BudgetFeedMortalityRateDto>.ErrorResult(validation.Message, validation.Message, validation.StatusCode);
        entity.WaterTemperatureId = dto.WaterTemperatureId;
        entity.CalibrationDefinitionId = dto.CalibrationDefinitionId;
        entity.FeedStockId = dto.FeedStockId;
        entity.ReductionRatePercent = dto.ReductionRatePercent;
        entity.Description = Normalize(dto.Description);
        await _unitOfWork.Repository<BudgetFeedMortalityRate>().UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetFeedMortalityRateAsync(entity.Id);
    }

    public async Task<ApiResponse<bool>> DeleteFeedMortalityRateAsync(long id)
    {
        var deleted = await _unitOfWork.Repository<BudgetFeedMortalityRate>().SoftDeleteAsync(id);
        if (!deleted) return ApiResponse<bool>.ErrorResult("Yem fire orani bulunamadi.", "Yem fire orani bulunamadi.", StatusCodes.Status404NotFound);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Yem fire orani silindi.");
    }

    public async Task<ApiResponse<PagedResponse<BudgetFishGrowthQualityDto>>> GetFishGrowthQualitiesAsync(PagedRequest request)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();
        var query = GrowthQualityQuery().AsNoTracking()
            .ApplyFilters(request.Filters, request.FilterLogic, GrowthQualityColumnMapping);
        query = query.ApplySorting(string.IsNullOrWhiteSpace(request.SortBy) ? "FishStockId" : request.SortBy, request.SortDirection, GrowthQualityColumnMapping);
        var totalCount = await query.CountAsync();
        var rows = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();
        return ApiResponse<PagedResponse<BudgetFishGrowthQualityDto>>.SuccessResult(new PagedResponse<BudgetFishGrowthQualityDto>
        {
            Items = rows.Select(MapGrowthQuality).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        }, "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetFishGrowthQualityDto>> GetFishGrowthQualityAsync(long id)
    {
        var entity = await GrowthQualityQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity == null
            ? ApiResponse<BudgetFishGrowthQualityDto>.ErrorResult("Balik buyume kalitesi bulunamadi.", "Balik buyume kalitesi bulunamadi.", StatusCodes.Status404NotFound)
            : ApiResponse<BudgetFishGrowthQualityDto>.SuccessResult(MapGrowthQuality(entity), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetFishGrowthQualityDto>> CreateFishGrowthQualityAsync(CreateBudgetFishGrowthQualityDto dto)
    {
        var validation = await ValidateGrowthQualityAsync(dto);
        if (!validation.Success) return ApiResponse<BudgetFishGrowthQualityDto>.ErrorResult(validation.Message, validation.Message, validation.StatusCode);
        var entity = new BudgetFishGrowthQuality
        {
            FishStockId = dto.FishStockId,
            GrowthMonthNo = dto.GrowthMonthNo,
            QualityPercent = dto.QualityPercent,
            Description = Normalize(dto.Description)
        };
        await _unitOfWork.Repository<BudgetFishGrowthQuality>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetFishGrowthQualityAsync(entity.Id);
    }

    public async Task<ApiResponse<BudgetFishGrowthQualityDto>> UpdateFishGrowthQualityAsync(long id, CreateBudgetFishGrowthQualityDto dto)
    {
        var entity = await _unitOfWork.Db.BudgetFishGrowthQualities.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return ApiResponse<BudgetFishGrowthQualityDto>.ErrorResult("Balik buyume kalitesi bulunamadi.", "Balik buyume kalitesi bulunamadi.", StatusCodes.Status404NotFound);
        var validation = await ValidateGrowthQualityAsync(dto, id);
        if (!validation.Success) return ApiResponse<BudgetFishGrowthQualityDto>.ErrorResult(validation.Message, validation.Message, validation.StatusCode);
        entity.FishStockId = dto.FishStockId;
        entity.GrowthMonthNo = dto.GrowthMonthNo;
        entity.QualityPercent = dto.QualityPercent;
        entity.Description = Normalize(dto.Description);
        await _unitOfWork.Repository<BudgetFishGrowthQuality>().UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetFishGrowthQualityAsync(entity.Id);
    }

    public async Task<ApiResponse<bool>> DeleteFishGrowthQualityAsync(long id)
    {
        var deleted = await _unitOfWork.Repository<BudgetFishGrowthQuality>().SoftDeleteAsync(id);
        if (!deleted) return ApiResponse<bool>.ErrorResult("Balik buyume kalitesi bulunamadi.", "Balik buyume kalitesi bulunamadi.", StatusCodes.Status404NotFound);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Balik buyume kalitesi silindi.");
    }

    private IQueryable<BudgetFeedMortalityRate> FeedMortalityQuery() => _unitOfWork.Db.BudgetFeedMortalityRates
        .Include(x => x.WaterTemperature).Include(x => x.CalibrationDefinition).Include(x => x.FeedStock);

    private IQueryable<BudgetFishGrowthQuality> GrowthQualityQuery() => _unitOfWork.Db.BudgetFishGrowthQualities.Include(x => x.FishStock);

    private async Task<ApiResponse<bool>> ValidateFeedMortalityAsync(CreateBudgetFeedMortalityRateDto dto, long? currentId = null)
    {
        if (dto.WaterTemperatureId <= 0 || dto.CalibrationDefinitionId <= 0 || dto.FeedStockId <= 0)
            return Invalid("Su sicakligi, kalibrasyon ve yem stogu zorunludur.");
        if (dto.ReductionRatePercent < 0 || dto.ReductionRatePercent > 100)
            return Invalid("Yem fire azaltma yuzdesi 0 ile 100 arasinda olmalidir.");
        if (!ValidDescription(dto.Description)) return Invalid("Aciklama en fazla 500 karakter olabilir.");
        if (!await _unitOfWork.Db.BudgetWaterTemperatures.AnyAsync(x => x.Id == dto.WaterTemperatureId)) return Invalid("Su sicakligi tanimi bulunamadi.");
        if (!await _unitOfWork.Db.BudgetCalibrationDefinitions.AnyAsync(x => x.Id == dto.CalibrationDefinitionId)) return Invalid("Kalibrasyon tanimi bulunamadi.");
        if (!await _unitOfWork.Db.Stocks.AnyAsync(x => x.Id == dto.FeedStockId && x.GrupKodu != null && x.GrupKodu.ToUpper() == "YEM")) return Invalid("Secilen stok yem grubunda degildir.");
        var duplicate = await _unitOfWork.Db.BudgetFeedMortalityRates.AnyAsync(x =>
            x.WaterTemperatureId == dto.WaterTemperatureId && x.CalibrationDefinitionId == dto.CalibrationDefinitionId &&
            x.FeedStockId == dto.FeedStockId && (!currentId.HasValue || x.Id != currentId.Value));
        return duplicate ? Conflict("Bu kombinasyon icin yem fire orani zaten var.") : ApiResponse<bool>.SuccessResult(true, "Valid");
    }

    private async Task<ApiResponse<bool>> ValidateGrowthQualityAsync(CreateBudgetFishGrowthQualityDto dto, long? currentId = null)
    {
        if (dto.FishStockId <= 0) return Invalid("Balik stogu zorunludur.");
        if (dto.GrowthMonthNo < 1 || dto.GrowthMonthNo > 120) return Invalid("Buyume ayi 1 ile 120 arasinda olmalidir.");
        if (dto.QualityPercent < 0 || dto.QualityPercent > 100) return Invalid("Kalite yuzdesi 0 ile 100 arasinda olmalidir.");
        if (!ValidDescription(dto.Description)) return Invalid("Aciklama en fazla 500 karakter olabilir.");
        if (!await _unitOfWork.Db.Stocks.AnyAsync(x => x.Id == dto.FishStockId)) return Invalid("Balik stogu bulunamadi.");
        var duplicate = await _unitOfWork.Db.BudgetFishGrowthQualities.AnyAsync(x =>
            x.FishStockId == dto.FishStockId && x.GrowthMonthNo == dto.GrowthMonthNo && (!currentId.HasValue || x.Id != currentId.Value));
        return duplicate ? Conflict("Bu balik ve buyume ayi icin kalite yuzdesi zaten var.") : ApiResponse<bool>.SuccessResult(true, "Valid");
    }

    private static BudgetFeedMortalityRateDto MapFeedMortality(BudgetFeedMortalityRate entity) => new()
    {
        Id = entity.Id, WaterTemperatureId = entity.WaterTemperatureId, WaterTemperatureYear = entity.WaterTemperature?.Year,
        WaterTemperatureMonth = entity.WaterTemperature?.Month, WaterTemperatureCelsius = entity.WaterTemperature?.WaterTemperatureCelsius,
        CalibrationDefinitionId = entity.CalibrationDefinitionId, CalibrationCode = entity.CalibrationDefinition?.CalibrationCode,
        CalibrationInfo = entity.CalibrationDefinition?.CalibrationInfo, FeedStockId = entity.FeedStockId,
        FeedStockCode = entity.FeedStock?.ErpStockCode, FeedStockName = entity.FeedStock?.StockName,
        ReductionRatePercent = entity.ReductionRatePercent, Description = entity.Description
    };

    private static BudgetFishGrowthQualityDto MapGrowthQuality(BudgetFishGrowthQuality entity) => new()
    {
        Id = entity.Id, FishStockId = entity.FishStockId, FishStockCode = entity.FishStock?.ErpStockCode,
        FishStockName = entity.FishStock?.StockName, GrowthMonthNo = entity.GrowthMonthNo,
        QualityPercent = entity.QualityPercent, Description = entity.Description
    };

    private static ApiResponse<bool> Invalid(string message) => ApiResponse<bool>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
    private static ApiResponse<bool> Conflict(string message) => ApiResponse<bool>.ErrorResult(message, message, StatusCodes.Status409Conflict);
    private static bool ValidDescription(string? value) => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= 500;
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
