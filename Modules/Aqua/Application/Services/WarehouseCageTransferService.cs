using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WarehouseCageTransferService : IWarehouseCageTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WarehouseCageTransferService(
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

        public async Task<ApiResponse<WarehouseCageTransferDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<WarehouseCageTransfer>()
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WarehouseCageTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<WarehouseCageTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseCageTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WarehouseCageTransferDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<WarehouseCageTransfer>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WarehouseCageTransfer.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                return ApiResponse<PagedResponse<WarehouseCageTransferDto>>.SuccessResult(
                    new PagedResponse<WarehouseCageTransferDto>
                    {
                        Items = entities.Select(_mapper.Map<WarehouseCageTransferDto>).ToList(),
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WarehouseCageTransferDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseCageTransferDto>> CreateAsync(CreateWarehouseCageTransferDto dto)
        {
            try
            {
                var entity = _mapper.Map<WarehouseCageTransfer>(dto);
                await _unitOfWork.Repository<WarehouseCageTransfer>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseCageTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseCageTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseCageTransferDto>> UpdateAsync(long id, UpdateWarehouseCageTransferDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<WarehouseCageTransfer>();
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<WarehouseCageTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseCageTransferDto>.SuccessResult(
                    _mapper.Map<WarehouseCageTransferDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<WarehouseCageTransfer>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long warehouseCageTransferId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var transfer = await _unitOfWork.Repository<WarehouseCageTransfer>()
                    .Query()
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == warehouseCageTransferId && !x.IsDeleted);

                if (transfer == null)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseCageTransferService.NotFound"));
                }

                if (transfer.Status != DocumentStatus.Draft)
                {
                    throw new InvalidOperationException("WarehouseCageTransfer must be in Draft status.");
                }

                var lines = transfer.Lines.Where(x => !x.IsDeleted).ToList();
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseCageTransferService.MustContainLines"));
                }

                foreach (var line in lines)
                {
                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromWarehouseId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Warehouse to cage transfer out",
                        "RII_WarehouseCageTransfer",
                        transfer.Id,
                        line.FromWarehouseId,
                        null,
                        null,
                        null,
                        line.AverageGram,
                        null,
                        userId);

                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToProjectCageId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Warehouse to cage transfer in",
                        "RII_WarehouseCageTransfer",
                        transfer.Id,
                        null,
                        line.ToProjectCageId,
                        null,
                        null,
                        null,
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
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
