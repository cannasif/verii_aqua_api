using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.FishGrowths.Application.Services;

public class FishGrowthService : IFishGrowthService
{
    private const string ReferenceTable = "RII_FISH_GROWTH";
    private const int TimelineMonthLimit = 120;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFishGrowthLedgerReplayService _ledgerReplayService;
    private readonly ILocalizationService _localizationService;

    public FishGrowthService(
        IUnitOfWork unitOfWork,
        IFishGrowthLedgerReplayService ledgerReplayService,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _ledgerReplayService = ledgerReplayService;
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
                .ApplySearch(request)
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

            var alreadyExists = await _unitOfWork.Db.FishGrowths.AnyAsync(x =>
                !x.IsDeleted
                && x.ProjectCageId == dto.ProjectCageId
                && x.FishBatchId == dto.FishBatchId
                && x.GrowthYear == growthDate.Year
                && x.GrowthMonth == growthDate.Month);

            if (alreadyExists)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FishGrowthService.MonthlyGrowthAlreadyExists"));

            await _ledgerReplayService.PrepareAsync(
                dto.ProjectId,
                dto.FishBatchId,
                dto.ProjectCageId,
                userId);
            var state = await _ledgerReplayService.GetStateBeforeAsync(
                dto.FishBatchId,
                dto.ProjectCageId,
                effectiveDate);

            var previousAverageGram = state.AverageGram;
            var (growthGram, newAverageGram) = ResolveGrowthValues(dto, previousAverageGram);
            var previousBiomassGram = state.BiomassGram;
            var newBiomassGram = BatchMath.CalculateBiomassGram(state.FishCount, newAverageGram);
            var growth = new FishGrowth
            {
                ProjectId = dto.ProjectId,
                ProjectCageId = dto.ProjectCageId,
                FishBatchId = dto.FishBatchId,
                GrowthDate = growthDate,
                GrowthYear = growthDate.Year,
                GrowthMonth = (byte)growthDate.Month,
                FishCount = state.FishCount,
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

            await _unitOfWork.Db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = dto.FishBatchId,
                ProjectCageId = dto.ProjectCageId,
                FromProjectCageId = dto.ProjectCageId,
                ToProjectCageId = dto.ProjectCageId,
                FromStockId = fishBatch.FishStockId,
                ToStockId = fishBatch.FishStockId,
                FromAverageGram = previousAverageGram,
                ToAverageGram = newAverageGram,
                MovementDate = effectiveDate,
                MovementType = BatchMovementType.FishGrowth,
                SignedCount = 0,
                SignedBiomassGram = newBiomassGram - previousBiomassGram,
                ActorUserId = userId,
                ReferenceTable = ReferenceTable,
                ReferenceId = growth.Id,
                Note = BuildMovementNote(
                    growth,
                    fishBatch.FishStockId,
                    previousAverageGram,
                    newAverageGram,
                    _localizationService.GetLocalizedString("FishGrowthService.Created")),
                CreatedBy = userId,
                IsDeleted = false
            });

            await _unitOfWork.SaveChangesAsync();
            await _ledgerReplayService.ReplayAsync(
                dto.ProjectId,
                dto.FishBatchId,
                dto.ProjectCageId,
                effectiveDate,
                userId);
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

            await _ledgerReplayService.PrepareAsync(
                growth.ProjectId,
                growth.FishBatchId,
                growth.ProjectCageId,
                userId);

