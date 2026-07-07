using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Transfers.Application.Services
{
    public class TransferLineService : ITransferLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public TransferLineService(
            IUnitOfWork unitOfWork,
            ITransferService transferService,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _transferService = transferService;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<TransferLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.TransferLines
                    .Query()
                    .Include(x => x.FishBatch)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Include(x => x.ToProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ToProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<TransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = MapTransferLine(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<TransferLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.TransferLines
                    .Query()
                    .Include(x => x.FishBatch)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Include(x => x.ToProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ToProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(TransferLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(MapTransferLine).ToList();

                var pagedResponse = new PagedResponse<TransferLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<TransferLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<TransferLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private TransferLineDto MapTransferLine(TransferLine entity)
        {
            var dto = _mapper.Map<TransferLineDto>(entity);
            dto.BatchCode = entity.FishBatch?.BatchCode;
            dto.FromProjectCode = entity.FromProjectCage?.Project?.ProjectCode;
            dto.FromProjectName = entity.FromProjectCage?.Project?.ProjectName;
            dto.FromCageCode = entity.FromProjectCage?.Cage?.CageCode;
            dto.FromCageName = entity.FromProjectCage?.Cage?.CageName;
            dto.ToProjectCode = entity.ToProjectCage?.Project?.ProjectCode;
            dto.ToProjectName = entity.ToProjectCage?.Project?.ProjectName;
            dto.ToCageCode = entity.ToProjectCage?.Cage?.CageCode;
            dto.ToCageName = entity.ToProjectCage?.Cage?.CageName;
            return dto;
        }

        public async Task<ApiResponse<TransferLineDto>> CreateAsync(CreateTransferLineDto dto)
        {
            try
            {
                var entity = _mapper.Map<TransferLine>(dto);
                await _unitOfWork.TransferLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var transfer = await _unitOfWork.Transfers
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == entity.TransferId && !x.IsDeleted);
                if (transfer != null && transfer.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? transfer.CreatedBy ?? 1L;
                    var postResult = await _transferService.Post(transfer.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferLineDto>> CreateWithAutoHeaderAsync(CreateTransferLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transfer = await _unitOfWork.Transfers
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.TransferDate.Date == dto.TransferDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (transfer == null)
                {
                    var project = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    transfer = new Transfer
                    {
                        ProjectId = dto.ProjectId,
                        TransferDate = dto.TransferDate,
                        Status = DocumentStatus.Draft,
                        TransferNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Transfers.AddAsync(transfer);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new TransferLine
                {
                    TransferId = transfer.Id,
                    FishBatchId = dto.FishBatchId,
                    FromProjectCageId = dto.FromProjectCageId,
                    ToProjectCageId = dto.ToProjectCageId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.TransferLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (transfer.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? transfer.CreatedBy ?? 1L;
                    var postResult = await _transferService.Post(transfer.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferLineDto>> UpdateAsync(long id, UpdateTransferLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.TransferLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<TransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, long? userId = null)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var line = await _unitOfWork.TransferLines
                    .Query(tracking: true)
                    .Include(x => x.Transfer)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (line == null || line.Transfer == null || line.Transfer.IsDeleted)
                {
                    await _unitOfWork.Rollback();
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var transfer = line.Transfer;
                var hasPostedLedger = await _unitOfWork.Db.BatchMovements.AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ReferenceTable == "RII_TRANSFER" &&
                    x.ReferenceId == transfer.Id);

                if (transfer.Status == DocumentStatus.Posted || hasPostedLedger)
                {
                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToProjectCageId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.Transfer,
                        transfer.TransferDate,
                        "Transfer line cancellation out",
                        "RII_TRANSFER",
                        transfer.Id,
                        line.ToProjectCageId,
                        line.FromProjectCageId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);

                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromProjectCageId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.Transfer,
                        transfer.TransferDate,
                        "Transfer line cancellation in",
                        "RII_TRANSFER",
                        transfer.Id,
                        line.ToProjectCageId,
                        line.FromProjectCageId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);

                    var sourceCage = await _unitOfWork.Db.ProjectCages
                        .FirstOrDefaultAsync(x => x.Id == line.FromProjectCageId && !x.IsDeleted && x.ReleasedDate != null);
                    if (sourceCage != null)
                    {
                        var hasLiveBalance = await _unitOfWork.Db.BatchCageBalances
                            .AnyAsync(x => x.ProjectCageId == sourceCage.Id && !x.IsDeleted && x.LiveCount > 0);
                        if (hasLiveBalance)
                        {
                            sourceCage.ReleasedDate = null;
                            sourceCage.UpdatedBy = userId;
                            sourceCage.UpdatedDate = DateTimeProvider.UtcNow;
                        }
                    }
                }

                line.IsDeleted = true;
                line.DeletedBy = userId;
                line.DeletedDate = DateTimeProvider.UtcNow;

                var hasRemainingLines = await _unitOfWork.Db.TransferLines.AnyAsync(x =>
                    x.TransferId == transfer.Id && x.Id != line.Id && !x.IsDeleted);
                if (!hasRemainingLines)
                {
                    transfer.Status = DocumentStatus.Cancelled;
                    transfer.IsDeleted = true;
                    transfer.DeletedBy = userId;
                    transfer.DeletedDate = DateTimeProvider.UtcNow;
                    transfer.UpdatedBy = userId;
                    transfer.UpdatedDate = DateTimeProvider.UtcNow;
                }

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "DOC" : baseValue.Trim();
            return $"{normalized}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
