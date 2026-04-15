using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class CageWarehouseTransferService : ICageWarehouseTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public CageWarehouseTransferService(
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

        public async Task<ApiResponse<CageWarehouseTransferDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<CageWarehouseTransfer>()
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<CageWarehouseTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<CageWarehouseTransferDto>.SuccessResult(
                    _mapper.Map<CageWarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<CageWarehouseTransferDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<CageWarehouseTransfer>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CageWarehouseTransfer.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                return ApiResponse<PagedResponse<CageWarehouseTransferDto>>.SuccessResult(
                    new PagedResponse<CageWarehouseTransferDto>
                    {
                        Items = entities.Select(_mapper.Map<CageWarehouseTransferDto>).ToList(),
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CageWarehouseTransferDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseTransferDto>> CreateAsync(CreateCageWarehouseTransferDto dto)
        {
            try
            {
                var entity = _mapper.Map<CageWarehouseTransfer>(dto);
                await _unitOfWork.Repository<CageWarehouseTransfer>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<CageWarehouseTransferDto>.SuccessResult(
                    _mapper.Map<CageWarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseTransferDto>> UpdateAsync(long id, UpdateCageWarehouseTransferDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<CageWarehouseTransfer>();
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<CageWarehouseTransferDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<CageWarehouseTransferDto>.SuccessResult(
                    _mapper.Map<CageWarehouseTransferDto>(entity),
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageWarehouseTransferDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<CageWarehouseTransfer>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long cageWarehouseTransferId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var transfer = await _unitOfWork.Repository<CageWarehouseTransfer>()
                    .Query()
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == cageWarehouseTransferId && !x.IsDeleted);

                if (transfer == null)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("CageWarehouseTransferService.NotFound"));
                }

                if (transfer.Status != DocumentStatus.Draft)
                {
                    throw new InvalidOperationException("CageWarehouseTransfer must be in Draft status.");
                }

                var lines = transfer.Lines.Where(x => !x.IsDeleted).ToList();
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("CageWarehouseTransferService.MustContainLines"));
                }

                foreach (var line in lines)
                {
                    await _balanceLedgerManager.ApplyDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.FromProjectCageId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Cage to warehouse transfer out",
                        "RII_CageWarehouseTransfer",
                        transfer.Id,
                        line.FromProjectCageId,
                        null,
                        null,
                        null,
                        line.AverageGram,
                        null,
                        userId);

                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        transfer.ProjectId,
                        line.FishBatchId,
                        line.ToWarehouseId,
                        line.FishCount,
                        line.BiomassGram,
                        BatchMovementType.WarehouseTransfer,
                        transfer.TransferDate,
                        "Cage to warehouse transfer in",
                        "RII_CageWarehouseTransfer",
                        transfer.Id,
                        null,
                        line.ToWarehouseId,
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
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseTransferService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
