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
                .ApplySearch(request.Search)
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

            var growthDate = dto.GrowthDate.Date;
            var effectiveDate = new DateTime(growthDate.Year, growthDate.Month, 1);
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

            await EnsureShipmentLedgersCompleteAsync(
                dto.FishBatchId,
                effectiveDate.AddMonths(1));

            var hasMovementAfterEffectiveDate = await HasDependentBalanceMovementAsync(
                dto.FishBatchId,
                dto.ProjectCageId,
                effectiveDate);

            if (hasMovementAfterEffectiveDate)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.GrowthMustPrecedeBalanceMovements"));
            }

            var previousAverageGram = balance.AverageGram;
            var (growthGram, newAverageGram) = ResolveGrowthValues(dto, previousAverageGram);
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
                GrowthGram = growthGram,
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
                effectiveDate,
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

    public async Task<ApiResponse<FishGrowthDto>> UpdateAsync(
        long id,
        UpdateFishGrowthDto dto,
        long userId)
    {
        try
        {
            await _unitOfWork.BeginTransaction();

            var growth = await _unitOfWork.Db.FishGrowths
                .Include(x => x.Project)
                .Include(x => x.ProjectCage).ThenInclude(x => x!.Cage)
                .Include(x => x.FishBatch)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException(
                    _localizationService.GetLocalizedString("FishGrowthService.NotFound"));

            var movement = await GetGrowthMovementAsync(growth.Id);
            await EnsureShipmentLedgersCompleteAsync(
                growth.FishBatchId,
                GetEffectiveDate(growth.GrowthDate).AddMonths(1));
            await EnsureNoDependentBalanceMovementAsync(growth, movement.Id);

            var balance = await GetGrowthBalanceAsync(growth);
            EnsureBalanceMatchesGrowth(balance, growth, movement);

            var targetAverageGram = Math.Round(dto.NewAverageGram, 3, MidpointRounding.AwayFromZero);
            if (targetAverageGram <= growth.PreviousAverageGram)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.NewAverageMustExceedPrevious"));
            }

            var targetBiomassGram = BatchMath.CalculateBiomassGram(growth.FishCount, targetAverageGram);
            var growthGram = Math.Round(
                targetAverageGram - growth.PreviousAverageGram,
                3,
                MidpointRounding.AwayFromZero);
            var now = DateTimeProvider.UtcNow;
            var effectiveDate = GetEffectiveDate(growth.GrowthDate);

            balance.AverageGram = targetAverageGram;
            balance.BiomassGram = targetBiomassGram;
            balance.AsOfDate = effectiveDate;
            balance.UpdatedBy = userId;
            balance.UpdatedDate = now;

            growth.GrowthGram = growthGram;
            growth.NewAverageGram = targetAverageGram;
            growth.NewBiomassGram = targetBiomassGram;
            growth.Description = NormalizeDescription(dto.Description);
            growth.UpdatedBy = userId;
            growth.UpdatedDate = now;

            movement.MovementDate = effectiveDate;
            movement.SignedCount = 0;
            movement.SignedBiomassGram = targetBiomassGram - growth.PreviousBiomassGram;
            movement.FromAverageGram = growth.PreviousAverageGram;
            movement.ToAverageGram = targetAverageGram;
            movement.ActorUserId = userId;
            movement.Note = BuildMovementNote(
                growth,
                growth.FishBatch?.FishStockId,
                growth.PreviousAverageGram,
                targetAverageGram,
                _localizationService.GetLocalizedString("FishGrowthService.Updated"));
            movement.UpdatedBy = userId;
            movement.UpdatedDate = now;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.Commit();

            return ApiResponse<FishGrowthDto>.SuccessResult(
                Map(growth),
                _localizationService.GetLocalizedString("FishGrowthService.Updated"));
        }
        catch (KeyNotFoundException ex)
        {
            await _unitOfWork.Rollback();
            return ApiResponse<FishGrowthDto>.ErrorResult(
                ex.Message,
                ex.Message,
                StatusCodes.Status404NotFound);
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
                _localizationService.GetLocalizedString("FishGrowthService.UpdateFailed"),
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
                ?? throw new KeyNotFoundException(
                    _localizationService.GetLocalizedString("FishGrowthService.NotFound"));

            var movement = await GetGrowthMovementAsync(growth.Id);
            await EnsureNoDependentBalanceMovementAsync(growth, movement.Id);

            var balance = await GetGrowthBalanceAsync(growth);
            EnsureBalanceMatchesGrowth(balance, growth, movement);

            var now = DateTimeProvider.UtcNow;
            balance.BiomassGram = growth.PreviousBiomassGram;
            balance.AverageGram = growth.PreviousAverageGram;
            balance.AsOfDate = await GetPreviousMovementDateAsync(growth, movement.Id)
                ?? GetEffectiveDate(growth.GrowthDate);
            balance.UpdatedBy = userId;
            balance.UpdatedDate = now;

            growth.IsDeleted = true;
            growth.DeletedBy = userId;
            growth.DeletedDate = now;

            movement.IsDeleted = true;
            movement.DeletedBy = userId;
            movement.DeletedDate = now;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.Commit();

            return ApiResponse<bool>.SuccessResult(
                true,
                _localizationService.GetLocalizedString("FishGrowthService.Deleted"));
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
                _localizationService.GetLocalizedString("FishGrowthService.DeleteFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<BatchMovement> GetGrowthMovementAsync(long growthId)
    {
        return await _unitOfWork.Db.BatchMovements
            .FirstOrDefaultAsync(x =>
                x.ReferenceTable == ReferenceTable
                && x.ReferenceId == growthId
                && x.MovementType == BatchMovementType.FishGrowth)
            ?? throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.MovementNotFound"));
    }

    private async Task<BatchCageBalance> GetGrowthBalanceAsync(FishGrowth growth)
    {
        return await _unitOfWork.Db.BatchCageBalances
            .FirstOrDefaultAsync(x =>
                x.ProjectCageId == growth.ProjectCageId
                && x.FishBatchId == growth.FishBatchId)
            ?? throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.BalanceNotFound"));
    }

    private async Task EnsureNoDependentBalanceMovementAsync(FishGrowth growth, long growthMovementId)
    {
        var hasDependentMovement = await HasDependentBalanceMovementAsync(
            growth.FishBatchId,
            growth.ProjectCageId,
            GetEffectiveDate(growth.GrowthDate),
            growthMovementId);

        if (hasDependentMovement)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.DependentMovementExists"));
        }
    }

    private Task<bool> HasDependentBalanceMovementAsync(
        long fishBatchId,
        long projectCageId,
        DateTime effectiveDate,
        long? excludedMovementId = null)
    {
        return _unitOfWork.Db.BatchMovements.AnyAsync(x =>
            (!excludedMovementId.HasValue || x.Id != excludedMovementId.Value)
            && x.FishBatchId == fishBatchId
            && (x.ProjectCageId == projectCageId
                || x.FromProjectCageId == projectCageId
                || x.ToProjectCageId == projectCageId)
            && x.MovementDate >= effectiveDate
            && x.MovementType != BatchMovementType.OpeningImport
            && x.MovementType != BatchMovementType.Stocking
            && (x.SignedCount != 0 || x.SignedBiomassGram != 0));
    }

    private async Task EnsureShipmentLedgersCompleteAsync(
        long fishBatchId,
        DateTime periodEndExclusive)
    {
        var documentRows = await _unitOfWork.Db.ShipmentLines
            .Where(x =>
                x.FishBatchId == fishBatchId
                && x.Shipment != null
                && x.Shipment.Status == DocumentStatus.Posted
                && x.Shipment.ShipmentDate < periodEndExclusive)
            .Select(x => new
            {
                FishCount = (long)x.FishCount,
                x.BiomassGram
            })
            .ToListAsync();
        var documentTotals = new ShipmentLedgerTotals
        {
            FishCount = documentRows.Sum(x => x.FishCount),
            BiomassGram = documentRows.Sum(x => x.BiomassGram)
        };

        if (documentTotals.FishCount == 0 && documentTotals.BiomassGram == 0m)
        {
            return;
        }

        var movementRows = await _unitOfWork.Db.BatchMovements
            .Where(x =>
                x.FishBatchId == fishBatchId
                && x.ProjectCageId != null
                && x.MovementType == BatchMovementType.Shipment
                && x.MovementDate < periodEndExclusive
                && (x.SignedCount != 0 || x.SignedBiomassGram != 0m))
            .Select(x => new
            {
                x.SignedCount,
                x.SignedBiomassGram
            })
            .ToListAsync();
        var movementTotals = new ShipmentLedgerTotals
        {
            FishCount = Math.Max(0L, -movementRows.Sum(x => (long)x.SignedCount)),
            BiomassGram = Math.Max(0m, -movementRows.Sum(x => x.SignedBiomassGram))
        };

        if (documentTotals.FishCount > movementTotals.FishCount
            || documentTotals.BiomassGram > movementTotals.BiomassGram)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.UnrepresentedShipmentExists"));
        }
    }

    private async Task<DateTime?> GetPreviousMovementDateAsync(FishGrowth growth, long growthMovementId)
    {
        var effectiveDate = GetEffectiveDate(growth.GrowthDate);
        return await _unitOfWork.Db.BatchMovements
            .Where(x =>
                x.Id != growthMovementId
                && x.FishBatchId == growth.FishBatchId
                && (x.ProjectCageId == growth.ProjectCageId
                    || x.FromProjectCageId == growth.ProjectCageId
                    || x.ToProjectCageId == growth.ProjectCageId)
                && x.MovementDate <= effectiveDate)
            .OrderByDescending(x => x.MovementDate)
            .ThenByDescending(x => x.Id)
            .Select(x => (DateTime?)x.MovementDate)
            .FirstOrDefaultAsync();
    }

    private void EnsureBalanceMatchesGrowth(
        BatchCageBalance balance,
        FishGrowth growth,
        BatchMovement movement)
    {
        var expectedGrowthBiomassGram = growth.NewBiomassGram - growth.PreviousBiomassGram;
        var movementMatches = movement.SignedCount == 0
            && AreEqual(movement.SignedBiomassGram, expectedGrowthBiomassGram)
            && movement.FromAverageGram.HasValue
            && AreEqual(movement.FromAverageGram.Value, growth.PreviousAverageGram)
            && movement.ToAverageGram.HasValue
            && AreEqual(movement.ToAverageGram.Value, growth.NewAverageGram);
        var balanceMatches = balance.LiveCount == growth.FishCount
            && AreEqual(balance.AverageGram, growth.NewAverageGram)
            && AreEqual(balance.BiomassGram, growth.NewBiomassGram);

        if (!movementMatches || !balanceMatches || expectedGrowthBiomassGram <= 0m)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.BalanceMismatch"));
        }
    }

    private static bool AreEqual(decimal left, decimal right) => left == right;

    private static DateTime GetEffectiveDate(DateTime growthDate) =>
        new(growthDate.Year, growthDate.Month, 1);

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static string BuildMovementNote(
        FishGrowth growth,
        long? fishStockId,
        decimal fromAverageGram,
        decimal toAverageGram,
        string description)
    {
        return string.Join(" | ", new[]
        {
            description,
            $"projectId={growth.ProjectId}",
            $"fromCage={growth.ProjectCageId}",
            $"toCage={growth.ProjectCageId}",
            $"fromStock={fishStockId?.ToString() ?? "null"}",
            $"toStock={fishStockId?.ToString() ?? "null"}",
            $"fromAvg={fromAverageGram:0.###}",
            $"toAvg={toAverageGram:0.###}"
        });
    }

    private sealed class ShipmentLedgerTotals
    {
        public long FishCount { get; init; }
        public decimal BiomassGram { get; init; }
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
        GrowthRatePercent = entity.PreviousAverageGram > 0
            ? Math.Round(entity.GrowthGram / entity.PreviousAverageGram * 100m, 4, MidpointRounding.AwayFromZero)
            : 0m,
        NewAverageGram = entity.NewAverageGram,
        PreviousBiomassGram = entity.PreviousBiomassGram,
        NewBiomassGram = entity.NewBiomassGram,
        Description = entity.Description
    };

    private (decimal GrowthGram, decimal NewAverageGram) ResolveGrowthValues(
        CreateFishGrowthDto dto,
        decimal previousAverageGram)
    {
        if (dto.NewAverageGram.HasValue)
        {
            var targetAverageGram = Math.Round(dto.NewAverageGram.Value, 3, MidpointRounding.AwayFromZero);
            if (targetAverageGram <= previousAverageGram)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.NewAverageMustExceedCurrent"));
            }

            return (
                Math.Round(targetAverageGram - previousAverageGram, 3, MidpointRounding.AwayFromZero),
                targetAverageGram);
        }

        if (!dto.GrowthGram.HasValue || dto.GrowthGram.Value <= 0)
        {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("FishGrowthService.GrowthMustBePositive"));
        }

        var legacyGrowthGram = Math.Round(dto.GrowthGram.Value, 3, MidpointRounding.AwayFromZero);
        return (
            legacyGrowthGram,
            BatchMath.CalculateIncrementedAverageGram(previousAverageGram, legacyGrowthGram));
    }
}
