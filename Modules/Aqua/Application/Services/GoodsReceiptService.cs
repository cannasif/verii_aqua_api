using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly AquaDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public GoodsReceiptService(AquaDbContext db, IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<GoodsReceiptDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.GoodsReceipts
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<GoodsReceiptDto>(entity);
                return ApiResponse<GoodsReceiptDto>.SuccessResult(dto, _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<GoodsReceiptDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.GoodsReceipts
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(GoodsReceipt.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<GoodsReceiptDto>(x)).ToList();

                var pagedResponse = new PagedResponse<GoodsReceiptDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<GoodsReceiptDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<GoodsReceiptDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto dto)
        {
            try
            {
                var normalizedReceiptNo = dto.ReceiptNo?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedReceiptNo))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ReceiptNumberRequired"));
                }

                var duplicateReceiptNo = await _unitOfWork.GoodsReceipts
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.ReceiptNo == normalizedReceiptNo);

                if (duplicateReceiptNo)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.DuplicateReceiptNumber"));
                }

                if (dto.ProjectId.HasValue)
                {
                    var projectExists = await _unitOfWork.Projects
                        .Query()
                        .AnyAsync(x => !x.IsDeleted && x.Id == dto.ProjectId.Value);

                    if (!projectExists)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ProjectNotFound"));
                    }

                    var existsForProject = await _unitOfWork.GoodsReceipts
                        .Query()
                        .AnyAsync(x => !x.IsDeleted && x.ProjectId == dto.ProjectId.Value);

                    if (existsForProject)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ProjectGoodsReceiptAlreadyExists"));
                    }
                }

                dto.ReceiptNo = normalizedReceiptNo;
                var entity = _mapper.Map<GoodsReceipt>(dto);
                await _unitOfWork.GoodsReceipts.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptDto>(entity);
                return ApiResponse<GoodsReceiptDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex) when (IsKnownGoodsReceiptConstraintError(ex, out var message))
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    message,
                    message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<GoodsReceiptDto>> UpdateAsync(long id, UpdateGoodsReceiptDto dto)
        {
            try
            {
                var repo = _unitOfWork.GoodsReceipts;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var normalizedReceiptNo = dto.ReceiptNo?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedReceiptNo))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ReceiptNumberRequired"));
                }

                var duplicateReceiptNo = await _unitOfWork.GoodsReceipts
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.Id != id && x.ReceiptNo == normalizedReceiptNo);

                if (duplicateReceiptNo)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.DuplicateReceiptNumber"));
                }

                if (dto.ProjectId.HasValue)
                {
                    var projectExists = await _unitOfWork.Projects
                        .Query()
                        .AnyAsync(x => !x.IsDeleted && x.Id == dto.ProjectId.Value);

                    if (!projectExists)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ProjectNotFound"));
                    }

                    var existsForProject = await _unitOfWork.GoodsReceipts
                        .Query()
                        .AnyAsync(x => !x.IsDeleted && x.Id != id && x.ProjectId == dto.ProjectId.Value);

                    if (existsForProject)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ProjectGoodsReceiptAlreadyExists"));
                    }
                }

                dto.ReceiptNo = normalizedReceiptNo;
                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptDto>(entity);
                return ApiResponse<GoodsReceiptDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex) when (IsKnownGoodsReceiptConstraintError(ex, out var message))
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    message,
                    message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.GoodsReceipts;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> PostAsync(long goodsReceiptId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var receipt = await _db.GoodsReceipts
                    .Include(x => x.Lines)
                    .ThenInclude(x => x.FishDistributions)
                    .FirstOrDefaultAsync(x => x.Id == goodsReceiptId && !x.IsDeleted)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.GoodsReceiptNotFound"));

                EnsureDraftStatus(receipt.Status, nameof(GoodsReceipt));
                if (!receipt.Lines.Any())
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.MustContainAtLeastOneLine"));

                var hasFish = receipt.Lines.Any(x => x.ItemType == GoodsReceiptItemType.Fish);
                if (hasFish && !receipt.ProjectId.HasValue)
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.ProjectRequiredForFishLines"));

                foreach (var line in receipt.Lines.Where(x => !x.IsDeleted))
                {
                    if (line.ItemType == GoodsReceiptItemType.Feed)
                    {
                        if (line.QtyUnit is null || line.GramPerUnit is null || line.TotalGram is null)
                            throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.FeedLineValuesRequired"));
                        continue;
                    }

                    if (line.FishCount is null || line.FishAverageGram is null || line.FishTotalGram is null)
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.FishLineValuesRequired"));

                    if (!line.FishDistributions.Any(x => !x.IsDeleted))
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.FishLineDistributionRequired"));

                    var distributedCount = line.FishDistributions.Where(x => !x.IsDeleted).Sum(x => x.FishCount);
                    if (distributedCount != line.FishCount.Value)
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.FishDistributionTotalMustEqualFishCount"));

                    var fishBatchId = line.FishBatchId;
                    if (!fishBatchId.HasValue)
                    {
                        var batch = new FishBatch
                        {
                            ProjectId = receipt.ProjectId!.Value,
                            BatchCode = receipt.ReceiptNo,
                            FishStockId = line.StockId,
                            CurrentAverageGram = line.FishAverageGram.Value,
                            StartDate = receipt.ReceiptDate,
                            SourceGoodsReceiptLineId = line.Id,
                            CreatedBy = userId,
                            CreatedDate = DateTimeProvider.Now,
                            IsDeleted = false
                        };

                        await _db.FishBatches.AddAsync(batch);
                        await _unitOfWork.SaveChanges();
                        fishBatchId = batch.Id;
                        line.FishBatchId = batch.Id;
                    }

                    foreach (var distribution in line.FishDistributions.Where(x => !x.IsDeleted))
                    {
                        var biomass = Math.Round(distribution.FishCount * line.FishAverageGram.Value, 3, MidpointRounding.AwayFromZero);
                        await ApplyBalanceAndLedgerAsync(
                            fishBatchId.Value,
                            distribution.ProjectCageId,
                            distribution.FishCount,
                            biomass,
                            BatchMovementType.Stocking,
                            receipt.ReceiptDate,
                            nameof(GoodsReceipt),
                            receipt.Id,
                            userId);
                    }
                }

                receipt.Status = DocumentStatus.Posted;
                receipt.UpdatedBy = userId;
                receipt.UpdatedDate = DateTimeProvider.Now;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("GoodsReceiptService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private bool IsKnownGoodsReceiptConstraintError(DbUpdateException ex, out string message)
        {
            var allMessages = ex.ToString();

            if (allMessages.Contains("UX_RII_GoodsReceipt_ReceiptNo_Active", StringComparison.OrdinalIgnoreCase))
            {
                message = _localizationService.GetLocalizedString("GoodsReceiptService.DuplicateReceiptNumber");
                return true;
            }

            if (allMessages.Contains("UX_RII_GoodsReceipt_Project_Active", StringComparison.OrdinalIgnoreCase))
            {
                message = _localizationService.GetLocalizedString("GoodsReceiptService.ProjectGoodsReceiptAlreadyExists");
                return true;
            }

            if (allMessages.Contains("FK_RII_GoodsReceipt_RII_Project_ProjectId", StringComparison.OrdinalIgnoreCase))
            {
                message = _localizationService.GetLocalizedString("GoodsReceiptService.ProjectNotFound");
                return true;
            }

            message = string.Empty;
            return false;
        }

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
        }

        private async Task<BatchCageBalance> GetOrCreateBalanceAsync(long fishBatchId, long projectCageId, long userId, DateTime asOfDate)
        {
            var balance = await _db.BatchCageBalances
                .FirstOrDefaultAsync(x => x.FishBatchId == fishBatchId && x.ProjectCageId == projectCageId && !x.IsDeleted);

            if (balance != null)
                return balance;

            balance = new BatchCageBalance
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                LiveCount = 0,
                AverageGram = 0,
                BiomassGram = 0,
                AsOfDate = asOfDate,
                CreatedBy = userId,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _db.BatchCageBalances.AddAsync(balance);
            return balance;
        }

        private async Task ApplyBalanceAndLedgerAsync(long fishBatchId, long? projectCageId, int signedCount, decimal signedBiomassGram, BatchMovementType movementType, DateTime movementDate, string referenceTable, long referenceId, long userId)
        {
            if (projectCageId.HasValue)
            {
                var balance = await GetOrCreateBalanceAsync(fishBatchId, projectCageId.Value, userId, movementDate);

                var nextCount = balance.LiveCount + signedCount;
                var nextBiomass = balance.BiomassGram + signedBiomassGram;
                if (nextCount < 0 || nextBiomass < 0)
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("GoodsReceiptService.BatchCageBalanceCannotGoNegative"));

                balance.LiveCount = nextCount;
                balance.BiomassGram = nextBiomass;
                balance.AverageGram = nextCount == 0 ? 0 : Math.Round(nextBiomass / nextCount, 3, MidpointRounding.AwayFromZero);
                balance.AsOfDate = movementDate;
                balance.UpdatedBy = userId;
                balance.UpdatedDate = DateTimeProvider.Now;
            }

            await _db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = fishBatchId,
                ProjectCageId = projectCageId,
                MovementDate = movementDate,
                MovementType = movementType,
                SignedCount = signedCount,
                SignedBiomassGram = signedBiomassGram,
                FeedGram = null,
                ActorUserId = userId,
                ReferenceTable = referenceTable,
                ReferenceId = referenceId,
                CreatedBy = userId,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            });
        }
    }
}
