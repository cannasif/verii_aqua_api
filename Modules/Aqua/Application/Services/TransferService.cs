using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferRepository _transferRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public TransferService(
            IUnitOfWork unitOfWork,
            ITransferRepository transferRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<TransferDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Transfers
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<TransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<TransferDto>(entity);
                return ApiResponse<TransferDto>.SuccessResult(dto, _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<TransferDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Transfers
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Transfer.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<TransferDto>(x)).ToList();

                var pagedResponse = new PagedResponse<TransferDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<TransferDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<TransferDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferDto>> CreateAsync(CreateTransferDto dto)
        {
            try
            {
                var entity = _mapper.Map<Transfer>(dto);
                await _unitOfWork.Transfers.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<TransferDto>(entity);
                return ApiResponse<TransferDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferDto>> UpdateAsync(long id, UpdateTransferDto dto)
        {
            try
            {
                var repo = _unitOfWork.Transfers;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<TransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<TransferDto>(entity);
                return ApiResponse<TransferDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Transfers;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        _localizationService.GetLocalizedString("TransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long transferId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var transfer = await _transferRepository.GetForPost(transferId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("TransferService.TransferNotFound"));

                EnsureDraftStatus(transfer.Status, nameof(Transfer));
                if (!transfer.Lines.Any(x => !x.IsDeleted))
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("TransferService.MustContainLines"));

                await EnsureTransferSettingsAsync(transfer);

                var sourceProjectCageIds = new HashSet<long>();
                foreach (var line in transfer.Lines.Where(x => !x.IsDeleted))
                {
                    if (line.FromProjectCageId == line.ToProjectCageId)
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("TransferService.SourceAndTargetCageCannotBeSame"));
                    sourceProjectCageIds.Add(line.FromProjectCageId);

                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromProjectCageId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.Transfer,
                        transfer.TransferDate,
                        "Transfer out",
                        "RII_Transfer",
                        transfer.Id,
                        line.FromProjectCageId,
                        line.ToProjectCageId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);

                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToProjectCageId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.Transfer,
                        transfer.TransferDate,
                        "Transfer in",
                        "RII_Transfer",
                        transfer.Id,
                        line.FromProjectCageId,
                        line.ToProjectCageId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);
                }

                if (sourceProjectCageIds.Count > 0)
                {
                    var sourceCages = await _unitOfWork.Db.ProjectCages
                        .Where(x => sourceProjectCageIds.Contains(x.Id) && !x.IsDeleted && x.ReleasedDate == null)
                        .ToListAsync();

                    foreach (var sourceCage in sourceCages)
                    {
                        var hasLiveBalance = await _unitOfWork.Db.BatchCageBalances
                            .AnyAsync(x => x.ProjectCageId == sourceCage.Id && !x.IsDeleted && x.LiveCount > 0);
                        if (hasLiveBalance)
                        {
                            continue;
                        }

                        var releaseDate = transfer.TransferDate.Date < sourceCage.AssignedDate.Date
                            ? sourceCage.AssignedDate.Date
                            : transfer.TransferDate.Date;
                        sourceCage.ReleasedDate = releaseDate;
                        sourceCage.UpdatedBy = userId;
                        sourceCage.UpdatedDate = DateTimeProvider.UtcNow;
                    }
                }

                transfer.Status = DocumentStatus.Posted;
                transfer.UpdatedBy = userId;
                transfer.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("TransferService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
        }

        private async Task EnsureTransferSettingsAsync(Transfer transfer)
        {
            var settings = await _unitOfWork.AquaSettings
                .Query()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            var mode = settings?.PartialTransferOccupiedCageMode ?? 0;
            if (mode == 2)
            {
                return;
            }

            var activeLines = transfer.Lines.Where(x => !x.IsDeleted).ToList();
            var sourceGroups = activeLines
                .GroupBy(x => new { x.FishBatchId, x.FromProjectCageId })
                .ToList();

            foreach (var group in sourceGroups)
            {
                var sourceBalance = await _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .Where(x =>
                        x.FishBatchId == group.Key.FishBatchId &&
                        x.ProjectCageId == group.Key.FromProjectCageId &&
                        !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                var currentLiveCount = sourceBalance?.LiveCount ?? 0;
                var totalTransferCount = group.Sum(x => x.FishCount);
                var isPartialTransfer = totalTransferCount < currentLiveCount;

                if (!isPartialTransfer)
                {
                    continue;
                }

                foreach (var line in group)
                {
                    var targetLiveBalances = await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .Where(x => x.ProjectCageId == line.ToProjectCageId && !x.IsDeleted && x.LiveCount > 0)
                        .ToListAsync();

                    if (targetLiveBalances.Count == 0)
                    {
                        continue;
                    }

                    if (mode == 0)
                    {
                        throw new InvalidOperationException("Dolu kafese kismi transfer yapilamaz.");
                    }

                    var onlySameBatchExists = targetLiveBalances.All(x => x.FishBatchId == line.FishBatchId);
                    if (!onlySameBatchExists)
                    {
                        throw new InvalidOperationException("Dolu kafese kismi transfer yalnizca ayni batch icin yapilabilir.");
                    }
                }
            }
        }

    }
}
