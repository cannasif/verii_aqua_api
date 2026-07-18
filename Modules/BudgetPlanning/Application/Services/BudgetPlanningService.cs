using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using aqua_api.Modules.BudgetPlanning.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Domain.Enums;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.BudgetPlanning.Application.Services;

public class BudgetPlanningService : IBudgetPlanningService
{
    private readonly IUnitOfWork _unitOfWork;

    public BudgetPlanningService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResponse<BudgetPlanDto>>> GetPlansAsync(PagedRequest request)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();

        var query = _unitOfWork.Db.BudgetPlans
            .AsNoTracking()
            .Include(x => x.FishBatches)
            .Include(x => x.MonthlyProjections)
            .Where(x => !x.IsDeleted)
            .ApplyFilters(request.Filters, request.FilterLogic);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BudgetPlan.Id) : request.SortBy;
        query = query.ApplySorting(sortBy, request.SortDirection);

        var totalCount = await query.CountAsync();
        var plans = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

        return ApiResponse<PagedResponse<BudgetPlanDto>>.SuccessResult(new PagedResponse<BudgetPlanDto>
        {
            Items = plans.Select(MapPlan).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        }, "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetPlanDto>> GetPlanAsync(long id)
    {
        var plan = await PlanQuery().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        return ApiResponse<BudgetPlanDto>.SuccessResult(MapPlan(plan), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetPlanDto>> CreatePlanAsync(CreateBudgetPlanDto dto)
    {
        var validation = ValidatePlanPeriod(dto.StartYear, dto.StartMonth, dto.EndYear, dto.EndMonth);
        if (!validation.Success)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult(validation.Message, validation.Message, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(dto.BudgetName))
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult("Butce adi zorunludur.", "Butce adi zorunludur.", StatusCodes.Status400BadRequest);
        }

        var budgetNo = await GenerateBudgetNoAsync(dto.StartYear, dto.StartMonth);
        var budgetCode = string.IsNullOrWhiteSpace(dto.BudgetCode) ? budgetNo : dto.BudgetCode.Trim();

        var exists = await _unitOfWork.Db.BudgetPlans.AnyAsync(x =>
            !x.IsDeleted && (x.BudgetNo == budgetNo || x.BudgetCode == budgetCode));
        if (exists)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult("Bu butce kodu veya numarasi zaten var.", "Bu butce kodu veya numarasi zaten var.", StatusCodes.Status409Conflict);
        }

        var plan = new BudgetPlan
        {
            BudgetNo = budgetNo,
            BudgetCode = budgetCode,
            BudgetName = dto.BudgetName.Trim(),
            StartYear = dto.StartYear,
            StartMonth = dto.StartMonth,
            EndYear = dto.EndYear,
            EndMonth = dto.EndMonth,
            Description = NormalizeOptional(dto.Description)
        };

        await _unitOfWork.Repository<BudgetPlan>().AddAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        var saved = await PlanQuery().FirstAsync(x => x.Id == plan.Id);
        return ApiResponse<BudgetPlanDto>.SuccessResult(MapPlan(saved), "Butce olusturuldu.");
    }

    public async Task<ApiResponse<BudgetPlanDto>> CopyPlanAsync(long sourceBudgetPlanId, CopyBudgetPlanDto dto)
    {
        var source = await PlanQuery().FirstOrDefaultAsync(x => x.Id == sourceBudgetPlanId);
        if (source == null)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult("Kopyalanacak butce plani bulunamadi.", "Kopyalanacak butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var startYear = dto.StartYear ?? source.StartYear;
        var startMonth = dto.StartMonth ?? source.StartMonth;
        var endYear = dto.EndYear ?? source.EndYear;
        var endMonth = dto.EndMonth ?? source.EndMonth;
        var validation = ValidatePlanPeriod(startYear, startMonth, endYear, endMonth);
        if (!validation.Success)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult(validation.Message, validation.Message, StatusCodes.Status400BadRequest);
        }

        var budgetNo = await GenerateBudgetNoAsync(startYear, startMonth);
        var budgetCode = string.IsNullOrWhiteSpace(dto.BudgetCode) ? budgetNo : dto.BudgetCode.Trim();
        var exists = await _unitOfWork.Db.BudgetPlans.AnyAsync(x => !x.IsDeleted && (x.BudgetNo == budgetNo || x.BudgetCode == budgetCode));
        if (exists)
        {
            return ApiResponse<BudgetPlanDto>.ErrorResult("Yeni butce kodu veya numarasi zaten var.", "Yeni butce kodu veya numarasi zaten var.", StatusCodes.Status409Conflict);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var target = new BudgetPlan
            {
                BudgetNo = budgetNo,
                BudgetCode = budgetCode,
                BudgetName = string.IsNullOrWhiteSpace(dto.BudgetName) ? $"{source.BudgetName} Revizyon" : dto.BudgetName.Trim(),
                StartYear = startYear,
                StartMonth = startMonth,
                EndYear = endYear,
                EndMonth = endMonth,
                Status = dto.ResetToDraft
                    ? BudgetPlanStatus.Draft
                    : dto.IncludeCalculatedResults ? source.Status : TrimCopiedStatus(source.Status),
                Description = NormalizeOptional(dto.Description) ?? source.Description,
                CalculatedAt = dto.IncludeCalculatedResults && !dto.ResetToDraft ? source.CalculatedAt : null
            };

            await _unitOfWork.Repository<BudgetPlan>().AddAsync(target);
            await _unitOfWork.SaveChangesAsync();

            var projectIdMap = new Dictionary<long, long>();
            foreach (var project in source.Projects.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                var copy = new BudgetPlanProject
                {
                    BudgetPlanId = target.Id,
                    SourceType = project.SourceType,
                    SourceProjectId = project.SourceProjectId,
                    ProjectCode = project.ProjectCode,
                    ProjectName = project.ProjectName,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate
                };
                await _unitOfWork.Repository<BudgetPlanProject>().AddAsync(copy);
                await _unitOfWork.SaveChangesAsync();
                projectIdMap[project.Id] = copy.Id;
            }

            var batchIdMap = new Dictionary<long, long>();
            foreach (var batch in source.FishBatches.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                var copy = new BudgetPlanFishBatch
                {
                    BudgetPlanId = target.Id,
                    BudgetPlanProjectId = projectIdMap[batch.BudgetPlanProjectId],
                    SourceType = batch.SourceType,
                    SourceFishBatchId = batch.SourceFishBatchId,
                    FishStockId = batch.FishStockId,
                    BatchCode = batch.BatchCode,
                    InitialLiveCount = batch.InitialLiveCount,
                    InitialAverageGram = batch.InitialAverageGram,
                    InitialBiomassKg = batch.InitialBiomassKg,
                    InitialUnitCost = batch.InitialUnitCost,
                    InitialSmmAmount = batch.InitialSmmAmount,
                    GrowthStartYear = batch.GrowthStartYear,
                    GrowthStartMonth = batch.GrowthStartMonth,
                    Note = batch.Note
                };
                await _unitOfWork.Repository<BudgetPlanFishBatch>().AddAsync(copy);
                await _unitOfWork.SaveChangesAsync();
                batchIdMap[batch.Id] = copy.Id;
            }

            foreach (var adjustment in source.FishBatchAdjustments.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                await _unitOfWork.Repository<BudgetPlanFishBatchAdjustment>().AddAsync(new BudgetPlanFishBatchAdjustment
                {
                    BudgetPlanId = target.Id,
                    BudgetPlanFishBatchId = batchIdMap[adjustment.BudgetPlanFishBatchId],
                    AdjustmentType = adjustment.AdjustmentType,
                    LiveCount = adjustment.LiveCount,
                    AverageGram = adjustment.AverageGram,
                    BiomassKg = adjustment.BiomassKg,
                    Description = adjustment.Description
                });
            }

            foreach (var rate in source.ExchangeRates.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                await _unitOfWork.Repository<BudgetPlanExchangeRate>().AddAsync(new BudgetPlanExchangeRate
                {
                    BudgetPlanId = target.Id,
                    Year = rate.Year,
                    Month = rate.Month,
                    CurrencyCode = rate.CurrencyCode,
                    RateType = rate.RateType,
                    ExchangeRate = rate.ExchangeRate,
                    SourceType = rate.SourceType,
                    SourceReference = rate.SourceReference,
                    IsManualOverride = rate.IsManualOverride,
                    Description = rate.Description
                });
            }

            foreach (var price in source.FishPrices.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                await _unitOfWork.Repository<BudgetPlanFishPrice>().AddAsync(new BudgetPlanFishPrice
                {
                    BudgetPlanId = target.Id,
                    FishStockId = price.FishStockId,
                    CalibrationDefinitionId = price.CalibrationDefinitionId,
                    Year = price.Year,
                    Month = price.Month,
                    PriceType = price.PriceType,
                    MarketType = price.MarketType,
                    CurrencyCode = price.CurrencyCode,
                    UnitPrice = price.UnitPrice,
                    IncreaseRatePercent = price.IncreaseRatePercent,
                    IncreasePeriodMonths = price.IncreasePeriodMonths,
                    Description = price.Description
                });
            }

            foreach (var sale in source.SalesLines.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                await _unitOfWork.Repository<BudgetPlanSalesLine>().AddAsync(new BudgetPlanSalesLine
                {
                    BudgetPlanId = target.Id,
                    BudgetPlanFishBatchId = batchIdMap[sale.BudgetPlanFishBatchId],
                    Year = sale.Year,
                    Month = sale.Month,
                    SalesTon = sale.SalesTon,
                    SalesCount = sale.SalesCount,
                    UnitPrice = sale.UnitPrice,
                    Description = sale.Description
                });
            }

            if (dto.IncludeCalculatedResults && !dto.ResetToDraft)
            {
                var projectionIdMap = new Dictionary<long, long>();
                foreach (var projection in source.MonthlyProjections.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
                {
                    var copy = new BudgetPlanMonthlyProjection
                    {
                        BudgetPlanId = target.Id,
                        BudgetPlanFishBatchId = batchIdMap[projection.BudgetPlanFishBatchId],
                        Year = projection.Year,
                        Month = projection.Month,
                        MonthIndex = projection.MonthIndex,
                        OpeningLiveCount = projection.OpeningLiveCount,
                        OpeningAverageGram = projection.OpeningAverageGram,
                        OpeningBiomassKg = projection.OpeningBiomassKg,
                        MonthlyGrowthGram = projection.MonthlyGrowthGram,
                        ClosingAverageGram = projection.ClosingAverageGram,
                        SalesTon = projection.SalesTon,
                        SalesCount = projection.SalesCount,
                        MortalityKg = projection.MortalityKg,
                        MortalityCount = projection.MortalityCount,
                        FeedKg = projection.FeedKg,
                        ClosingLiveCount = projection.ClosingLiveCount,
                        ClosingBiomassKg = projection.ClosingBiomassKg,
                        CalibrationDefinitionId = projection.CalibrationDefinitionId,
                        WaterTemperatureId = projection.WaterTemperatureId
                    };
                    await _unitOfWork.Repository<BudgetPlanMonthlyProjection>().AddAsync(copy);
                    await _unitOfWork.SaveChangesAsync();
                    projectionIdMap[projection.Id] = copy.Id;
                }

                foreach (var line in source.FeedingLines.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
                {
                    await _unitOfWork.Repository<BudgetPlanFeedingLine>().AddAsync(new BudgetPlanFeedingLine
                    {
                        BudgetPlanId = target.Id,
                        BudgetPlanMonthlyProjectionId = projectionIdMap[line.BudgetPlanMonthlyProjectionId],
                        BudgetPlanFishBatchId = batchIdMap[line.BudgetPlanFishBatchId],
                        Year = line.Year,
                        Month = line.Month,
                        FeedStockId = line.FeedStockId,
                        FeedAmountRate = line.FeedAmountRate,
                        FeedKg = line.FeedKg
                    });
                }

                foreach (var line in source.MortalityLines.Where(x => !x.IsDeleted).OrderBy(x => x.Id))
                {
                    await _unitOfWork.Repository<BudgetPlanMortalityLine>().AddAsync(new BudgetPlanMortalityLine
                    {
                        BudgetPlanId = target.Id,
                        BudgetPlanMonthlyProjectionId = projectionIdMap[line.BudgetPlanMonthlyProjectionId],
                        BudgetPlanFishBatchId = batchIdMap[line.BudgetPlanFishBatchId],
                        Year = line.Year,
                        Month = line.Month,
                        MortalityRatePercent = line.MortalityRatePercent,
                        MortalityCount = line.MortalityCount,
                        MortalityKg = line.MortalityKg
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var saved = await PlanQuery().FirstAsync(x => x.Id == target.Id);
            return ApiResponse<BudgetPlanDto>.SuccessResult(MapPlan(saved), "Butce kopyalandi.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResponse<List<BudgetAvailableFishBatchDto>>> GetAvailableFishBatchesAsync()
    {
#pragma warning disable CS8602
        var cageBalances = await _unitOfWork.Db.BatchCageBalances
            .AsNoTracking()
            .Include(x => x.FishBatch)!.ThenInclude(x => x.Project)
            .Include(x => x.FishBatch)!.ThenInclude(x => x.FishStock)
            .Where(x => !x.IsDeleted && x.LiveCount > 0 && x.FishBatch != null && x.FishBatch.Project != null && x.FishBatch.Project.Status != DocumentStatus.Cancelled)
            .ToListAsync();

        var warehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
            .AsNoTracking()
            .Include(x => x.FishBatch)!.ThenInclude(x => x.Project)
            .Include(x => x.FishBatch)!.ThenInclude(x => x.FishStock)
            .Where(x => !x.IsDeleted && x.LiveCount > 0 && x.FishBatch != null && x.FishBatch.Project != null && x.FishBatch.Project.Status != DocumentStatus.Cancelled)
            .ToListAsync();
#pragma warning restore CS8602

        var rows = cageBalances
            .Select(x => new BalanceSeed(x.FishBatch!, x.LiveCount, x.AverageGram, x.BiomassGram, x.AsOfDate))
            .Concat(warehouseBalances.Select(x => new BalanceSeed(x.FishBatch!, x.LiveCount, x.AverageGram, x.BiomassGram, x.AsOfDate)))
            .GroupBy(x => x.FishBatch.Id)
            .Select(group =>
            {
                var first = group.First().FishBatch;
                var liveCount = group.Sum(x => x.LiveCount);
                var biomassKg = group.Sum(x => x.BiomassGram > 0m ? x.BiomassGram : x.LiveCount * x.AverageGram) / 1000m;
                var averageGram = liveCount <= 0 ? first.CurrentAverageGram : (biomassKg * 1000m) / liveCount;
                return new BudgetAvailableFishBatchDto
                {
                    FishBatchId = first.Id,
                    ProjectId = first.ProjectId,
                    ProjectCode = first.Project?.ProjectCode ?? string.Empty,
                    ProjectName = first.Project?.ProjectName ?? string.Empty,
                    BatchCode = first.BatchCode,
                    FishStockId = first.FishStockId,
                    FishStockCode = first.FishStock?.ErpStockCode,
                    FishStockName = first.FishStock?.StockName,
                    LiveCount = liveCount,
                    AverageGram = Round(averageGram),
                    BiomassKg = Round(biomassKg),
                    AsOfDate = group.Max(x => x.AsOfDate)
                };
            })
            .Where(x => x.LiveCount > 0 && x.BiomassKg > 0)
            .OrderBy(x => x.ProjectCode)
            .ThenBy(x => x.BatchCode)
            .ToList();

        return ApiResponse<List<BudgetAvailableFishBatchDto>>.SuccessResult(rows, "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanFishBatchDto>>> GetPlanFishBatchesAsync(long budgetPlanId)
    {
        return ApiResponse<List<BudgetPlanFishBatchDto>>.SuccessResult(await LoadPlanFishBatchesAsync(budgetPlanId), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanFishBatchDto>>> AddActualFishBatchesAsync(long budgetPlanId, AddActualFishBatchesToBudgetDto dto)
    {
        var plan = await PlanQuery().FirstOrDefaultAsync(x => x.Id == budgetPlanId);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanFishBatchDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status >= BudgetPlanStatus.GrowthCalculated)
        {
            return ApiResponse<List<BudgetPlanFishBatchDto>>.ErrorResult("Buyutme basladiktan sonra canli getirilemez.", "Buyutme basladiktan sonra canli getirilemez.", StatusCodes.Status400BadRequest);
        }

        if (dto.FishBatchIds.Count == 0)
        {
            return ApiResponse<List<BudgetPlanFishBatchDto>>.ErrorResult("En az bir balik partisi secilmelidir.", "En az bir balik partisi secilmelidir.", StatusCodes.Status400BadRequest);
        }

        var available = (await GetAvailableFishBatchesAsync()).Data?
            .Where(x => dto.FishBatchIds.Contains(x.FishBatchId))
            .ToList() ?? new List<BudgetAvailableFishBatchDto>();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var source in available)
            {
                var project = await EnsurePlanProjectAsync(plan, BudgetPlanSourceType.Actual, source.ProjectId, source.ProjectCode, source.ProjectName);
                var exists = await _unitOfWork.Db.BudgetPlanFishBatches.AnyAsync(x =>
                    x.BudgetPlanId == plan.Id && x.SourceFishBatchId == source.FishBatchId && !x.IsDeleted);
                if (exists)
                {
                    continue;
                }

                await _unitOfWork.Repository<BudgetPlanFishBatch>().AddAsync(new BudgetPlanFishBatch
                {
                    BudgetPlanId = plan.Id,
                    BudgetPlanProjectId = project.Id,
                    SourceType = BudgetPlanSourceType.Actual,
                    SourceFishBatchId = source.FishBatchId,
                    FishStockId = source.FishStockId,
                    BatchCode = source.BatchCode,
                    InitialLiveCount = source.LiveCount,
                    InitialAverageGram = source.AverageGram,
                    InitialBiomassKg = source.BiomassKg,
                    GrowthStartYear = dto.GrowthStartYear ?? plan.StartYear,
                    GrowthStartMonth = dto.GrowthStartMonth ?? plan.StartMonth
                });
            }

            await _unitOfWork.SaveChangesAsync();
            plan.Status = BudgetPlanStatus.LiveImported;
            await _unitOfWork.Repository<BudgetPlan>().UpdateAsync(plan);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return ApiResponse<List<BudgetPlanFishBatchDto>>.SuccessResult(await LoadPlanFishBatchesAsync(plan.Id), "Baliklar butceye alindi.");
    }

    public async Task<ApiResponse<BudgetPlanFishBatchDto>> AddVirtualFishBatchAsync(long budgetPlanId, AddVirtualFishBatchDto dto)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanFishBatchDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status >= BudgetPlanStatus.GrowthCalculated)
        {
            return ApiResponse<BudgetPlanFishBatchDto>.ErrorResult("Buyutme basladiktan sonra sanal proje veya sanal balik eklenemez.", "Buyutme basladiktan sonra sanal proje veya sanal balik eklenemez.", StatusCodes.Status400BadRequest);
        }

        if (dto.FishStockId <= 0 || dto.InitialLiveCount < 0 || dto.InitialAverageGram < 0)
        {
            return ApiResponse<BudgetPlanFishBatchDto>.ErrorResult("Sanal balik bilgileri eksik veya hatali.", "Sanal balik bilgileri eksik veya hatali.", StatusCodes.Status400BadRequest);
        }

        var fishStockExists = await _unitOfWork.Db.Stocks.AnyAsync(x => x.Id == dto.FishStockId && !x.IsDeleted);
        if (!fishStockExists)
        {
            return ApiResponse<BudgetPlanFishBatchDto>.ErrorResult("Secilen balik stogu bulunamadi.", "Secilen balik stogu bulunamadi.", StatusCodes.Status404NotFound);
        }

        var project = await EnsurePlanProjectAsync(plan, BudgetPlanSourceType.Virtual, null, dto.ProjectCode, dto.ProjectName);
        var entity = new BudgetPlanFishBatch
        {
            BudgetPlanId = plan.Id,
            BudgetPlanProjectId = project.Id,
            SourceType = BudgetPlanSourceType.Virtual,
            FishStockId = dto.FishStockId,
            BatchCode = dto.BatchCode.Trim(),
            InitialLiveCount = dto.InitialLiveCount,
            InitialAverageGram = dto.InitialAverageGram,
            InitialBiomassKg = Round(dto.InitialLiveCount * dto.InitialAverageGram / 1000m),
            InitialUnitCost = dto.InitialUnitCost,
            InitialSmmAmount = dto.InitialSmmAmount,
            GrowthStartYear = dto.GrowthStartYear,
            GrowthStartMonth = dto.GrowthStartMonth,
            Note = NormalizeOptional(dto.Note)
        };

        await _unitOfWork.Repository<BudgetPlanFishBatch>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var saved = await FishBatchQuery().FirstAsync(x => x.Id == entity.Id);
        return ApiResponse<BudgetPlanFishBatchDto>.SuccessResult(MapFishBatch(saved), "Sanal balik butceye eklendi.");
    }

    public async Task<ApiResponse<List<BudgetPlanFishBatchAdjustmentDto>>> GetFishBatchAdjustmentsAsync(long budgetPlanId)
    {
        var rows = await FishBatchAdjustmentQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return ApiResponse<List<BudgetPlanFishBatchAdjustmentDto>>.SuccessResult(rows.Select(MapFishBatchAdjustment).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetPlanFishBatchAdjustmentDto>> CreateFishBatchAdjustmentAsync(long budgetPlanId, CreateBudgetPlanFishBatchAdjustmentDto dto)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status >= BudgetPlanStatus.GrowthCalculated)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Buyutme basladiktan sonra canli veya sanal miktar degisikligi yapilamaz.", "Buyutme basladiktan sonra canli veya sanal miktar degisikligi yapilamaz.", StatusCodes.Status400BadRequest);
        }

        if (dto.LiveCount <= 0)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Miktar sifirdan buyuk olmalidir.", "Miktar sifirdan buyuk olmalidir.", StatusCodes.Status400BadRequest);
        }

        var batch = await _unitOfWork.Db.BudgetPlanFishBatches
            .Include(x => x.BudgetPlanProject)
            .Include(x => x.FishStock)
            .FirstOrDefaultAsync(x => x.Id == dto.BudgetPlanFishBatchId && x.BudgetPlanId == budgetPlanId && !x.IsDeleted);
        if (batch == null)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Butce balik satiri bulunamadi.", "Butce balik satiri bulunamadi.", StatusCodes.Status404NotFound);
        }

        var averageGram = dto.AverageGram ?? batch.InitialAverageGram;
        if (averageGram < 0)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Ortalama gram negatif olamaz.", "Ortalama gram negatif olamaz.", StatusCodes.Status400BadRequest);
        }

        var sign = dto.AdjustmentType == BudgetPlanFishBatchAdjustmentType.Increase ? 1 : -1;
        var newLiveCount = batch.InitialLiveCount + sign * dto.LiveCount;
        if (newLiveCount < 0)
        {
            return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.ErrorResult("Cikis miktari mevcut adetten fazla olamaz.", "Cikis miktari mevcut adetten fazla olamaz.", StatusCodes.Status400BadRequest);
        }

        var adjustmentBiomassKg = Round(dto.LiveCount * averageGram / 1000m);
        var newBiomassKg = Math.Max(0m, Round(batch.InitialBiomassKg + sign * adjustmentBiomassKg));
        batch.InitialLiveCount = newLiveCount;
        batch.InitialBiomassKg = newBiomassKg;
        batch.InitialAverageGram = newLiveCount <= 0 ? 0m : Round(newBiomassKg * 1000m / newLiveCount);

        var entity = new BudgetPlanFishBatchAdjustment
        {
            BudgetPlanId = budgetPlanId,
            BudgetPlanFishBatchId = batch.Id,
            AdjustmentType = dto.AdjustmentType,
            LiveCount = dto.LiveCount,
            AverageGram = averageGram,
            BiomassKg = adjustmentBiomassKg,
            Description = NormalizeOptional(dto.Description)
        };

        await _unitOfWork.Repository<BudgetPlanFishBatchAdjustment>().AddAsync(entity);
        await _unitOfWork.Repository<BudgetPlanFishBatch>().UpdateAsync(batch);
        plan.Status = BudgetPlanStatus.Adjusted;
        await _unitOfWork.Repository<BudgetPlan>().UpdateAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        var saved = await FishBatchAdjustmentQuery().FirstAsync(x => x.Id == entity.Id);
        return ApiResponse<BudgetPlanFishBatchAdjustmentDto>.SuccessResult(MapFishBatchAdjustment(saved), "Miktar hareketi kaydedildi.");
    }

    public async Task<ApiResponse<BudgetPlanSalesLineDto>> UpsertSalesLineAsync(long budgetPlanId, UpsertBudgetPlanSalesLineDto dto)
    {
        if (!IsValidMonth(dto.Month) || dto.SalesTon < 0)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Satis donemi veya miktari hatali.", "Satis donemi veya miktari hatali.", StatusCodes.Status400BadRequest);
        }

        var plan = await _unitOfWork.Db.BudgetPlans.FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status < BudgetPlanStatus.GrowthCalculated)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Satis plani icin once baliklari sanalda buyutmelisiniz.", "Satis plani icin once baliklari sanalda buyutmelisiniz.", StatusCodes.Status400BadRequest);
        }

        var fishBatchExists = await _unitOfWork.Db.BudgetPlanFishBatches.AnyAsync(x =>
            x.Id == dto.BudgetPlanFishBatchId && x.BudgetPlanId == budgetPlanId && !x.IsDeleted);
        if (!fishBatchExists)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Butce balik satiri bulunamadi.", "Butce balik satiri bulunamadi.", StatusCodes.Status404NotFound);
        }

        var entity = await _unitOfWork.Db.BudgetPlanSalesLines.FirstOrDefaultAsync(x =>
            x.BudgetPlanId == budgetPlanId &&
            x.BudgetPlanFishBatchId == dto.BudgetPlanFishBatchId &&
            x.Year == dto.Year &&
            x.Month == dto.Month &&
            !x.IsDeleted);

        if (entity == null)
        {
            entity = new BudgetPlanSalesLine { BudgetPlanId = budgetPlanId };
            await _unitOfWork.Repository<BudgetPlanSalesLine>().AddAsync(entity);
        }

        entity.BudgetPlanFishBatchId = dto.BudgetPlanFishBatchId;
        entity.Year = dto.Year;
        entity.Month = dto.Month;
        entity.SalesTon = Round(dto.SalesTon);
        entity.SalesCount = dto.SalesCount;
        entity.UnitPrice = dto.UnitPrice ?? await FindFishPriceEuroAsync(budgetPlanId, dto.BudgetPlanFishBatchId, dto.Year, dto.Month);
        entity.Description = NormalizeOptional(dto.Description);
        plan.Status = BudgetPlanStatus.SalesPlanned;
        plan.CalculatedAt = null;

        await _unitOfWork.SaveChangesAsync();
        var saved = await SalesLineQuery()
            .FirstAsync(x => x.Id == entity.Id);
        var exchangeRate = await FindExchangeRateAsync(budgetPlanId, dto.Year, dto.Month, "EUR");
        return ApiResponse<BudgetPlanSalesLineDto>.SuccessResult(MapSalesLine(saved, exchangeRate), "Satis plani kaydedildi.");
    }

    public async Task<ApiResponse<BudgetPlanSalesLineDto>> UpsertSalesTonAsync(long budgetPlanId, UpsertBudgetPlanSalesTonDto dto)
    {
        if (dto.SalesTon < 0)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Satis tonu negatif olamaz.", "Satis tonu negatif olamaz.", StatusCodes.Status400BadRequest);
        }

        var projection = await ProjectionQuery()
            .FirstOrDefaultAsync(x =>
                x.BudgetPlanId == budgetPlanId &&
                x.BudgetPlanFishBatchId == dto.BudgetPlanFishBatchId &&
                x.Year == dto.Year &&
                x.Month == dto.Month);

        if (projection == null)
        {
            return ApiResponse<BudgetPlanSalesLineDto>.ErrorResult("Bu ay ve parti icin buyutme sonucu bulunamadi.", "Bu ay ve parti icin buyutme sonucu bulunamadi.", StatusCodes.Status404NotFound);
        }

        var salesKg = Round(dto.SalesTon * 1000m);
        var averageKg = projection.ClosingAverageGram / 1000m;
        var salesCount = averageKg <= 0 ? 0 : (int)Math.Round(salesKg / averageKg, MidpointRounding.AwayFromZero);

        return await UpsertSalesLineAsync(budgetPlanId, new UpsertBudgetPlanSalesLineDto
        {
            BudgetPlanFishBatchId = dto.BudgetPlanFishBatchId,
            Year = dto.Year,
            Month = dto.Month,
            SalesTon = dto.SalesTon,
            SalesCount = salesCount,
            UnitPrice = dto.UnitPrice,
            Description = NormalizeOptional(dto.Description)
        });
    }

    public async Task<ApiResponse<List<BudgetPlanSalesLineDto>>> ImportSalesTonsAsync(long budgetPlanId, ImportBudgetPlanSalesTonsDto dto)
    {
        if (dto.Lines.Count == 0)
        {
            return ApiResponse<List<BudgetPlanSalesLineDto>>.ErrorResult(
                "Excel dosyasında aktarılacak satış satırı bulunamadı.",
                "Excel dosyasında aktarılacak satış satırı bulunamadı.",
                StatusCodes.Status400BadRequest);
        }

        var duplicatePeriod = dto.Lines
            .GroupBy(x => new { x.BudgetPlanFishBatchId, x.Year, x.Month })
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicatePeriod != null)
        {
            return ApiResponse<List<BudgetPlanSalesLineDto>>.ErrorResult(
                $"Aynı parti ve dönem Excel içerisinde birden fazla kez yer alıyor: {duplicatePeriod.Key.Year}/{duplicatePeriod.Key.Month:00}.",
                $"Aynı parti ve dönem Excel içerisinde birden fazla kez yer alıyor: {duplicatePeriod.Key.Year}/{duplicatePeriod.Key.Month:00}.",
                StatusCodes.Status400BadRequest);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var savedRows = new List<BudgetPlanSalesLineDto>();
            for (var index = 0; index < dto.Lines.Count; index++)
            {
                var result = await UpsertSalesTonAsync(budgetPlanId, dto.Lines[index]);
                if (!result.Success || result.Data == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<List<BudgetPlanSalesLineDto>>.ErrorResult(
                        $"Excel {index + 2}. satır işlenemedi: {result.Message}",
                        result.ExceptionMessage,
                        result.StatusCode);
                }

                savedRows.Add(result.Data);
            }

            await _unitOfWork.CommitTransactionAsync();
            return ApiResponse<List<BudgetPlanSalesLineDto>>.SuccessResult(savedRows, $"{savedRows.Count} satış satırı Excel'den aktarıldı.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResponse<List<BudgetPlanSalesLineDto>>> GetSalesLinesAsync(long budgetPlanId)
    {
        var rows = await SalesLineQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode)
            .ThenBy(x => x.BudgetPlanFishBatch.BatchCode)
            .ToListAsync();

        var exchangeRates = await LoadExchangeRateLookupAsync(budgetPlanId, "EUR");
        return ApiResponse<List<BudgetPlanSalesLineDto>>.SuccessResult(rows.Select(row =>
            MapSalesLine(row, exchangeRates.GetValueOrDefault(new BudgetPeriod(row.Year, row.Month)))).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetSalesPlanningRowDto>>> GetSalesPlanningRowsAsync(long budgetPlanId)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<List<BudgetSalesPlanningRowDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status < BudgetPlanStatus.GrowthCalculated)
        {
            return ApiResponse<List<BudgetSalesPlanningRowDto>>.ErrorResult("Satis planlama icin once baliklar buyutulmelidir.", "Satis planlama icin once baliklar buyutulmelidir.", StatusCodes.Status400BadRequest);
        }

        var sales = await _unitOfWork.Db.BudgetPlanSalesLines
            .AsNoTracking()
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        var rows = await ProjectionQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode)
            .ThenBy(x => x.BudgetPlanFishBatch.BatchCode)
            .ToListAsync();

        return ApiResponse<List<BudgetSalesPlanningRowDto>>.SuccessResult(rows.Select(row =>
        {
            var plannedSalesKg = sales
                .Where(x => x.BudgetPlanFishBatchId == row.BudgetPlanFishBatchId && x.Year == row.Year && x.Month == row.Month)
                .Sum(x => x.SalesTon * 1000m);
            var averageKg = row.ClosingAverageGram / 1000m;
            var plannedSalesCount = averageKg <= 0 ? 0 : (int)Math.Round(plannedSalesKg / averageKg, MidpointRounding.AwayFromZero);
            var remainingKg = Math.Max(0m, row.ClosingBiomassKg - plannedSalesKg);
            var remainingCount = Math.Max(0, row.ClosingLiveCount - plannedSalesCount);

            return new BudgetSalesPlanningRowDto
            {
                BudgetPlanFishBatchId = row.BudgetPlanFishBatchId,
                ProjectCode = row.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
                ProjectName = row.BudgetPlanFishBatch.BudgetPlanProject.ProjectName,
                BatchCode = row.BudgetPlanFishBatch.BatchCode,
                FishStockCode = row.BudgetPlanFishBatch.FishStock.ErpStockCode,
                FishStockName = row.BudgetPlanFishBatch.FishStock.StockName,
                Year = row.Year,
                Month = row.Month,
                AverageGram = Round(row.ClosingAverageGram),
                AverageKg = Round(averageKg),
                AvailableCount = row.ClosingLiveCount,
                AvailableKg = Round(row.ClosingBiomassKg),
                AvailableTon = Round(row.ClosingBiomassKg / 1000m),
                PlannedSalesTon = Round(plannedSalesKg / 1000m),
                PlannedSalesCount = plannedSalesCount,
                RemainingKg = Round(remainingKg),
                RemainingTon = Round(remainingKg / 1000m),
                RemainingCount = remainingCount
            };
        }).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanExchangeRateDto>>> GetExchangeRatesAsync(long budgetPlanId)
    {
        var planExists = await _unitOfWork.Db.BudgetPlans
            .AsNoTracking()
            .AnyAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (!planExists)
        {
            return ApiResponse<List<BudgetPlanExchangeRateDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var rows = await _unitOfWork.Db.BudgetPlanExchangeRates
            .AsNoTracking()
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.CurrencyCode)
            .ThenBy(x => x.RateType)
            .ToListAsync();

        return ApiResponse<List<BudgetPlanExchangeRateDto>>.SuccessResult(rows.Select(MapExchangeRate).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanExchangeRateDto>>> GenerateExchangeRatesAsync(long budgetPlanId, GenerateBudgetPlanExchangeRatesDto dto)
    {
        var plan = await _unitOfWork.Db.BudgetPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanExchangeRateDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var currencyCodes = dto.CurrencyCodes
            .Select(NormalizeCurrencyCode)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (currencyCodes.Count == 0)
        {
            return ApiResponse<List<BudgetPlanExchangeRateDto>>.ErrorResult("En az bir para birimi secilmelidir.", "En az bir para birimi secilmelidir.", StatusCodes.Status400BadRequest);
        }

        if (dto.DefaultExchangeRate < 0)
        {
            return ApiResponse<List<BudgetPlanExchangeRateDto>>.ErrorResult("Kur negatif olamaz.", "Kur negatif olamaz.", StatusCodes.Status400BadRequest);
        }

        var rateType = NormalizeRequired(dto.RateType, "Budget");
        var sourceType = NormalizeRequired(dto.SourceType, "Manual");
        var periods = BuildPeriods(plan.StartYear, plan.StartMonth, plan.EndYear, plan.EndMonth);
        var existingRows = await _unitOfWork.Db.BudgetPlanExchangeRates
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        foreach (var period in periods)
        {
            foreach (var currencyCode in currencyCodes)
            {
                var entity = existingRows.FirstOrDefault(x =>
                    x.Year == period.Year &&
                    x.Month == period.Month &&
                    x.CurrencyCode == currencyCode &&
                    x.RateType == rateType);

                if (entity == null)
                {
                    entity = new BudgetPlanExchangeRate
                    {
                        BudgetPlanId = budgetPlanId,
                        Year = period.Year,
                        Month = period.Month,
                        CurrencyCode = currencyCode,
                        RateType = rateType
                    };
                    existingRows.Add(entity);
                    await _unitOfWork.Repository<BudgetPlanExchangeRate>().AddAsync(entity);
                }

                if (!entity.IsManualOverride)
                {
                    entity.ExchangeRate = dto.DefaultExchangeRate;
                    entity.SourceType = sourceType;
                    entity.SourceReference = NormalizeOptional(dto.SourceReference);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetExchangeRatesAsync(budgetPlanId);
    }

    public async Task<ApiResponse<BudgetPlanExchangeRateDto>> UpsertExchangeRateAsync(long budgetPlanId, UpsertBudgetPlanExchangeRateDto dto)
    {
        if (!IsValidMonth(dto.Month) || dto.Year < 2000 || dto.Year > 2100)
        {
            return ApiResponse<BudgetPlanExchangeRateDto>.ErrorResult("Kur donemi hatali.", "Kur donemi hatali.", StatusCodes.Status400BadRequest);
        }

        if (dto.ExchangeRate < 0)
        {
            return ApiResponse<BudgetPlanExchangeRateDto>.ErrorResult("Kur negatif olamaz.", "Kur negatif olamaz.", StatusCodes.Status400BadRequest);
        }

        var plan = await _unitOfWork.Db.BudgetPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanExchangeRateDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var periodKey = dto.Year * 12 + dto.Month;
        if (periodKey < plan.StartYear * 12 + plan.StartMonth || periodKey > plan.EndYear * 12 + plan.EndMonth)
        {
            return ApiResponse<BudgetPlanExchangeRateDto>.ErrorResult("Kur donemi butce tarih araligi disinda olamaz.", "Kur donemi butce tarih araligi disinda olamaz.", StatusCodes.Status400BadRequest);
        }

        var currencyCode = NormalizeCurrencyCode(dto.CurrencyCode);
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return ApiResponse<BudgetPlanExchangeRateDto>.ErrorResult("Para birimi zorunludur.", "Para birimi zorunludur.", StatusCodes.Status400BadRequest);
        }

        var rateType = NormalizeRequired(dto.RateType, "Budget");
        var entity = await _unitOfWork.Db.BudgetPlanExchangeRates.FirstOrDefaultAsync(x =>
            x.BudgetPlanId == budgetPlanId &&
            x.Year == dto.Year &&
            x.Month == dto.Month &&
            x.CurrencyCode == currencyCode &&
            x.RateType == rateType &&
            !x.IsDeleted);

        if (entity == null)
        {
            entity = new BudgetPlanExchangeRate { BudgetPlanId = budgetPlanId };
            await _unitOfWork.Repository<BudgetPlanExchangeRate>().AddAsync(entity);
        }

        entity.Year = dto.Year;
        entity.Month = dto.Month;
        entity.CurrencyCode = currencyCode;
        entity.RateType = rateType;
        entity.ExchangeRate = dto.ExchangeRate;
        entity.SourceType = NormalizeRequired(dto.SourceType, "Manual");
        entity.SourceReference = NormalizeOptional(dto.SourceReference);
        entity.IsManualOverride = dto.IsManualOverride;
        entity.Description = NormalizeOptional(dto.Description);

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<BudgetPlanExchangeRateDto>.SuccessResult(MapExchangeRate(entity), "Kur kaydedildi.");
    }

    public async Task<ApiResponse<List<BudgetPlanFishPriceDto>>> GetFishPricesAsync(long budgetPlanId)
    {
        var planExists = await _unitOfWork.Db.BudgetPlans.AnyAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (!planExists)
        {
            return ApiResponse<List<BudgetPlanFishPriceDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var rows = await FishPriceQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.PriceType)
            .ThenBy(x => x.MarketType)
            .ThenBy(x => x.CurrencyCode)
            .ThenBy(x => x.CalibrationDefinition.CalibrationCode)
            .ToListAsync();

        var exchangeRateLookup = await LoadFishPriceExchangeRateLookupAsync(budgetPlanId, rows);
        var result = rows.Select(x => MapFishPrice(
            x,
            ResolveFishPriceExchangeRate(exchangeRateLookup, x.Year, x.Month, x.CurrencyCode))).ToList();

        return ApiResponse<List<BudgetPlanFishPriceDto>>.SuccessResult(result, "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanFishPriceDto>>> GenerateFishPricesAsync(long budgetPlanId, GenerateBudgetPlanFishPricesDto dto)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanFishPriceDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var defaultUnitPrice = ResolveUnitPrice(dto.DefaultUnitPrice, dto.DefaultUnitPriceEuro);
        var currencyCode = NormalizeCurrencyCode(dto.CurrencyCode);
        if (defaultUnitPrice < 0 || dto.IncreaseRatePercent < 0 || dto.IncreasePeriodMonths < 1)
        {
            return ApiResponse<List<BudgetPlanFishPriceDto>>.ErrorResult("Fiyat, artis orani veya periyot hatali.", "Fiyat, artis orani veya periyot hatali.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(currencyCode) ||
            !Enum.IsDefined(dto.PriceType) ||
            !Enum.IsDefined(dto.MarketType))
        {
            return ApiResponse<List<BudgetPlanFishPriceDto>>.ErrorResult("Fiyat tipi, pazar veya doviz bilgisi hatali.", "Fiyat tipi, pazar veya doviz bilgisi hatali.", StatusCodes.Status400BadRequest);
        }

        var calibrationIds = dto.CalibrationDefinitionIds.Where(x => x > 0).Distinct().ToList();
        if (calibrationIds.Count == 0)
        {
            calibrationIds = await _unitOfWork.Db.BudgetCalibrationDefinitions
                .Where(x => !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();
        }

        if (calibrationIds.Count == 0)
        {
            return ApiResponse<List<BudgetPlanFishPriceDto>>.ErrorResult("Kalibre tanimi bulunamadi.", "Kalibre tanimi bulunamadi.", StatusCodes.Status400BadRequest);
        }

        var fishStockIds = dto.FishStockIds.Where(x => x > 0).Distinct().ToList();
        var fishStockKeys = fishStockIds.Count == 0 ? new List<long?> { null } : fishStockIds.Select(x => (long?)x).ToList();
        var periods = BuildPeriods(plan.StartYear, plan.StartMonth, plan.EndYear, plan.EndMonth);
        var existingRows = await _unitOfWork.Db.BudgetPlanFishPrices
            .Where(x => x.BudgetPlanId == budgetPlanId && calibrationIds.Contains(x.CalibrationDefinitionId) && !x.IsDeleted)
            .ToListAsync();

        foreach (var fishStockId in fishStockKeys)
        {
            foreach (var calibrationId in calibrationIds)
            {
                for (var periodIndex = 0; periodIndex < periods.Count; periodIndex++)
                {
                    var period = periods[periodIndex];
                    var entity = existingRows.FirstOrDefault(x =>
                        x.FishStockId == fishStockId &&
                        x.CalibrationDefinitionId == calibrationId &&
                        x.Year == period.Year &&
                        x.Month == period.Month &&
                        x.PriceType == dto.PriceType &&
                        x.MarketType == dto.MarketType &&
                        x.CurrencyCode == currencyCode);
                    var unitPrice = CalculateEscalatedPrice(
                        defaultUnitPrice,
                        dto.IncreaseRatePercent,
                        dto.IncreasePeriodMonths,
                        periodIndex);
                    if (entity == null)
                    {
                        entity = new BudgetPlanFishPrice
                        {
                            BudgetPlanId = budgetPlanId,
                            FishStockId = fishStockId,
                            CalibrationDefinitionId = calibrationId,
                            Year = period.Year,
                            Month = period.Month,
                            PriceType = dto.PriceType,
                            MarketType = dto.MarketType,
                            CurrencyCode = currencyCode,
                            UnitPrice = unitPrice,
                            IncreaseRatePercent = dto.IncreaseRatePercent,
                            IncreasePeriodMonths = dto.IncreasePeriodMonths
                        };
                        await _unitOfWork.Repository<BudgetPlanFishPrice>().AddAsync(entity);
                    }
                    else
                    {
                        entity.UnitPrice = unitPrice;
                        entity.IncreaseRatePercent = dto.IncreaseRatePercent;
                        entity.IncreasePeriodMonths = dto.IncreasePeriodMonths;
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetFishPricesAsync(budgetPlanId);
    }

    public async Task<ApiResponse<BudgetPlanFishPriceDto>> UpsertFishPriceAsync(long budgetPlanId, UpsertBudgetPlanFishPriceDto dto)
    {
        var unitPrice = ResolveUnitPrice(dto.UnitPrice, dto.UnitPriceEuro);
        var currencyCode = NormalizeCurrencyCode(dto.CurrencyCode);
        if (!IsValidMonth(dto.Month) ||
            unitPrice < 0 ||
            dto.IncreaseRatePercent < 0 ||
            dto.IncreasePeriodMonths < 1 ||
            string.IsNullOrWhiteSpace(currencyCode) ||
            !Enum.IsDefined(dto.PriceType) ||
            !Enum.IsDefined(dto.MarketType))
        {
            return ApiResponse<BudgetPlanFishPriceDto>.ErrorResult("Fiyat donemi veya tutari hatali.", "Fiyat donemi veya tutari hatali.", StatusCodes.Status400BadRequest);
        }

        var plan = await _unitOfWork.Db.BudgetPlans.FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<BudgetPlanFishPriceDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (!IsPeriodWithinPlan(plan, dto.Year, dto.Month))
        {
            return ApiResponse<BudgetPlanFishPriceDto>.ErrorResult("Fiyat donemi butce tarih araligi disinda olamaz.", "Fiyat donemi butce tarih araligi disinda olamaz.", StatusCodes.Status400BadRequest);
        }

        var calibrationExists = await _unitOfWork.Db.BudgetCalibrationDefinitions.AnyAsync(x => x.Id == dto.CalibrationDefinitionId && !x.IsDeleted);
        if (!calibrationExists)
        {
            return ApiResponse<BudgetPlanFishPriceDto>.ErrorResult("Kalibre tanimi bulunamadi.", "Kalibre tanimi bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (dto.FishStockId.HasValue)
        {
            var fishStockExists = await _unitOfWork.Db.Stocks.AnyAsync(x => x.Id == dto.FishStockId.Value && !x.IsDeleted);
            if (!fishStockExists)
            {
                return ApiResponse<BudgetPlanFishPriceDto>.ErrorResult("Balik stogu bulunamadi.", "Balik stogu bulunamadi.", StatusCodes.Status404NotFound);
            }
        }

        var entity = await _unitOfWork.Db.BudgetPlanFishPrices.FirstOrDefaultAsync(x =>
            x.BudgetPlanId == budgetPlanId &&
            x.FishStockId == dto.FishStockId &&
            x.CalibrationDefinitionId == dto.CalibrationDefinitionId &&
            x.Year == dto.Year &&
            x.Month == dto.Month &&
            x.PriceType == dto.PriceType &&
            x.MarketType == dto.MarketType &&
            x.CurrencyCode == currencyCode &&
            !x.IsDeleted);

        if (entity == null)
        {
            entity = new BudgetPlanFishPrice { BudgetPlanId = budgetPlanId };
            await _unitOfWork.Repository<BudgetPlanFishPrice>().AddAsync(entity);
        }

        entity.FishStockId = dto.FishStockId;
        entity.CalibrationDefinitionId = dto.CalibrationDefinitionId;
        entity.Year = dto.Year;
        entity.Month = dto.Month;
        entity.PriceType = dto.PriceType;
        entity.MarketType = dto.MarketType;
        entity.CurrencyCode = currencyCode;
        entity.UnitPrice = unitPrice;
        entity.IncreaseRatePercent = dto.IncreaseRatePercent;
        entity.IncreasePeriodMonths = dto.IncreasePeriodMonths;
        entity.Description = NormalizeOptional(dto.Description);

        await _unitOfWork.SaveChangesAsync();
        var saved = await FishPriceQuery().FirstAsync(x => x.Id == entity.Id);
        var exchangeRate = await FindExchangeRateAsync(budgetPlanId, dto.Year, dto.Month, currencyCode);
        return ApiResponse<BudgetPlanFishPriceDto>.SuccessResult(MapFishPrice(saved, exchangeRate), "Balik fiyati kaydedildi.");
    }

    public Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> CalculateGrowthAsync(long budgetPlanId)
    {
        return CalculateProjectionAsync(budgetPlanId, includeSalesAndOperations: false);
    }

    public Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> CalculateAsync(long budgetPlanId)
    {
        return CalculateProjectionAsync(budgetPlanId, includeSalesAndOperations: true);
    }

    private async Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> CalculateProjectionAsync(long budgetPlanId, bool includeSalesAndOperations)
    {
        var plan = await PlanQuery().FirstOrDefaultAsync(x => x.Id == budgetPlanId);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        var batches = await FishBatchQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .ToListAsync();
        if (batches.Count == 0)
        {
            return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.ErrorResult("Butceye en az bir balik satiri eklenmelidir.", "Butceye en az bir balik satiri eklenmelidir.", StatusCodes.Status400BadRequest);
        }

        var periods = BuildPeriods(plan.StartYear, plan.StartMonth, plan.EndYear, plan.EndMonth);
        var sales = await _unitOfWork.Db.BudgetPlanSalesLines
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        if (includeSalesAndOperations && plan.Status < BudgetPlanStatus.SalesPlanned)
        {
            return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.ErrorResult("Yemleme ve fire hesabi icin once buyutme yapilmali ve satis plani girilmelidir.", "Yemleme ve fire hesabi icin once buyutme yapilmali ve satis plani girilmelidir.", StatusCodes.Status400BadRequest);
        }

        if (includeSalesAndOperations && sales.Count == 0)
        {
            return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.ErrorResult("Yemleme ve fire hesabi icin once satis plani girilmelidir.", "Yemleme ve fire hesabi icin once satis plani girilmelidir.", StatusCodes.Status400BadRequest);
        }

        var growthProfiles = await _unitOfWork.Db.BudgetFishGrowthProfiles
            .Include(x => x.Stock)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var waterTemperatures = await _unitOfWork.Db.BudgetWaterTemperatures
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var calibrations = await _unitOfWork.Db.BudgetCalibrationDefinitions
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var feedRates = await _unitOfWork.Db.BudgetFeedConsumptionRates
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var feedMortalityRates = await _unitOfWork.Db.BudgetFeedMortalityRates
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var growthQualities = await _unitOfWork.Db.BudgetFishGrowthQualities
            .Where(x => !x.IsDeleted)
            .ToListAsync();
        var mortalityRates = await _unitOfWork.Db.BudgetMortalityRateDefinitions
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        var definitionValidation = ValidateProjectionDefinitions(
            batches,
            periods,
            sales,
            growthProfiles,
            waterTemperatures,
            calibrations,
            feedRates,
            mortalityRates,
            includeSalesAndOperations);
        if (!definitionValidation.Success)
        {
            return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.ErrorResult(
                definitionValidation.Message,
                definitionValidation.Message,
                StatusCodes.Status400BadRequest);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _unitOfWork.Db.BudgetPlanFeedingLines.RemoveRange(_unitOfWork.Db.BudgetPlanFeedingLines.Where(x => x.BudgetPlanId == budgetPlanId));
            _unitOfWork.Db.BudgetPlanMortalityLines.RemoveRange(_unitOfWork.Db.BudgetPlanMortalityLines.Where(x => x.BudgetPlanId == budgetPlanId));
            _unitOfWork.Db.BudgetPlanMonthlyProjections.RemoveRange(_unitOfWork.Db.BudgetPlanMonthlyProjections.Where(x => x.BudgetPlanId == budgetPlanId));
            await _unitOfWork.SaveChangesAsync();

            foreach (var batch in batches)
            {
                var liveCount = batch.InitialLiveCount;
                var averageGram = batch.InitialAverageGram;
                var profile = FindGrowthProfile(growthProfiles, batch);

                foreach (var period in periods)
                {
                    var monthIndex = MonthsBetween(batch.GrowthStartYear, batch.GrowthStartMonth, period.Year, period.Month) + 1;
                    var openingBiomassKg = Round(liveCount * averageGram / 1000m);
                    var rawMonthlyGrowth = profile?.Lines.FirstOrDefault(x => x.GrowthMonthNo == monthIndex && !x.IsDeleted)?.MonthlyGrowthGram ?? 0m;
                    var growthQualityPercent = FindGrowthQuality(growthQualities, batch.FishStockId, monthIndex)?.QualityPercent ?? 100m;
                    var monthlyGrowth = Round(rawMonthlyGrowth * growthQualityPercent / 100m);
                    var closingAverageBeforeLoss = averageGram + monthlyGrowth;
                    var periodSales = includeSalesAndOperations
                        ? sales.Where(x => x.BudgetPlanFishBatchId == batch.Id && x.Year == period.Year && x.Month == period.Month).ToList()
                        : new List<BudgetPlanSalesLine>();
                    var salesKg = includeSalesAndOperations
                        ? Math.Min(periodSales.Sum(x => x.SalesTon * 1000m), Round(liveCount * closingAverageBeforeLoss / 1000m))
                        : 0m;
                    var salesCount = includeSalesAndOperations ? periodSales.Sum(x => x.SalesCount ?? 0) : 0;
                    if (salesCount <= 0 && closingAverageBeforeLoss > 0)
                    {
                        salesCount = (int)Math.Round(salesKg * 1000m / closingAverageBeforeLoss, MidpointRounding.AwayFromZero);
                    }

                    salesCount = Math.Min(salesCount, liveCount);
                    var afterSalesLiveCount = Math.Max(0, liveCount - salesCount);
                    var calibration = FindCalibration(calibrations, closingAverageBeforeLoss);
                    var waterTemperature = FindWaterTemperature(waterTemperatures, period.Year, period.Month);
                    var mortalityRate = includeSalesAndOperations
                        ? FindMortalityRateDefinition(mortalityRates, batch.FishStockId, calibration?.Id, monthIndex)?.MortalityRatePercent ?? 0m
                        : 0m;
                    var mortalityCount = Math.Min(afterSalesLiveCount, (int)Math.Round(afterSalesLiveCount * mortalityRate / 100m, MidpointRounding.AwayFromZero));
                    var mortalityKg = Round(mortalityCount * closingAverageBeforeLoss / 1000m);
                    var closingLiveCount = Math.Max(0, afterSalesLiveCount - mortalityCount);
                    var closingBiomassKg = Round(closingLiveCount * closingAverageBeforeLoss / 1000m);

                    var feedRate = includeSalesAndOperations ? FindFeedRate(feedRates, waterTemperature?.Id, calibration?.Id) : null;
                    var averageBiomassKg = (openingBiomassKg + closingBiomassKg) / 2m;
                    var baseFeedKg = feedRate == null ? 0m : Round(averageBiomassKg * (feedRate.FeedAmount / 100m) * DateTime.DaysInMonth(period.Year, period.Month));
                    var feedMortalityRate = feedRate == null
                        ? null
                        : FindFeedMortalityRate(feedMortalityRates, waterTemperature?.Id, calibration?.Id, feedRate.FeedStockId);
                    var mortalityShare = afterSalesLiveCount <= 0 ? 0m : Math.Clamp(mortalityCount / (decimal)afterSalesLiveCount, 0m, 1m);
                    var feedMortalityReductionPercent = feedMortalityRate?.ReductionRatePercent ?? 0m;
                    var feedMortalityReductionKg = Round(baseFeedKg * mortalityShare * feedMortalityReductionPercent / 100m);
                    var feedKg = Math.Max(0m, Round(baseFeedKg - feedMortalityReductionKg));

                    var projection = new BudgetPlanMonthlyProjection
                    {
                        BudgetPlanId = budgetPlanId,
                        BudgetPlanFishBatchId = batch.Id,
                        Year = period.Year,
                        Month = period.Month,
                        MonthIndex = monthIndex,
                        OpeningLiveCount = liveCount,
                        OpeningAverageGram = Round(averageGram),
                        OpeningBiomassKg = openingBiomassKg,
                        RawMonthlyGrowthGram = Round(rawMonthlyGrowth),
                        GrowthQualityPercent = growthQualityPercent,
                        MonthlyGrowthGram = Round(monthlyGrowth),
                        ClosingAverageGram = Round(closingAverageBeforeLoss),
                        SalesTon = Round(salesKg / 1000m),
                        SalesCount = salesCount,
                        MortalityKg = mortalityKg,
                        MortalityCount = mortalityCount,
                        FeedKg = feedKg,
                        FeedMortalityReductionPercent = feedMortalityReductionPercent,
                        FeedMortalityReductionKg = feedMortalityReductionKg,
                        ClosingLiveCount = closingLiveCount,
                        ClosingBiomassKg = closingBiomassKg,
                        CalibrationDefinitionId = calibration?.Id,
                        WaterTemperatureId = waterTemperature?.Id
                    };

                    await _unitOfWork.Repository<BudgetPlanMonthlyProjection>().AddAsync(projection);
                    await _unitOfWork.SaveChangesAsync();

                    if (includeSalesAndOperations)
                    {
                        await _unitOfWork.Repository<BudgetPlanFeedingLine>().AddAsync(new BudgetPlanFeedingLine
                        {
                            BudgetPlanId = budgetPlanId,
                            BudgetPlanMonthlyProjectionId = projection.Id,
                            BudgetPlanFishBatchId = batch.Id,
                            Year = period.Year,
                            Month = period.Month,
                            FeedStockId = feedRate?.FeedStockId,
                            FeedAmountRate = feedRate?.FeedAmount ?? 0m,
                            MortalityReductionPercent = feedMortalityReductionPercent,
                            MortalityReductionKg = feedMortalityReductionKg,
                            FeedKg = feedKg
                        });

                        await _unitOfWork.Repository<BudgetPlanMortalityLine>().AddAsync(new BudgetPlanMortalityLine
                        {
                            BudgetPlanId = budgetPlanId,
                            BudgetPlanMonthlyProjectionId = projection.Id,
                            BudgetPlanFishBatchId = batch.Id,
                            Year = period.Year,
                            Month = period.Month,
                            MortalityRatePercent = mortalityRate,
                            MortalityCount = mortalityCount,
                            MortalityKg = mortalityKg
                        });
                    }

                    liveCount = closingLiveCount;
                    averageGram = closingAverageBeforeLoss;
                }
            }

            plan.Status = includeSalesAndOperations ? BudgetPlanStatus.Calculated : BudgetPlanStatus.GrowthCalculated;
            plan.CalculatedAt = includeSalesAndOperations ? DateTime.Now : null;
            await _unitOfWork.Repository<BudgetPlan>().UpdateAsync(plan);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return await GetProjectionsAsync(budgetPlanId);
    }

    public async Task<ApiResponse<List<BudgetPlanMonthlyProjectionDto>>> GetProjectionsAsync(long budgetPlanId)
    {
        var rows = await ProjectionQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.BudgetPlanFishBatch.BatchCode)
            .ThenBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        return ApiResponse<List<BudgetPlanMonthlyProjectionDto>>.SuccessResult(rows.Select(MapProjection).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanFeedingLineDto>>> GetFeedingLinesAsync(long budgetPlanId)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanFeedingLineDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status != BudgetPlanStatus.Calculated)
        {
            return ApiResponse<List<BudgetPlanFeedingLineDto>>.ErrorResult("Yemleme raporu icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", "Yemleme raporu icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", StatusCodes.Status400BadRequest);
        }

        var rows = await _unitOfWork.Db.BudgetPlanFeedingLines
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.BudgetPlanProject)
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.FishStock)
            .Include(x => x.FeedStock)
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode)
            .ThenBy(x => x.BudgetPlanFishBatch.BatchCode)
            .ToListAsync();

        return ApiResponse<List<BudgetPlanFeedingLineDto>>.SuccessResult(rows.Select(MapFeedingLine).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<List<BudgetPlanMortalityLineDto>>> GetMortalityLinesAsync(long budgetPlanId)
    {
        var plan = await _unitOfWork.Db.BudgetPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == budgetPlanId && !x.IsDeleted);
        if (plan == null)
        {
            return ApiResponse<List<BudgetPlanMortalityLineDto>>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status != BudgetPlanStatus.Calculated)
        {
            return ApiResponse<List<BudgetPlanMortalityLineDto>>.ErrorResult("Fire raporu icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", "Fire raporu icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", StatusCodes.Status400BadRequest);
        }

        var rows = await _unitOfWork.Db.BudgetPlanMortalityLines
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.BudgetPlanProject)
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.FishStock)
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode)
            .ThenBy(x => x.BudgetPlanFishBatch.BatchCode)
            .ToListAsync();

        return ApiResponse<List<BudgetPlanMortalityLineDto>>.SuccessResult(rows.Select(MapMortalityLine).ToList(), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetKpiSummaryDto>> GetKpiSummaryAsync(long budgetPlanId)
    {
        var plan = await PlanQuery().FirstOrDefaultAsync(x => x.Id == budgetPlanId);
        if (plan == null)
        {
            return ApiResponse<BudgetKpiSummaryDto>.ErrorResult("Butce plani bulunamadi.", "Butce plani bulunamadi.", StatusCodes.Status404NotFound);
        }

        if (plan.Status != BudgetPlanStatus.Calculated)
        {
            return ApiResponse<BudgetKpiSummaryDto>.ErrorResult("KPI icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", "KPI icin once satis sonrasi yemleme ve fire hesabi tamamlanmalidir.", StatusCodes.Status400BadRequest);
        }

        var rows = await _unitOfWork.Db.BudgetPlanMonthlyProjections
            .Where(x => x.BudgetPlanId == budgetPlanId && !x.IsDeleted)
            .ToListAsync();

        var initial = plan.FishBatches.Sum(x => x.InitialBiomassKg);
        var final = rows
            .GroupBy(x => x.BudgetPlanFishBatchId)
            .Select(x => x.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).FirstOrDefault()?.ClosingBiomassKg ?? 0m)
            .Sum();
        var sales = rows.Sum(x => x.SalesTon * 1000m);
        var feed = rows.Sum(x => x.FeedKg);
        var mortality = rows.Sum(x => x.MortalityKg);
        var mortalityCount = rows.Sum(x => x.MortalityCount);
        var initialLiveCount = plan.FishBatches.Sum(x => x.InitialLiveCount);
        var salesCount = rows.Sum(x => x.SalesCount);
        var finalLiveCount = rows
            .GroupBy(x => x.BudgetPlanFishBatchId)
            .Select(x => x.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).FirstOrDefault()?.ClosingLiveCount ?? 0)
            .Sum();
        var produced = Math.Max(0m, final + sales + mortality);

        return ApiResponse<BudgetKpiSummaryDto>.SuccessResult(new BudgetKpiSummaryDto
        {
            BudgetPlanId = plan.Id,
            BudgetNo = plan.BudgetNo,
            BudgetCode = plan.BudgetCode,
            InitialBiomassKg = Round(initial),
            FinalBiomassKg = Round(final),
            SalesTon = Round(sales / 1000m),
            FeedKg = Round(feed),
            MortalityKg = Round(mortality),
            MortalityCount = mortalityCount,
            InitialLiveCount = initialLiveCount,
            SalesCount = salesCount,
            FinalLiveCount = finalLiveCount,
            ProducedBiomassKg = Round(produced),
            Fcr = produced <= 0 ? 0m : Round(feed / produced),
            MortalityRatePercent = initialLiveCount <= 0
                ? 0m
                : Round(mortalityCount * 100m / initialLiveCount)
        }, "Islem basarili.");
    }

    public async Task<ApiResponse<PagedResponse<BudgetMortalityRateDefinitionDto>>> GetMortalityRatesAsync(PagedRequest request)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();

        var query = _unitOfWork.Db.BudgetMortalityRateDefinitions
            .AsNoTracking()
            .Include(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .Where(x => !x.IsDeleted)
            .ApplyFilters(request.Filters, request.FilterLogic);

        var totalCount = await query.CountAsync();
        var rows = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

        return ApiResponse<PagedResponse<BudgetMortalityRateDefinitionDto>>.SuccessResult(new PagedResponse<BudgetMortalityRateDefinitionDto>
        {
            Items = rows.Select(MapMortalityRate).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        }, "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetMortalityRateDefinitionDto>> GetMortalityRateAsync(long id)
    {
        var entity = await _unitOfWork.Db.BudgetMortalityRateDefinitions
            .AsNoTracking()
            .Include(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            return ApiResponse<BudgetMortalityRateDefinitionDto>.ErrorResult("Fire orani tanimi bulunamadi.", "Fire orani tanimi bulunamadi.", StatusCodes.Status404NotFound);
        }

        return ApiResponse<BudgetMortalityRateDefinitionDto>.SuccessResult(MapMortalityRate(entity), "Islem basarili.");
    }

    public async Task<ApiResponse<BudgetMortalityRateDefinitionDto>> CreateMortalityRateAsync(CreateBudgetMortalityRateDefinitionDto dto)
    {
        var validation = ValidateMortalityRate(dto);
        if (!validation.Success)
        {
            return ApiResponse<BudgetMortalityRateDefinitionDto>.ErrorResult(validation.Message, validation.Message, StatusCodes.Status400BadRequest);
        }

        var entity = new BudgetMortalityRateDefinition
        {
            FishStockId = dto.FishStockId,
            CalibrationDefinitionId = dto.CalibrationDefinitionId,
            GrowthMonthNo = dto.GrowthMonthNo,
            MortalityRatePercent = dto.MortalityRatePercent,
            Description = NormalizeOptional(dto.Description)
        };

        await _unitOfWork.Repository<BudgetMortalityRateDefinition>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _unitOfWork.Db.BudgetMortalityRateDefinitions
            .Include(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .FirstAsync(x => x.Id == entity.Id);

        return ApiResponse<BudgetMortalityRateDefinitionDto>.SuccessResult(MapMortalityRate(saved), "Fire orani kaydedildi.");
    }

    public async Task<ApiResponse<BudgetMortalityRateDefinitionDto>> UpdateMortalityRateAsync(long id, CreateBudgetMortalityRateDefinitionDto dto)
    {
        var entity = await _unitOfWork.Db.BudgetMortalityRateDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
        {
            return ApiResponse<BudgetMortalityRateDefinitionDto>.ErrorResult("Fire orani tanimi bulunamadi.", "Fire orani tanimi bulunamadi.", StatusCodes.Status404NotFound);
        }

        var validation = ValidateMortalityRate(dto);
        if (!validation.Success)
        {
            return ApiResponse<BudgetMortalityRateDefinitionDto>.ErrorResult(validation.Message, validation.Message, StatusCodes.Status400BadRequest);
        }

        entity.FishStockId = dto.FishStockId;
        entity.CalibrationDefinitionId = dto.CalibrationDefinitionId;
        entity.GrowthMonthNo = dto.GrowthMonthNo;
        entity.MortalityRatePercent = dto.MortalityRatePercent;
        entity.Description = NormalizeOptional(dto.Description);

        await _unitOfWork.Repository<BudgetMortalityRateDefinition>().UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _unitOfWork.Db.BudgetMortalityRateDefinitions
            .AsNoTracking()
            .Include(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .FirstAsync(x => x.Id == id);

        return ApiResponse<BudgetMortalityRateDefinitionDto>.SuccessResult(MapMortalityRate(saved), "Fire orani guncellendi.");
    }

    public async Task<ApiResponse<bool>> DeleteMortalityRateAsync(long id)
    {
        var deleted = await _unitOfWork.Repository<BudgetMortalityRateDefinition>().SoftDeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResult("Fire orani tanimi bulunamadi.", "Fire orani tanimi bulunamadi.", StatusCodes.Status404NotFound);
        }

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Fire orani silindi.");
    }

    private IQueryable<BudgetPlan> PlanQuery()
    {
        return _unitOfWork.Db.BudgetPlans
            .AsSplitQuery()
            .Include(x => x.Projects)
            .Include(x => x.FishBatches)
            .Include(x => x.FishBatchAdjustments)
            .Include(x => x.MonthlyProjections)
            .Include(x => x.SalesLines)
            .Include(x => x.FeedingLines)
            .Include(x => x.MortalityLines)
            .Include(x => x.ExchangeRates)
            .Include(x => x.FishPrices)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<BudgetPlanFishBatch> FishBatchQuery()
    {
        return _unitOfWork.Db.BudgetPlanFishBatches
            .Include(x => x.BudgetPlanProject)
            .Include(x => x.FishStock)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<BudgetPlanMonthlyProjection> ProjectionQuery()
    {
        return _unitOfWork.Db.BudgetPlanMonthlyProjections
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.BudgetPlanProject)
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .Include(x => x.WaterTemperature)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<BudgetPlanFishBatchAdjustment> FishBatchAdjustmentQuery()
    {
        return _unitOfWork.Db.BudgetPlanFishBatchAdjustments
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.BudgetPlanProject)
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.FishStock)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<BudgetPlanSalesLine> SalesLineQuery()
    {
        return _unitOfWork.Db.BudgetPlanSalesLines
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.BudgetPlanProject)
            .Include(x => x.BudgetPlanFishBatch).ThenInclude(x => x.FishStock)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<BudgetPlanFishPrice> FishPriceQuery()
    {
        return _unitOfWork.Db.BudgetPlanFishPrices
            .AsNoTracking()
            .Include(x => x.FishStock)
            .Include(x => x.CalibrationDefinition)
            .Where(x => !x.IsDeleted);
    }

    private async Task<BudgetPlanProject> EnsurePlanProjectAsync(BudgetPlan plan, BudgetPlanSourceType sourceType, long? sourceProjectId, string projectCode, string projectName)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(projectCode) ? $"SANAL-{plan.Id}" : projectCode.Trim();
        var normalizedName = string.IsNullOrWhiteSpace(projectName) ? normalizedCode : projectName.Trim();
        var existing = await _unitOfWork.Db.BudgetPlanProjects.FirstOrDefaultAsync(x =>
            x.BudgetPlanId == plan.Id &&
            x.ProjectCode == normalizedCode &&
            !x.IsDeleted);
        if (existing != null)
        {
            return existing;
        }

        var entity = new BudgetPlanProject
        {
            BudgetPlanId = plan.Id,
            SourceType = sourceType,
            SourceProjectId = sourceProjectId,
            ProjectCode = normalizedCode,
            ProjectName = normalizedName
        };

        await _unitOfWork.Repository<BudgetPlanProject>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity;
    }

    private async Task<List<BudgetPlanFishBatchDto>> LoadPlanFishBatchesAsync(long budgetPlanId)
    {
        var rows = await FishBatchQuery()
            .Where(x => x.BudgetPlanId == budgetPlanId)
            .OrderBy(x => x.BudgetPlanProject.ProjectCode)
            .ThenBy(x => x.BatchCode)
            .ToListAsync();

        return rows.Select(MapFishBatch).ToList();
    }

    private async Task<string> GenerateBudgetNoAsync(int startYear, int startMonth)
    {
        var prefix = $"BUD-{startYear}{startMonth:00}";
        var count = await _unitOfWork.Db.BudgetPlans.CountAsync(x => x.BudgetNo.StartsWith(prefix));
        return $"{prefix}-{count + 1:0000}";
    }

    private static ApiResponse<bool> ValidatePlanPeriod(int startYear, int startMonth, int endYear, int endMonth)
    {
        if (startYear < 2000 || startYear > 2100 || endYear < 2000 || endYear > 2100 || !IsValidMonth(startMonth) || !IsValidMonth(endMonth))
        {
            return ApiResponse<bool>.ErrorResult("Butce donemi hatali.", "Butce donemi hatali.", StatusCodes.Status400BadRequest);
        }

        if ((endYear * 12 + endMonth) < (startYear * 12 + startMonth))
        {
            return ApiResponse<bool>.ErrorResult("Bitis donemi baslangictan once olamaz.", "Bitis donemi baslangictan once olamaz.", StatusCodes.Status400BadRequest);
        }

        return ApiResponse<bool>.SuccessResult(true, "Valid");
    }

    private static List<BudgetPeriod> BuildPeriods(int startYear, int startMonth, int endYear, int endMonth)
    {
        var periods = new List<BudgetPeriod>();
        var year = startYear;
        var month = startMonth;

        while ((year * 12 + month) <= (endYear * 12 + endMonth))
        {
            periods.Add(new BudgetPeriod(year, month));
            month++;
            if (month <= 12)
            {
                continue;
            }

            month = 1;
            year++;
        }

        return periods;
    }

    private static int MonthsBetween(int startYear, int startMonth, int year, int month)
    {
        return (year - startYear) * 12 + (month - startMonth);
    }

    private static BudgetCalibrationDefinition? FindCalibration(List<BudgetCalibrationDefinition> calibrations, decimal averageGram)
    {
        return calibrations
            .Select(x => new { Calibration = x, Range = TryParseRange(x.CalibrationInfo) ?? TryParseRange(x.CalibrationCode) })
            .Where(x => x.Range != null && averageGram >= x.Range.Value.Min && averageGram <= x.Range.Value.Max)
            .OrderBy(x => x.Range!.Value.Max - x.Range.Value.Min)
            .Select(x => x.Calibration)
            .FirstOrDefault();
    }

    private static (decimal Min, decimal Max)? TryParseRange(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var matches = Regex.Matches(value, @"\d+([.,]\d+)?");
        if (matches.Count == 0)
        {
            return null;
        }

        var first = ParseDecimal(matches[0].Value);
        var second = matches.Count > 1 ? ParseDecimal(matches[1].Value) : first;

        if (matches.Count == 1 && IsOpenEndedUpperRange(value))
        {
            return (first, decimal.MaxValue);
        }

        if (matches.Count == 1 && IsOpenEndedLowerRange(value))
        {
            return (0m, first);
        }

        return (Math.Min(first, second), Math.Max(first, second));
    }

    private static BudgetFeedConsumptionRate? FindFeedRate(List<BudgetFeedConsumptionRate> rates, long? waterTemperatureId, long? calibrationId)
    {
        if (!waterTemperatureId.HasValue || !calibrationId.HasValue)
        {
            return null;
        }

        return rates.FirstOrDefault(x =>
            x.WaterTemperatureId == waterTemperatureId.Value &&
            x.CalibrationDefinitionId == calibrationId.Value);
    }

    private static BudgetFeedMortalityRate? FindFeedMortalityRate(
        List<BudgetFeedMortalityRate> rates,
        long? waterTemperatureId,
        long? calibrationId,
        long feedStockId)
    {
        if (!waterTemperatureId.HasValue || !calibrationId.HasValue) return null;
        return rates.FirstOrDefault(x =>
            x.WaterTemperatureId == waterTemperatureId.Value &&
            x.CalibrationDefinitionId == calibrationId.Value &&
            x.FeedStockId == feedStockId);
    }

    private static BudgetFishGrowthQuality? FindGrowthQuality(
        List<BudgetFishGrowthQuality> qualities,
        long fishStockId,
        int growthMonthNo) =>
        qualities.FirstOrDefault(x => x.FishStockId == fishStockId && x.GrowthMonthNo == growthMonthNo);

    private static BudgetFishGrowthProfile? FindGrowthProfile(
        List<BudgetFishGrowthProfile> profiles,
        BudgetPlanFishBatch batch)
    {
        var exactProfile = profiles.FirstOrDefault(x =>
            x.StockId == batch.FishStockId &&
            x.StartMonth == batch.GrowthStartMonth);
        if (exactProfile != null)
        {
            return exactProfile;
        }

        var speciesKey = FindFishSpeciesKey(batch.FishStock?.StockName);
        if (speciesKey == null)
        {
            return null;
        }

        return profiles
            .Where(x => x.StartMonth == batch.GrowthStartMonth)
            .Where(x => FindFishSpeciesKey(x.Stock?.StockName) == speciesKey)
            .OrderBy(x => x.Id)
            .FirstOrDefault();
    }

    private static string? FindFishSpeciesKey(string? stockName)
    {
        if (string.IsNullOrWhiteSpace(stockName))
        {
            return null;
        }

        var normalized = new string(stockName
            .Normalize(NormalizationForm.FormD)
            .Where(x => CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .ToLowerInvariant();

        if (Regex.IsMatch(normalized, @"\blevrek\b|\bsea\s*bass\b"))
        {
            return "LEVREK";
        }

        if (Regex.IsMatch(normalized, @"\bcipura\b|\bsea\s*bream\b"))
        {
            return "CIPURA";
        }

        return null;
    }

    private static BudgetWaterTemperature? FindWaterTemperature(List<BudgetWaterTemperature> temperatures, int year, int month)
    {
        return temperatures.FirstOrDefault(x => x.Year == year && x.Month == month) ??
               temperatures.FirstOrDefault(x => x.Month == month);
    }

    private static bool IsOpenEndedUpperRange(string value)
    {
        return value.Contains("+", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("üzeri", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("uzeri", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("üstü", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("ustu", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("above", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("greater", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOpenEndedLowerRange(string value)
    {
        return value.Contains("altı", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("alti", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("below", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("less", StringComparison.OrdinalIgnoreCase);
    }

    private static BudgetMortalityRateDefinition? FindMortalityRateDefinition(List<BudgetMortalityRateDefinition> rates, long fishStockId, long? calibrationId, int growthMonthNo)
    {
        return rates
            .Where(x => !x.FishStockId.HasValue || x.FishStockId == fishStockId)
            .Where(x => !x.CalibrationDefinitionId.HasValue || x.CalibrationDefinitionId == calibrationId)
            .Where(x => !x.GrowthMonthNo.HasValue || x.GrowthMonthNo == growthMonthNo)
            .OrderByDescending(x => x.FishStockId.HasValue)
            .ThenByDescending(x => x.CalibrationDefinitionId.HasValue)
            .ThenByDescending(x => x.GrowthMonthNo.HasValue)
            .FirstOrDefault();
    }

    private static ApiResponse<bool> ValidateProjectionDefinitions(
        List<BudgetPlanFishBatch> batches,
        List<BudgetPeriod> periods,
        List<BudgetPlanSalesLine> sales,
        List<BudgetFishGrowthProfile> growthProfiles,
        List<BudgetWaterTemperature> waterTemperatures,
        List<BudgetCalibrationDefinition> calibrations,
        List<BudgetFeedConsumptionRate> feedRates,
        List<BudgetMortalityRateDefinition> mortalityRates,
        bool includeSalesAndOperations)
    {
        var errors = new List<string>();

        foreach (var batch in batches.OrderBy(x => x.BudgetPlanProject.ProjectCode).ThenBy(x => x.BatchCode))
        {
            var liveCount = batch.InitialLiveCount;
            var averageGram = batch.InitialAverageGram;
            var profile = FindGrowthProfile(growthProfiles, batch);

            if (profile == null)
            {
                errors.Add($"{DescribeBudgetBatch(batch)} icin {batch.GrowthStartMonth}. ay baslangicli buyume profili yok.");
                continue;
            }

            foreach (var period in periods)
            {
                var monthIndex = MonthsBetween(batch.GrowthStartYear, batch.GrowthStartMonth, period.Year, period.Month) + 1;
                var growthLine = profile.Lines.FirstOrDefault(x => x.GrowthMonthNo == monthIndex && !x.IsDeleted);
                if (monthIndex < 1 || growthLine == null)
                {
                    errors.Add($"{DescribeBudgetBatch(batch)} icin {period.Year}/{period.Month:00} donemi {monthIndex}. buyume ayi tanimi yok.");
                    continue;
                }

                var closingAverageBeforeLoss = averageGram + growthLine.MonthlyGrowthGram;
                var calibration = FindCalibration(calibrations, closingAverageBeforeLoss);
                if (calibration == null)
                {
                    errors.Add($"{DescribeBudgetBatch(batch)} icin {period.Year}/{period.Month:00} doneminde {Round(closingAverageBeforeLoss)} gr kalibrasyon tanimi yok.");
                }

                var periodSales = includeSalesAndOperations
                    ? sales.Where(x => x.BudgetPlanFishBatchId == batch.Id && x.Year == period.Year && x.Month == period.Month).ToList()
                    : new List<BudgetPlanSalesLine>();
                var salesKg = includeSalesAndOperations
                    ? Math.Min(periodSales.Sum(x => x.SalesTon * 1000m), Round(liveCount * closingAverageBeforeLoss / 1000m))
                    : 0m;
                var salesCount = includeSalesAndOperations ? periodSales.Sum(x => x.SalesCount ?? 0) : 0;
                if (salesCount <= 0 && closingAverageBeforeLoss > 0)
                {
                    salesCount = (int)Math.Round(salesKg * 1000m / closingAverageBeforeLoss, MidpointRounding.AwayFromZero);
                }

                salesCount = Math.Min(salesCount, liveCount);
                var afterSalesLiveCount = Math.Max(0, liveCount - salesCount);

                if (includeSalesAndOperations)
                {
                    var waterTemperature = FindWaterTemperature(waterTemperatures, period.Year, period.Month);
                    if (waterTemperature == null)
                    {
                        errors.Add($"{period.Year}/{period.Month:00} donemi icin su sicakligi tanimi yok.");
                    }

                    if (calibration != null && waterTemperature != null && FindFeedRate(feedRates, waterTemperature.Id, calibration.Id) == null)
                    {
                        errors.Add($"{period.Year}/{period.Month:00} donemi ve {calibration.CalibrationCode} kalibrasyonu icin yem tuketim orani tanimi yok.");
                    }

                    var mortalityRate = calibration == null
                        ? null
                        : FindMortalityRateDefinition(mortalityRates, batch.FishStockId, calibration.Id, monthIndex);
                    if (mortalityRate == null)
                    {
                        errors.Add($"{DescribeBudgetBatch(batch)} icin {period.Year}/{period.Month:00} donemi fire orani tanimi yok.");
                    }

                    var mortalityCount = Math.Min(afterSalesLiveCount, (int)Math.Round(afterSalesLiveCount * (mortalityRate?.MortalityRatePercent ?? 0m) / 100m, MidpointRounding.AwayFromZero));
                    liveCount = Math.Max(0, afterSalesLiveCount - mortalityCount);
                }

                averageGram = closingAverageBeforeLoss;

                if (errors.Count >= 20)
                {
                    return ApiResponse<bool>.ErrorResult(BuildDefinitionErrorMessage(errors), BuildDefinitionErrorMessage(errors), StatusCodes.Status400BadRequest);
                }
            }
        }

        return errors.Count == 0
            ? ApiResponse<bool>.SuccessResult(true, "Valid")
            : ApiResponse<bool>.ErrorResult(BuildDefinitionErrorMessage(errors), BuildDefinitionErrorMessage(errors), StatusCodes.Status400BadRequest);
    }

    private static string DescribeBudgetBatch(BudgetPlanFishBatch batch)
    {
        var projectCode = batch.BudgetPlanProject?.ProjectCode;
        var stockCode = batch.FishStock?.ErpStockCode;
        return $"{projectCode ?? "-"} / {batch.BatchCode} / {stockCode ?? batch.FishStockId.ToString()}";
    }

    private static string BuildDefinitionErrorMessage(List<string> errors)
    {
        return $"Butce hesaplama icin eksik tanimlar var: {string.Join(" | ", errors.Distinct().Take(20))}";
    }

    private static BudgetPlanDto MapPlan(BudgetPlan plan)
    {
        return new BudgetPlanDto
        {
            Id = plan.Id,
            BudgetNo = plan.BudgetNo,
            BudgetCode = plan.BudgetCode,
            BudgetName = plan.BudgetName,
            StartYear = plan.StartYear,
            StartMonth = plan.StartMonth,
            EndYear = plan.EndYear,
            EndMonth = plan.EndMonth,
            Status = plan.Status,
            Description = plan.Description,
            CalculatedAt = plan.CalculatedAt,
            FishBatchCount = plan.FishBatches.Count(x => !x.IsDeleted),
            TotalInitialBiomassKg = Round(plan.FishBatches.Where(x => !x.IsDeleted).Sum(x => x.InitialBiomassKg)),
            TotalSalesTon = Round(plan.MonthlyProjections.Where(x => !x.IsDeleted).Sum(x => x.SalesTon)),
            TotalFeedKg = Round(plan.MonthlyProjections.Where(x => !x.IsDeleted).Sum(x => x.FeedKg)),
            TotalMortalityKg = Round(plan.MonthlyProjections.Where(x => !x.IsDeleted).Sum(x => x.MortalityKg))
        };
    }

    private static BudgetPlanFishBatchDto MapFishBatch(BudgetPlanFishBatch entity)
    {
        return new BudgetPlanFishBatchDto
        {
            Id = entity.Id,
            BudgetPlanId = entity.BudgetPlanId,
            BudgetPlanProjectId = entity.BudgetPlanProjectId,
            ProjectCode = entity.BudgetPlanProject.ProjectCode,
            ProjectName = entity.BudgetPlanProject.ProjectName,
            SourceType = entity.SourceType,
            SourceFishBatchId = entity.SourceFishBatchId,
            FishStockId = entity.FishStockId,
            FishStockCode = entity.FishStock.ErpStockCode,
            FishStockName = entity.FishStock.StockName,
            BatchCode = entity.BatchCode,
            InitialLiveCount = entity.InitialLiveCount,
            InitialAverageGram = entity.InitialAverageGram,
            InitialBiomassKg = entity.InitialBiomassKg,
            InitialUnitCost = entity.InitialUnitCost,
            InitialSmmAmount = entity.InitialSmmAmount,
            GrowthStartYear = entity.GrowthStartYear,
            GrowthStartMonth = entity.GrowthStartMonth,
            Note = entity.Note
        };
    }

    private static BudgetPlanSalesLineDto MapSalesLine(BudgetPlanSalesLine entity, decimal? exchangeRate = null)
    {
        var salesAmountEuro = Round(entity.SalesTon * 1000m * (entity.UnitPrice ?? 0m));
        return new BudgetPlanSalesLineDto
        {
            Id = entity.Id,
            BudgetPlanFishBatchId = entity.BudgetPlanFishBatchId,
            ProjectCode = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
            ProjectName = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectName,
            BatchCode = entity.BudgetPlanFishBatch.BatchCode,
            FishStockCode = entity.BudgetPlanFishBatch.FishStock.ErpStockCode,
            FishStockName = entity.BudgetPlanFishBatch.FishStock.StockName,
            Year = entity.Year,
            Month = entity.Month,
            SalesTon = entity.SalesTon,
            SalesCount = entity.SalesCount,
            UnitPrice = entity.UnitPrice,
            SalesAmount = salesAmountEuro,
            UnitPriceEuro = entity.UnitPrice,
            SalesAmountEuro = salesAmountEuro,
            ExchangeRate = exchangeRate,
            SalesAmountTry = exchangeRate.HasValue ? Round(salesAmountEuro * exchangeRate.Value) : null,
            Description = entity.Description
        };
    }

    private static BudgetPlanExchangeRateDto MapExchangeRate(BudgetPlanExchangeRate entity)
    {
        return new BudgetPlanExchangeRateDto
        {
            Id = entity.Id,
            BudgetPlanId = entity.BudgetPlanId,
            Year = entity.Year,
            Month = entity.Month,
            CurrencyCode = entity.CurrencyCode,
            RateType = entity.RateType,
            ExchangeRate = entity.ExchangeRate,
            SourceType = entity.SourceType,
            SourceReference = entity.SourceReference,
            IsManualOverride = entity.IsManualOverride,
            Description = entity.Description
        };
    }

    private static BudgetPlanFishPriceDto MapFishPrice(BudgetPlanFishPrice entity, decimal? exchangeRate)
    {
        var normalizedCurrency = NormalizeCurrencyCode(entity.CurrencyCode);
        var resolvedExchangeRate = normalizedCurrency == "TRY" ? 1m : exchangeRate;
        return new BudgetPlanFishPriceDto
        {
            Id = entity.Id,
            BudgetPlanId = entity.BudgetPlanId,
            FishStockId = entity.FishStockId,
            FishStockCode = entity.FishStock?.ErpStockCode,
            FishStockName = entity.FishStock?.StockName,
            CalibrationDefinitionId = entity.CalibrationDefinitionId,
            CalibrationCode = entity.CalibrationDefinition.CalibrationCode,
            CalibrationInfo = entity.CalibrationDefinition.CalibrationInfo,
            Year = entity.Year,
            Month = entity.Month,
            PriceType = entity.PriceType,
            MarketType = entity.MarketType,
            CurrencyCode = normalizedCurrency,
            CurrencyName = ResolveCurrencyName(normalizedCurrency),
            UnitPrice = entity.UnitPrice,
            UnitPriceEuro = normalizedCurrency == "EUR" ? entity.UnitPrice : null,
            IncreaseRatePercent = entity.IncreaseRatePercent,
            IncreasePeriodMonths = entity.IncreasePeriodMonths,
            ExchangeRate = resolvedExchangeRate,
            UnitPriceTry = resolvedExchangeRate.HasValue ? Round(entity.UnitPrice * resolvedExchangeRate.Value) : null,
            Description = entity.Description
        };
    }

    private static BudgetPlanFishBatchAdjustmentDto MapFishBatchAdjustment(BudgetPlanFishBatchAdjustment entity)
    {
        return new BudgetPlanFishBatchAdjustmentDto
        {
            Id = entity.Id,
            BudgetPlanFishBatchId = entity.BudgetPlanFishBatchId,
            AdjustmentType = entity.AdjustmentType,
            LiveCount = entity.LiveCount,
            AverageGram = entity.AverageGram,
            BiomassKg = entity.BiomassKg,
            Description = entity.Description,
            ProjectCode = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
            ProjectName = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectName,
            BatchCode = entity.BudgetPlanFishBatch.BatchCode,
            FishStockCode = entity.BudgetPlanFishBatch.FishStock.ErpStockCode,
            FishStockName = entity.BudgetPlanFishBatch.FishStock.StockName
        };
    }

    private static BudgetPlanFeedingLineDto MapFeedingLine(BudgetPlanFeedingLine entity)
    {
        return new BudgetPlanFeedingLineDto
        {
            Id = entity.Id,
            BudgetPlanFishBatchId = entity.BudgetPlanFishBatchId,
            ProjectCode = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
            ProjectName = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectName,
            BatchCode = entity.BudgetPlanFishBatch.BatchCode,
            FishStockCode = entity.BudgetPlanFishBatch.FishStock.ErpStockCode,
            FishStockName = entity.BudgetPlanFishBatch.FishStock.StockName,
            Year = entity.Year,
            Month = entity.Month,
            FeedStockId = entity.FeedStockId,
            FeedStockCode = entity.FeedStock?.ErpStockCode,
            FeedStockName = entity.FeedStock?.StockName,
            FeedAmountRate = entity.FeedAmountRate,
            MortalityReductionPercent = entity.MortalityReductionPercent,
            MortalityReductionKg = entity.MortalityReductionKg,
            FeedKg = entity.FeedKg
        };
    }

    private static BudgetPlanMortalityLineDto MapMortalityLine(BudgetPlanMortalityLine entity)
    {
        return new BudgetPlanMortalityLineDto
        {
            Id = entity.Id,
            BudgetPlanFishBatchId = entity.BudgetPlanFishBatchId,
            ProjectCode = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectCode,
            ProjectName = entity.BudgetPlanFishBatch.BudgetPlanProject.ProjectName,
            BatchCode = entity.BudgetPlanFishBatch.BatchCode,
            FishStockCode = entity.BudgetPlanFishBatch.FishStock.ErpStockCode,
            FishStockName = entity.BudgetPlanFishBatch.FishStock.StockName,
            Year = entity.Year,
            Month = entity.Month,
            MortalityRatePercent = entity.MortalityRatePercent,
            MortalityCount = entity.MortalityCount,
            MortalityKg = entity.MortalityKg
        };
    }

    private static BudgetPlanMonthlyProjectionDto MapProjection(BudgetPlanMonthlyProjection entity)
    {
        return new BudgetPlanMonthlyProjectionDto
        {
            Id = entity.Id,
            BudgetPlanFishBatchId = entity.BudgetPlanFishBatchId,
            BatchCode = entity.BudgetPlanFishBatch.BatchCode,
            Year = entity.Year,
            Month = entity.Month,
            MonthIndex = entity.MonthIndex,
            OpeningLiveCount = entity.OpeningLiveCount,
            OpeningAverageGram = entity.OpeningAverageGram,
            OpeningBiomassKg = entity.OpeningBiomassKg,
            RawMonthlyGrowthGram = entity.RawMonthlyGrowthGram,
            GrowthQualityPercent = entity.GrowthQualityPercent,
            MonthlyGrowthGram = entity.MonthlyGrowthGram,
            ClosingAverageGram = entity.ClosingAverageGram,
            SalesTon = entity.SalesTon,
            SalesCount = entity.SalesCount,
            MortalityKg = entity.MortalityKg,
            MortalityCount = entity.MortalityCount,
            FeedKg = entity.FeedKg,
            FeedMortalityReductionPercent = entity.FeedMortalityReductionPercent,
            FeedMortalityReductionKg = entity.FeedMortalityReductionKg,
            ClosingLiveCount = entity.ClosingLiveCount,
            ClosingBiomassKg = entity.ClosingBiomassKg,
            CalibrationCode = entity.CalibrationDefinition?.CalibrationCode,
            WaterTemperatureCelsius = entity.WaterTemperature?.WaterTemperatureCelsius
        };
    }

    private static BudgetMortalityRateDefinitionDto MapMortalityRate(BudgetMortalityRateDefinition entity)
    {
        return new BudgetMortalityRateDefinitionDto
        {
            Id = entity.Id,
            FishStockId = entity.FishStockId,
            FishStockCode = entity.FishStock?.ErpStockCode,
            FishStockName = entity.FishStock?.StockName,
            CalibrationDefinitionId = entity.CalibrationDefinitionId,
            CalibrationCode = entity.CalibrationDefinition?.CalibrationCode,
            GrowthMonthNo = entity.GrowthMonthNo,
            MortalityRatePercent = entity.MortalityRatePercent,
            Description = entity.Description
        };
    }

    private static ApiResponse<bool> ValidateMortalityRate(CreateBudgetMortalityRateDefinitionDto dto)
    {
        if (dto.MortalityRatePercent < 0 || dto.MortalityRatePercent > 100)
        {
            return ApiResponse<bool>.ErrorResult("Fire orani 0 ile 100 arasinda olmalidir.", "Fire orani 0 ile 100 arasinda olmalidir.", StatusCodes.Status400BadRequest);
        }

        if (dto.GrowthMonthNo is < 1 or > 100)
        {
            return ApiResponse<bool>.ErrorResult("Buyutme ayi 1 ile 100 arasinda olmalidir.", "Buyutme ayi 1 ile 100 arasinda olmalidir.", StatusCodes.Status400BadRequest);
        }

        return ApiResponse<bool>.SuccessResult(true, "Valid");
    }

    private async Task<decimal?> FindFishPriceEuroAsync(long budgetPlanId, long budgetPlanFishBatchId, int year, int month)
    {
        var projection = await _unitOfWork.Db.BudgetPlanMonthlyProjections
            .AsNoTracking()
            .Include(x => x.BudgetPlanFishBatch)
            .FirstOrDefaultAsync(x =>
                x.BudgetPlanId == budgetPlanId &&
                x.BudgetPlanFishBatchId == budgetPlanFishBatchId &&
                x.Year == year &&
                x.Month == month &&
                !x.IsDeleted);

        if (projection?.CalibrationDefinitionId == null)
        {
            return null;
        }

        return await _unitOfWork.Db.BudgetPlanFishPrices
            .AsNoTracking()
            .Where(x =>
                x.BudgetPlanId == budgetPlanId &&
                x.CalibrationDefinitionId == projection.CalibrationDefinitionId.Value &&
                x.Year == year &&
                x.Month == month &&
                x.PriceType == BudgetFishPriceType.Sales &&
                x.MarketType == BudgetMarketType.Domestic &&
                x.CurrencyCode == "EUR" &&
                (!x.FishStockId.HasValue || x.FishStockId == projection.BudgetPlanFishBatch.FishStockId) &&
                !x.IsDeleted)
            .OrderByDescending(x => x.FishStockId.HasValue)
            .Select(x => (decimal?)x.UnitPrice)
            .FirstOrDefaultAsync();
    }

    private async Task<Dictionary<string, decimal>> LoadFishPriceExchangeRateLookupAsync(
        long budgetPlanId,
        IReadOnlyCollection<BudgetPlanFishPrice> prices)
    {
        var currencyCodes = prices
            .Select(x => NormalizeCurrencyCode(x.CurrencyCode))
            .Where(x => x != "TRY")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (currencyCodes.Count == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var rates = await _unitOfWork.Db.BudgetPlanExchangeRates
            .AsNoTracking()
            .Where(x =>
                x.BudgetPlanId == budgetPlanId &&
                currencyCodes.Contains(x.CurrencyCode) &&
                !x.IsDeleted)
            .ToListAsync();

        return rates
            .GroupBy(x => FishPriceExchangeRateKey(x.Year, x.Month, x.CurrencyCode), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(row => row.IsManualOverride).ThenByDescending(row => row.Id).First().ExchangeRate,
                StringComparer.OrdinalIgnoreCase);
    }

    private static decimal? ResolveFishPriceExchangeRate(
        IReadOnlyDictionary<string, decimal> exchangeRates,
        int year,
        int month,
        string currencyCode)
    {
        var normalizedCurrency = NormalizeCurrencyCode(currencyCode);
        if (normalizedCurrency == "TRY")
        {
            return 1m;
        }

        return exchangeRates.TryGetValue(FishPriceExchangeRateKey(year, month, normalizedCurrency), out var rate)
            ? rate
            : null;
    }

    private static string FishPriceExchangeRateKey(int year, int month, string currencyCode)
    {
        return $"{year:D4}-{month:D2}-{NormalizeCurrencyCode(currencyCode)}";
    }

    private static decimal ResolveUnitPrice(decimal unitPrice, decimal? legacyUnitPriceEuro)
    {
        return unitPrice == 0m && legacyUnitPriceEuro.HasValue ? legacyUnitPriceEuro.Value : unitPrice;
    }

    private static decimal CalculateEscalatedPrice(
        decimal basePrice,
        decimal increaseRatePercent,
        int increasePeriodMonths,
        int zeroBasedMonthIndex)
    {
        var increaseCount = zeroBasedMonthIndex / increasePeriodMonths;
        var multiplier = 1m + increaseRatePercent / 100m;
        var result = basePrice;
        for (var index = 0; index < increaseCount; index++)
        {
            result *= multiplier;
        }

        return Round(result);
    }

    private static string ResolveCurrencyName(string currencyCode)
    {
        return currencyCode switch
        {
            "TRY" => "Turk Lirasi",
            "EUR" => "Euro",
            "USD" => "ABD Dolari",
            "GBP" => "Ingiliz Sterlini",
            _ => currencyCode
        };
    }

    private async Task<Dictionary<BudgetPeriod, decimal>> LoadExchangeRateLookupAsync(long budgetPlanId, string currencyCode)
    {
        var normalizedCurrency = NormalizeCurrencyCode(currencyCode);
        return await _unitOfWork.Db.BudgetPlanExchangeRates
            .AsNoTracking()
            .Where(x =>
                x.BudgetPlanId == budgetPlanId &&
                x.CurrencyCode == normalizedCurrency &&
                !x.IsDeleted)
            .GroupBy(x => new { x.Year, x.Month })
            .Select(x => new
            {
                x.Key.Year,
                x.Key.Month,
                ExchangeRate = x.OrderByDescending(row => row.IsManualOverride).ThenByDescending(row => row.Id).Select(row => row.ExchangeRate).FirstOrDefault()
            })
            .ToDictionaryAsync(x => new BudgetPeriod(x.Year, x.Month), x => x.ExchangeRate);
    }

    private async Task<decimal?> FindExchangeRateAsync(long budgetPlanId, int year, int month, string currencyCode)
    {
        var normalizedCurrency = NormalizeCurrencyCode(currencyCode);
        return await _unitOfWork.Db.BudgetPlanExchangeRates
            .AsNoTracking()
            .Where(x =>
                x.BudgetPlanId == budgetPlanId &&
                x.Year == year &&
                x.Month == month &&
                x.CurrencyCode == normalizedCurrency &&
                !x.IsDeleted)
            .OrderByDescending(x => x.IsManualOverride)
            .ThenByDescending(x => x.Id)
            .Select(x => (decimal?)x.ExchangeRate)
            .FirstOrDefaultAsync();
    }

    private static bool IsPeriodWithinPlan(BudgetPlan plan, int year, int month)
    {
        if (!IsValidMonth(month))
        {
            return false;
        }

        var period = year * 12 + month;
        return period >= plan.StartYear * 12 + plan.StartMonth &&
            period <= plan.EndYear * 12 + plan.EndMonth;
    }

    private static BudgetPlanStatus TrimCopiedStatus(BudgetPlanStatus status)
    {
        return status >= BudgetPlanStatus.Adjusted ? BudgetPlanStatus.Adjusted : status;
    }

    private static bool IsValidMonth(int month)
    {
        return month >= 1 && month <= 12;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.Parse(value.Replace(',', '.'), global::System.Globalization.CultureInfo.InvariantCulture);
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRequired(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string NormalizeCurrencyCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    }

    private sealed record BalanceSeed(FishBatch FishBatch, int LiveCount, decimal AverageGram, decimal BiomassGram, DateTime AsOfDate);
    private sealed record BudgetPeriod(int Year, int Month);
}
