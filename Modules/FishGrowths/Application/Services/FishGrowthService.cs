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
                ex.Message,
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

    public async Task<ApiResponse<FishGrowthDto?>> GetMonthlyAsync(
        long projectCageId,
        long fishBatchId,
        int year,
        int month)
    {
        if (projectCageId <= 0 || fishBatchId <= 0 || year is < 2000 or > 2100 || month is < 1 or > 12)
        {
            const string message = "Büyütme dönemi veya kafes/balık partisi bilgisi geçersiz.";
            return ApiResponse<FishGrowthDto?>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
        }

        var entity = await _unitOfWork.Db.FishGrowths
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.ProjectCage).ThenInclude(x => x!.Cage)
            .Include(x => x.FishBatch)
            .FirstOrDefaultAsync(x =>
                x.ProjectCageId == projectCageId &&
                x.FishBatchId == fishBatchId &&
                x.GrowthYear == year &&
                x.GrowthMonth == month);

        return ApiResponse<FishGrowthDto?>.SuccessResult(
            entity == null ? null : Map(entity),
            entity == null ? "Büyütme kaydı bulunamadı." : "Büyütme kaydı getirildi.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id, long userId)
    {
        try
        {
            await _unitOfWork.BeginTransaction();

            var growth = await _unitOfWork.Db.FishGrowths
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Silinecek balık büyütme kaydı bulunamadı.");

            var movement = await _unitOfWork.Db.BatchMovements
                .FirstOrDefaultAsync(x =>
                    x.ReferenceTable == ReferenceTable &&
                    x.ReferenceId == growth.Id &&
                    x.MovementType == BatchMovementType.FishGrowth)
                ?? throw new InvalidOperationException(
                    "Büyütmeye bağlı stok hareketi bulunamadığı için kayıt güvenli şekilde geri alınamadı.");

            var hasLaterBalanceMovement = await _unitOfWork.Db.BatchMovements.AnyAsync(x =>
                x.FishBatchId == growth.FishBatchId &&
                x.ProjectCageId == growth.ProjectCageId &&
                x.Id > movement.Id &&
                (x.SignedCount != 0 || x.SignedBiomassGram != 0));
            if (hasLaterBalanceMovement)
            {
                throw new InvalidOperationException(
                    "Bu büyütmeden sonra satış, fire, transfer veya başka bir bakiye hareketi bulunmaktadır. Sonraki hareketler geri alınmadan büyütme silinemez.");
            }

            var balance = await _unitOfWork.Db.BatchCageBalances
                .FirstOrDefaultAsync(x =>
                    x.ProjectCageId == growth.ProjectCageId &&
                    x.FishBatchId == growth.FishBatchId)
                ?? throw new InvalidOperationException(
                    "Büyütmeye ait aktif kafes bakiyesi bulunamadığı için kayıt geri alınamadı.");

            var growthBiomassGram = movement.SignedBiomassGram;
            if (growthBiomassGram <= 0m ||
                balance.LiveCount != growth.FishCount ||
                balance.BiomassGram < growthBiomassGram)
            {
                throw new InvalidOperationException(
                    "Kafes bakiyesi büyütme kaydıyla uyuşmuyor. Veri kaybını önlemek için silme işlemi durduruldu.");
            }

            balance.BiomassGram -= growthBiomassGram;
            balance.AverageGram = balance.LiveCount > 0
                ? Math.Round(balance.BiomassGram / balance.LiveCount, 3, MidpointRounding.AwayFromZero)
                : 0m;
            balance.UpdatedBy = userId;
            balance.UpdatedDate = DateTimeProvider.UtcNow;

            growth.IsDeleted = true;
            growth.DeletedBy = userId;
            growth.DeletedDate = DateTimeProvider.UtcNow;

            movement.IsDeleted = true;
            movement.DeletedBy = userId;
            movement.DeletedDate = DateTimeProvider.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.Commit();

            return ApiResponse<bool>.SuccessResult(
                true,
                "Balık büyütme kaydı geri alındı. Aynı ay için yeniden büyütme girebilirsiniz.");
        }
        catch (KeyNotFoundException ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<bool>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<bool>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<bool>.ErrorResult(
                "Balık büyütme kaydı geri alınamadı.",
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
