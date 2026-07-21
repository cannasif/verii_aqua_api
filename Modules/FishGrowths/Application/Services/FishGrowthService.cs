using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.FishGrowths.Application.Services;

public class FishGrowthService : IFishGrowthService
{
    private const string ReferenceTable = "RII_FISH_GROWTH";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceLedgerManager _balanceLedgerManager;
    private readonly ILocalizationService _localizationService;

    public FishGrowthService(
        IUnitOfWork unitOfWork,
        IBalanceLedgerManager balanceLedgerManager,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _balanceLedgerManager = balanceLedgerManager;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<PagedResponse<FishGrowthDto>>> GetAllAsync(PagedRequest request)
    {
        try
        {
            request ??= new PagedRequest();
            request.Filters ??= new List<Filter>();

            var query = _unitOfWork.Db.FishGrowths
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Project)
                .Include(x => x.ProjectCage).ThenInclude(x => x!.Cage)
                .Include(x => x.FishBatch)
                .ApplyFilters(request.Filters, request.FilterLogic);

            query = query.ApplySorting(
                string.IsNullOrWhiteSpace(request.SortBy) ? nameof(FishGrowth.GrowthDate) : request.SortBy,
                request.SortDirection);

            var totalCount = await query.CountAsync();
            var entities = await query
                .ApplyPagination(request.PageNumber, request.PageSize)
                .ToListAsync();

            var response = new PagedResponse<FishGrowthDto>
            {
                Items = entities.Select(Map).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResponse<FishGrowthDto>>.SuccessResult(
                response,
                _localizationService.GetLocalizedString("FishGrowthService.Listed"));
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<FishGrowthDto>>.ErrorResult(
                _localizationService.GetLocalizedString("FishGrowthService.ListFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<FishGrowthDto>> CreateAsync(CreateFishGrowthDto dto, long userId)
    {
        try
        {
            await _unitOfWork.BeginTransaction();

            if (dto.GrowthGram <= 0)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.GrowthMustBePositive"));

            var growthDate = dto.GrowthDate.Date;
            var projectCage = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Cage)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == dto.ProjectCageId && x.ProjectId == dto.ProjectId && !x.IsDeleted)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.ProjectCageNotFound"));

            var fishBatch = await _unitOfWork.Db.FishBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.FishBatchId && x.ProjectId == dto.ProjectId && !x.IsDeleted)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.FishBatchNotFound"));

            var balance = await _unitOfWork.Db.BatchCageBalances
                .FirstOrDefaultAsync(x => x.ProjectCageId == dto.ProjectCageId
                    && x.FishBatchId == dto.FishBatchId
                    && !x.IsDeleted)
                ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));

            if (balance.LiveCount <= 0 || balance.AverageGram <= 0)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));

            var alreadyExists = await _unitOfWork.Db.FishGrowths.AnyAsync(x =>
                !x.IsDeleted
                && x.ProjectCageId == dto.ProjectCageId
                && x.FishBatchId == dto.FishBatchId
                && x.GrowthYear == growthDate.Year
                && x.GrowthMonth == growthDate.Month);

            if (alreadyExists)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.MonthlyGrowthAlreadyExists"));

            var previousAverageGram = balance.AverageGram;
            var newAverageGram = BatchMath.CalculateIncrementedAverageGram(previousAverageGram, dto.GrowthGram);
            var previousBiomassGram = balance.BiomassGram;
            var newBiomassGram = BatchMath.CalculateBiomassGram(balance.LiveCount, newAverageGram);
            var growth = new FishGrowth
            {
                ProjectId = dto.ProjectId,
                ProjectCageId = dto.ProjectCageId,
                FishBatchId = dto.FishBatchId,
                GrowthDate = growthDate,
                GrowthYear = growthDate.Year,
                GrowthMonth = (byte)growthDate.Month,
                FishCount = balance.LiveCount,
                PreviousAverageGram = previousAverageGram,
                GrowthGram = dto.GrowthGram,
                NewAverageGram = newAverageGram,
                PreviousBiomassGram = previousBiomassGram,
                NewBiomassGram = newBiomassGram,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CreatedBy = userId,
                IsDeleted = false
            };

            await _unitOfWork.Db.FishGrowths.AddAsync(growth);
            await _unitOfWork.SaveChangesAsync();

            await _balanceLedgerManager.ApplyDelta(
                dto.ProjectId,
                dto.FishBatchId,
                dto.ProjectCageId,
                0,
                newBiomassGram - previousBiomassGram,
                BatchMovementType.FishGrowth,
                growthDate,
                _localizationService.GetLocalizedString("FishGrowthService.Created"),
                ReferenceTable,
                growth.Id,
                dto.ProjectCageId,
                dto.ProjectCageId,
                fishBatch.FishStockId,
                fishBatch.FishStockId,
                previousAverageGram,
                newAverageGram,
                userId);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.Commit();

            growth.ProjectCage = projectCage;
            growth.Project = projectCage.Project;
            growth.FishBatch = fishBatch;
            return ApiResponse<FishGrowthDto>.SuccessResult(
                Map(growth),
                _localizationService.GetLocalizedString("FishGrowthService.Created"));
        }
        catch (DbUpdateException ex) when (DbUpdateExceptionHelper.TryGetUniqueViolation(ex, out _))
        {
            await _unitOfWork.Rollback();
            return ApiResponse<FishGrowthDto>.ErrorResult(
                _localizationService.GetLocalizedString("FishGrowthService.MonthlyGrowthAlreadyExists"),
                _localizationService.GetLocalizedString("FishGrowthService.MonthlyGrowthAlreadyExists"),
                StatusCodes.Status409Conflict);
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<FishGrowthDto>.ErrorResult(
                _localizationService.GetLocalizedString("FishGrowthService.BusinessRuleError"),
                ex.Message,
                StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<FishGrowthDto>.ErrorResult(
                _localizationService.GetLocalizedString("FishGrowthService.CreateFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    private static FishGrowthDto Map(FishGrowth entity) => new()
    {
        Id = entity.Id,
        ProjectId = entity.ProjectId,
        ProjectCode = entity.Project?.ProjectCode,
        ProjectName = entity.Project?.ProjectName,
        ProjectCageId = entity.ProjectCageId,
        CageCode = entity.ProjectCage?.Cage?.CageCode,
        CageName = entity.ProjectCage?.Cage?.CageName,
        FishBatchId = entity.FishBatchId,
        BatchCode = entity.FishBatch?.BatchCode,
        GrowthDate = entity.GrowthDate,
        GrowthYear = entity.GrowthYear,
        GrowthMonth = entity.GrowthMonth,
        FishCount = entity.FishCount,
        PreviousAverageGram = entity.PreviousAverageGram,
        GrowthGram = entity.GrowthGram,
        NewAverageGram = entity.NewAverageGram,
        PreviousBiomassGram = entity.PreviousBiomassGram,
        NewBiomassGram = entity.NewBiomassGram,
        Description = entity.Description
    };
}
