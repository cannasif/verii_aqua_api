using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Transfers.Application.Services
{
    public class CageWarehouseTransferLineService : ICageWarehouseTransferLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public CageWarehouseTransferLineService(
            IUnitOfWork unitOfWork,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<CageWarehouseTransferLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<CageWarehouseTransferLine>()
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<CageWarehouseTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<CageWarehouseTransferLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<CageWarehouseTransferLine>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CageWarehouseTransferLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var items = new List<CageWarehouseTransferLineDto>(entities.Count);
                foreach (var entity in entities)
                {
                    items.Add(await MapDtoAsync(entity));
                }

                return ApiResponse<PagedResponse<CageWarehouseTransferLineDto>>.SuccessResult(
                    new PagedResponse<CageWarehouseTransferLineDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CageWarehouseTransferLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseTransferLineDto>> CreateAsync(CreateCageWarehouseTransferLineDto dto)
        {
            try
            {
                await NormalizeAsync(dto.FromProjectCageId, dto.ToWarehouseId);

                var entity = _mapper.Map<CageWarehouseTransferLine>(dto);
                await _unitOfWork.Repository<CageWarehouseTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<CageWarehouseTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseTransferLineDto>> CreateWithAutoHeaderAsync(CreateCageWarehouseTransferLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await NormalizeAsync(dto.FromProjectCageId, dto.ToWarehouseId);

                var header = await _unitOfWork.Repository<CageWarehouseTransfer>()
                    .Query()
                    .Where(x => !x.IsDeleted && x.ProjectId == dto.ProjectId && x.Status == DocumentStatus.Draft && x.TransferDate.Date == dto.TransferDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (header == null)
                {
                    var project = await _unitOfWork.Projects.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);
                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    header = new CageWarehouseTransfer
                    {
                        ProjectId = dto.ProjectId,
                        TransferDate = dto.TransferDate,
                        Status = DocumentStatus.Draft,
                        TransferNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Repository<CageWarehouseTransfer>().AddAsync(header);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new CageWarehouseTransferLine
                {
                    CageWarehouseTransferId = header.Id,
                    FishBatchId = dto.FishBatchId,
                    FromProjectCageId = dto.FromProjectCageId,
                    ToWarehouseId = dto.ToWarehouseId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.Repository<CageWarehouseTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<CageWarehouseTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseTransferLineDto>> UpdateAsync(long id, UpdateCageWarehouseTransferLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<CageWarehouseTransferLine>();
                var entity = await repo.GetByIdForUpdateAsync(id);
                if (entity == null)
                {
                    return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await NormalizeAsync(dto.FromProjectCageId, dto.ToWarehouseId);

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<CageWarehouseTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, long? userId = null)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var line = await _unitOfWork.Repository<CageWarehouseTransferLine>()
                    .Query(tracking: true)
                    .Include(x => x.CageWarehouseTransfer)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (line == null || line.CageWarehouseTransfer == null || line.CageWarehouseTransfer.IsDeleted)
                {
                    await _unitOfWork.Rollback();
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var transfer = line.CageWarehouseTransfer;
                var hasPostedLedger = await _unitOfWork.Db.BatchMovements.AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ReferenceTable == "RII_CAGE_WAREHOUSE_TRANSFER" &&
                    x.ReferenceId == transfer.Id);

                if (transfer.Status == DocumentStatus.Posted || hasPostedLedger)
                {
                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToWarehouseId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Cage to warehouse transfer line cancellation - warehouse out",
                        "RII_CAGE_WAREHOUSE_TRANSFER",
                        transfer.Id,
                        line.ToWarehouseId,
                        null,
                        null,
                        null,
                        line.AverageGram,
                        null,
                        userId);

                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromProjectCageId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Cage to warehouse transfer line cancellation - cage in",
                        "RII_CAGE_WAREHOUSE_TRANSFER",
                        transfer.Id,
                        null,
                        line.FromProjectCageId,
                        null,
                        null,
                        null,
                        line.AverageGram,
                        userId);
                }

                line.IsDeleted = true;
                line.DeletedBy = userId;
                line.DeletedDate = DateTimeProvider.UtcNow;

                var hasRemainingLines = await _unitOfWork.Db.CageWarehouseTransferLines.AnyAsync(x =>
                    x.CageWarehouseTransferId == transfer.Id && x.Id != line.Id && !x.IsDeleted);
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
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("CageWarehouseTransferLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "CWT" : baseValue.Trim();
            return $"{normalized}-CW-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }

        private async Task NormalizeAsync(long fromProjectCageId, long toWarehouseId)
        {
            await EnsureProjectCageExistsAsync(fromProjectCageId);
            await EnsureWarehouseExistsAsync(toWarehouseId);
        }

        private async Task EnsureProjectCageExistsAsync(long projectCageId)
        {
            var exists = await _unitOfWork.ProjectCages
                .Query()
                .AnyAsync(x => !x.IsDeleted && x.Id == projectCageId);

            if (!exists)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("TransferService.SourceProjectCageNotFound"));
            }
        }

        private async Task EnsureWarehouseExistsAsync(long warehouseId)
        {
            var exists = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AnyAsync(x => !x.IsDeleted && x.Id == warehouseId);

            if (!exists)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferLineService.WarehouseNotFound"));
            }
        }

        private async Task<CageWarehouseTransferLineDto> MapDtoAsync(CageWarehouseTransferLine entity)
        {
            var dto = _mapper.Map<CageWarehouseTransferLineDto>(entity);
            var fishBatch = await _unitOfWork.FishBatches
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.FishBatchId);

            var sourceProjectCage = await _unitOfWork.ProjectCages
                .Query()
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.Cage)
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.FromProjectCageId);

            var warehouse = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.ToWarehouseId);

            dto.BatchCode = fishBatch?.BatchCode;
            dto.FromProjectCode = sourceProjectCage?.Project?.ProjectCode;
            dto.FromProjectName = sourceProjectCage?.Project?.ProjectName;
            dto.FromCageCode = sourceProjectCage?.Cage?.CageCode;
            dto.FromCageName = sourceProjectCage?.Cage?.CageName;
            dto.ToWarehouseCode = warehouse?.ErpWarehouseCode;
            dto.ToWarehouseName = warehouse?.WarehouseName;

            return dto;
        }
    }
}
