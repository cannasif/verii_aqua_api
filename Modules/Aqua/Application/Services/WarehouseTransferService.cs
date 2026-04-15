using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WarehouseTransferService : IWarehouseTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WarehouseTransferService(
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

        public async Task<ApiResponse<WarehouseTransferDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<WarehouseTransfer>()
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WarehouseTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<WarehouseTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WarehouseTransferDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<WarehouseTransfer>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WarehouseTransfer.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                return ApiResponse<PagedResponse<WarehouseTransferDto>>.SuccessResult(
                    new PagedResponse<WarehouseTransferDto>
                    {
                        Items = entities.Select(_mapper.Map<WarehouseTransferDto>).ToList(),
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WarehouseTransferDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseTransferDto>> CreateAsync(CreateWarehouseTransferDto dto)
        {
            try
            {
                var entity = _mapper.Map<WarehouseTransfer>(dto);
                await _unitOfWork.Repository<WarehouseTransfer>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseTransferDto>> UpdateAsync(long id, UpdateWarehouseTransferDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<WarehouseTransfer>();
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<WarehouseTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<WarehouseTransfer>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long warehouseTransferId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var transfer = await _unitOfWork.Repository<WarehouseTransfer>()
                    .Query()
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == warehouseTransferId && !x.IsDeleted);

                if (transfer == null)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferService.NotFound"));
                }

                EnsureDraftStatus(transfer.Status);

                var lines = transfer.Lines.Where(x => !x.IsDeleted).ToList();
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferService.MustContainLines"));
                }

                foreach (var line in lines)
                {
                    if (line.FromWarehouseId == line.ToWarehouseId)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferLineService.SourceAndTargetWarehouseCannotBeSame"));
                    }

                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromWarehouseId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Warehouse transfer out",
                        "RII_WarehouseTransfer",
                        transfer.Id,
                        line.FromWarehouseId,
                        line.ToWarehouseId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);

                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToWarehouseId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Warehouse transfer in",
                        "RII_WarehouseTransfer",
                        transfer.Id,
                        line.FromWarehouseId,
                        line.ToWarehouseId,
                        null,
                        null,
                        line.AverageGram,
                        line.AverageGram,
                        userId);
                }

                transfer.Status = DocumentStatus.Posted;
                transfer.UpdatedBy = userId;
                transfer.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("WarehouseTransferService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureDraftStatus(DocumentStatus status)
        {
            if (status != DocumentStatus.Draft)
            {
                throw new InvalidOperationException("WarehouseTransfer must be in Draft status.");
            }
        }
    }
}