            var targetAverageGram = Math.Round(dto.NewAverageGram, 3, MidpointRounding.AwayFromZero);
            if (targetAverageGram <= 0m)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("FishGrowthService.NewAverageMustExceedPrevious"));
            }

            var now = DateTimeProvider.UtcNow;
            var effectiveDate = GetEffectiveDate(growth.GrowthDate);

            growth.NewAverageGram = targetAverageGram;
            growth.Description = NormalizeDescription(dto.Description);
            growth.UpdatedBy = userId;
            growth.UpdatedDate = now;

            await _ledgerReplayService.ReplayAsync(
                growth.ProjectId,
                growth.FishBatchId,
                growth.ProjectCageId,
                effectiveDate,
                userId);
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
                !x.IsDeleted &&
                x.ProjectCageId == projectCageId &&
                x.FishBatchId == fishBatchId &&
                x.GrowthYear == year &&
                x.GrowthMonth == month);

        return ApiResponse<FishGrowthDto?>.SuccessResult(
            entity == null ? null : Map(entity),
            entity == null ? "Büyütme kaydı bulunamadı." : "Büyütme kaydı getirildi.");
    }

    public async Task<ApiResponse<FishGrowthTimelineDto>> GetTimelineAsync(
        long projectCageId,
        long fishBatchId,
        int throughYear,
        int throughMonth)
    {
        if (projectCageId <= 0 || fishBatchId <= 0 || throughYear is < 2000 or > 2100 || throughMonth is < 1 or > 12)
        {
            const string message = "Büyütme zaman çizelgesi dönemi veya kafes/balık partisi bilgisi geçersiz.";
            return ApiResponse<FishGrowthTimelineDto>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
        }

        var selectedPeriod = new DateTime(throughYear, throughMonth, 1);
        var periodEndExclusive = selectedPeriod.AddMonths(1);
        var projectCage = await _unitOfWork.Db.ProjectCages
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == projectCageId)
            .Select(x => new { x.Id, x.ProjectId, x.AssignedDate })
            .FirstOrDefaultAsync();
        var fishBatch = projectCage == null
            ? null
            : await _unitOfWork.Db.FishBatches
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Id == fishBatchId && x.ProjectId == projectCage.ProjectId)
                .Select(x => new { x.Id, x.StartDate, x.CurrentAverageGram })
                .FirstOrDefaultAsync();

        if (projectCage == null || fishBatch == null)
        {
            const string message = "Kafes veya balık partisi bulunamadı.";
            return ApiResponse<FishGrowthTimelineDto>.ErrorResult(message, message, StatusCodes.Status404NotFound);
        }

        var growths = await _unitOfWork.Db.FishGrowths
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.ProjectCageId == projectCageId
                && x.FishBatchId == fishBatchId
                && x.GrowthDate < periodEndExclusive)
            .OrderBy(x => x.GrowthYear)
            .ThenBy(x => x.GrowthMonth)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var cageMovements = await _unitOfWork.Db.BatchMovements
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.FishBatchId == fishBatchId
                && x.MovementDate < periodEndExclusive
                && x.ProjectCageId == projectCageId
                && (x.SignedCount != 0 || x.SignedBiomassGram != 0m))
            .OrderBy(x => x.MovementDate)
            .ThenBy(x => x.Id)
            .ToListAsync();
        var entryMovement = cageMovements.FirstOrDefault(x => x.SignedCount > 0);

        var balanceSnapshot = await _unitOfWork.Db.BatchCageBalances
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId)
            .Select(x => new { x.AverageGram, x.LiveCount })
            .FirstOrDefaultAsync();

        var naturalStartDate = entryMovement?.MovementDate.Date
            ?? (projectCage.AssignedDate.Date >= fishBatch.StartDate.Date
                ? projectCage.AssignedDate.Date
                : fishBatch.StartDate.Date);
        var naturalStartPeriod = GetEffectiveDate(naturalStartDate);
        var earliestGrowthPeriod = growths.Count == 0
            ? (DateTime?)null
            : new DateTime(growths[0].GrowthYear, growths[0].GrowthMonth, 1);
        if (earliestGrowthPeriod.HasValue && earliestGrowthPeriod.Value < naturalStartPeriod)
        {
            naturalStartPeriod = earliestGrowthPeriod.Value;
        }

        if (naturalStartPeriod > selectedPeriod)
        {
            naturalStartPeriod = selectedPeriod;
        }

        var earliestAllowedPeriod = selectedPeriod.AddMonths(-(TimelineMonthLimit - 1));
        var wasTruncated = naturalStartPeriod < earliestAllowedPeriod;
        var startPeriod = wasTruncated ? earliestAllowedPeriod : naturalStartPeriod;
        var lastGrowthBeforeStart = growths
            .LastOrDefault(x => new DateTime(x.GrowthYear, x.GrowthMonth, 1) < startPeriod);
        growths = growths
            .Where(x => new DateTime(x.GrowthYear, x.GrowthMonth, 1) >= startPeriod)
            .ToList();

        var firstGrowth = growths.FirstOrDefault();
        var movementsBeforeStart = cageMovements
            .Where(x => x.MovementDate < startPeriod)
            .ToList();
        var runningFishCount = movementsBeforeStart.Sum(x => (long)x.SignedCount);
        var runningBiomassGram = movementsBeforeStart.Sum(x => x.SignedBiomassGram);
        var entryAverageGram = entryMovement == null
            ? (decimal?)null
            : CalculateMovementAverageGram(entryMovement);
        var fallbackInitialAverageGram = lastGrowthBeforeStart?.NewAverageGram
            ?? entryAverageGram
            ?? firstGrowth?.PreviousAverageGram
            ?? balanceSnapshot?.AverageGram
            ?? fishBatch.CurrentAverageGram;
        var initialAverageGram = CalculateLedgerAverageGram(
            runningFishCount,
            runningBiomassGram,
            fallbackInitialAverageGram);

        var growthByPeriod = growths
            .GroupBy(x => new DateTime(x.GrowthYear, x.GrowthMonth, 1))
            .ToDictionary(x => x.Key, x => x.OrderByDescending(growth => growth.Id).First());
        var currentPeriod = GetEffectiveDate(DateTimeProvider.Now);
        var runningAverageGram = initialAverageGram;
        var movementsByPeriod = cageMovements
            .Where(x => x.MovementDate >= startPeriod)
            .GroupBy(x => GetEffectiveDate(x.MovementDate))
            .ToDictionary(x => x.Key, x => x.OrderBy(movement => movement.MovementDate).ThenBy(movement => movement.Id).ToList());
        var hasLedger = cageMovements.Count > 0;
        if (!hasLedger)
        {
            runningFishCount = balanceSnapshot?.LiveCount ?? 0;
        }

        var lastSourcePeriod = lastGrowthBeforeStart == null
            ? startPeriod
            : new DateTime(lastGrowthBeforeStart.GrowthYear, lastGrowthBeforeStart.GrowthMonth, 1);
        var rows = new List<FishGrowthTimelineMonthDto>();

        for (var period = startPeriod; period <= selectedPeriod; period = period.AddMonths(1))
        {
            var periodStartAverageGram = CalculateLedgerAverageGram(
                runningFishCount,
                runningBiomassGram,
                runningAverageGram);
            var periodMovements = movementsByPeriod.GetValueOrDefault(period) ?? new List<BatchMovement>();
            foreach (var movement in periodMovements)
            {
                runningFishCount += movement.SignedCount;
                runningBiomassGram += movement.SignedBiomassGram;
            }

            var periodEndAverageGram = CalculateLedgerAverageGram(
                runningFishCount,
                runningBiomassGram,
                periodStartAverageGram);

            if (growthByPeriod.TryGetValue(period, out var growth))
            {
                var growthMovement = cageMovements.FirstOrDefault(x =>
                    x.MovementType == BatchMovementType.FishGrowth
                    && x.ReferenceTable == ReferenceTable
                    && x.ReferenceId == growth.Id);
                var ledgerAverageBeforeGrowth = growthMovement == null
                    ? null
                    : CalculateLedgerAverageBeforeMovement(cageMovements, growthMovement.Id);
                var expectedPreviousAverageGram = ledgerAverageBeforeGrowth ?? growth.PreviousAverageGram;
                var hasContinuityIssue = HasGrowthIntegrityIssue(growth, growthMovement)
                    || (ledgerAverageBeforeGrowth.HasValue
                        && Math.Abs(growth.PreviousAverageGram - ledgerAverageBeforeGrowth.Value) > 0.001m);
                var endAverageGram = runningFishCount > 0 && runningBiomassGram > 0m
                    ? periodEndAverageGram
                    : growth.NewAverageGram;
                rows.Add(new FishGrowthTimelineMonthDto
                {
                    Period = period,
                    Year = period.Year,
                    Month = (byte)period.Month,
                    Status = "Recorded",
                    GrowthId = growth.Id,
                    PreviousAverageGram = growth.PreviousAverageGram,
                    ExpectedPreviousAverageGram = expectedPreviousAverageGram,
                    GrowthGram = growth.GrowthGram,
                    EndAverageGram = endAverageGram,
                    OperationalAverageChangeGram = Math.Round(
                        endAverageGram - growth.NewAverageGram,
                        3,
                        MidpointRounding.AwayFromZero),
                    GrowthRatePercent = growth.PreviousAverageGram > 0m
                        ? Math.Round(growth.GrowthGram / growth.PreviousAverageGram * 100m, 4, MidpointRounding.AwayFromZero)
                        : 0m,
                    FishCount = (int)Math.Clamp(runningFishCount, 0L, int.MaxValue),
                    HasContinuityIssue = hasContinuityIssue,
                    IsSelectedPeriod = period == selectedPeriod,
                    Description = growth.Description
                });
                runningAverageGram = endAverageGram;
                lastSourcePeriod = period;
                continue;
            }

            var operationalAverageChangeGram = Math.Round(
                periodEndAverageGram - periodStartAverageGram,
                3,
                MidpointRounding.AwayFromZero);
            var status = period == startPeriod
                ? "Baseline"
                : period >= currentPeriod
                    ? "Pending"
                    : Math.Abs(operationalAverageChangeGram) > 0.001m
                        ? "OperationallyAdjusted"
                        : "CarriedForward";
            rows.Add(new FishGrowthTimelineMonthDto
            {
                Period = period,
                Year = period.Year,
                Month = (byte)period.Month,
                Status = status,
                PreviousAverageGram = period == startPeriod && runningFishCount > 0
                    ? periodEndAverageGram
                    : periodStartAverageGram,
                ExpectedPreviousAverageGram = periodStartAverageGram,
                EndAverageGram = periodEndAverageGram,
                OperationalAverageChangeGram = period == startPeriod ? 0m : operationalAverageChangeGram,
                FishCount = (int)Math.Clamp(runningFishCount, 0L, int.MaxValue),
                CarriedFromPeriod = period == startPeriod ? null : lastSourcePeriod,
                IsSelectedPeriod = period == selectedPeriod
            });
            runningAverageGram = periodEndAverageGram;
        }

        var response = new FishGrowthTimelineDto
        {
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            StartPeriod = startPeriod,
            EndPeriod = selectedPeriod,
            InitialAverageGram = initialAverageGram,
            LatestAverageGram = rows.LastOrDefault()?.EndAverageGram ?? runningAverageGram,
            RecordedMonthCount = rows.Count(x => x.Status == "Recorded"),
            CarriedForwardMonthCount = rows.Count(x =>
                x.Status == "CarriedForward" || x.Status == "OperationallyAdjusted"),
            HasContinuityIssue = rows.Any(x => x.HasContinuityIssue),
            WasTruncated = wasTruncated,
            Months = rows
        };

        return ApiResponse<FishGrowthTimelineDto>.SuccessResult(
            response,
            _localizationService.GetLocalizedString("FishGrowthService.Listed"));
    }

    private static decimal CalculateMovementAverageGram(BatchMovement movement)
    {
        if (movement.SignedCount > 0 && movement.SignedBiomassGram > 0m)
        {
            return Math.Round(
                movement.SignedBiomassGram / movement.SignedCount,
                3,
                MidpointRounding.AwayFromZero);
        }

        return Math.Round(
            Math.Max(0m, movement.ToAverageGram ?? movement.FromAverageGram ?? 0m),
            3,
            MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateLedgerAverageGram(
        long fishCount,
        decimal biomassGram,
        decimal fallbackAverageGram)
    {
        if (fishCount > 0 && biomassGram > 0m)
        {
            return Math.Round(
                biomassGram / fishCount,
                3,
                MidpointRounding.AwayFromZero);
        }

        return Math.Round(
            Math.Max(0m, fallbackAverageGram),
            3,
            MidpointRounding.AwayFromZero);
    }

    private static decimal? CalculateLedgerAverageBeforeMovement(
        IEnumerable<BatchMovement> movements,
        long targetMovementId)
    {
        long fishCount = 0;
        decimal biomassGram = 0m;
        var targetFound = false;

        foreach (var movement in movements.OrderBy(x => x.CreatedDate).ThenBy(x => x.Id))
        {
            if (movement.Id == targetMovementId)
            {
                targetFound = true;
                break;
            }

            fishCount += movement.SignedCount;
            biomassGram += movement.SignedBiomassGram;
        }

        return targetFound && fishCount > 0 && biomassGram > 0m
            ? CalculateLedgerAverageGram(fishCount, biomassGram, 0m)
            : null;
    }

    private static bool HasGrowthIntegrityIssue(FishGrowth growth, BatchMovement? movement)
    {
        if (growth.FishCount <= 0
            || Math.Abs(growth.NewAverageGram - (growth.PreviousAverageGram + growth.GrowthGram)) > 0.001m
            || movement == null)
        {
            return true;
        }

        var expectedBiomassDelta = growth.NewBiomassGram - growth.PreviousBiomassGram;
        return movement.SignedCount != 0
            || Math.Abs(movement.SignedBiomassGram - expectedBiomassDelta) > 0.001m
            || !movement.FromAverageGram.HasValue
            || Math.Abs(movement.FromAverageGram.Value - growth.PreviousAverageGram) > 0.001m
            || !movement.ToAverageGram.HasValue
            || Math.Abs(movement.ToAverageGram.Value - growth.NewAverageGram) > 0.001m;
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

            await _ledgerReplayService.PrepareAsync(
                growth.ProjectId,
                growth.FishBatchId,
                growth.ProjectCageId,
                userId);
            var movement = await GetGrowthMovementAsync(growth.Id);

            var now = DateTimeProvider.UtcNow;
            var effectiveDate = GetEffectiveDate(growth.GrowthDate);

            growth.IsDeleted = true;
            growth.DeletedBy = userId;
            growth.DeletedDate = now;

            movement.IsDeleted = true;
            movement.DeletedBy = userId;
            movement.DeletedDate = now;

            await _unitOfWork.SaveChangesAsync();
            await _ledgerReplayService.ReplayAsync(
                growth.ProjectId,
                growth.FishBatchId,
                growth.ProjectCageId,
                effectiveDate,
                userId);
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
